using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

/// <summary>
/// Alternative claw movement script that handles 4-directional movement (left, right, forward, back)
/// Designed to work with JoystickMovementScript without modifying the original claw functionality
/// </summary>
public class ClawMovementScript : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float slideBackSpeed = 1.5f;
    
    [Header("Machine Bounds")]
    [Tooltip("The bounds of the machine in local space")]
    public Vector2 machineBoundsX = new Vector2(-1f, 1f);
    public Vector2 machineBoundsZ = new Vector2(-1f, 1f);
    
    [Header("References")]
    public Transform homePosition;
    public Transform clawGrabPoint;
    
    [Header("Drop Settings")]
    public float dropDist = 2f;
    public float dropSpeed = 2f;
    public float bottomWaitTime = 0.3f;
    
    [Header("Haptic Feedback")]
    public bool enableHaptics = true;
    [Range(0f, 1f)]
    public float movementHapticIntensity = 0.2f;
    public float movementHapticDuration = 0.1f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    // Events
    public UnityEvent OnClawMoveStart;
    public UnityEvent OnClawMoveEnd;
    public UnityEvent<GameObject> OnToyGrabbed;
    public UnityEvent<GameObject> OnToyReleased;
    
    // Movement state
    private Vector2 moveInput = Vector2.zero;
    private bool isMoving = false;
    private bool isDropping = false;
    private Vector3 startLocalPos;
    private Vector3 currentLocalPos;
    
    // References
    private GameObject grabbedToy = null;
    
    void Start()
    {
        if (homePosition == null)
        {
            // Use current position as home if none specified
            homePosition = new GameObject("HomePosition").transform;
            homePosition.SetParent(transform.parent);
            homePosition.position = transform.position;
            
            if (showDebugInfo)
                Debug.Log("Created home position at current location");
        }
        
        // Create grab point if needed
        if (clawGrabPoint == null)
        {
            GameObject grabObj = new GameObject("GrabPoint");
            clawGrabPoint = grabObj.transform;
            clawGrabPoint.SetParent(transform);
            clawGrabPoint.localPosition = new Vector3(0, -0.05f, 0);
            
            if (showDebugInfo)
                Debug.Log("Created grab point at bottom of claw");
        }
        
        // Initialize position tracking
        startLocalPos = transform.localPosition;
        currentLocalPos = startLocalPos;
    }
    
    void Update()
    {
        if (!isDropping && moveInput.sqrMagnitude > 0.01f)
        {
            MoveClaw(moveInput);
        }
        
        // Visualize movement bounds if debugging
        if (showDebugInfo)
        {
            DrawDebugBounds();
        }
    }
    
    /// <summary>
    /// Set the movement input direction from joystick
    /// </summary>
    /// <param name="input">Input vector where x = left/right, y = forward/back</param>
    public void SetMovementInput(Vector2 input)
    {
        // Store previous input for change detection
        Vector2 prevInput = moveInput;
        
        // Set new input
        moveInput = input;
        
        // Movement state tracking for events
        bool wasMoving = isMoving;
        isMoving = moveInput.sqrMagnitude > 0.01f;
        
        // Fire events on state change
        if (!wasMoving && isMoving)
        {
            OnClawMoveStart?.Invoke();
        }
        else if (wasMoving && !isMoving)
        {
            OnClawMoveEnd?.Invoke();
        }
        
        // Log changes if debugging
        if (showDebugInfo && moveInput != prevInput)
        {
            Debug.Log($"Claw movement input: {moveInput}");
        }
    }
    
    /// <summary>
    /// Move the claw based on input vector
    /// </summary>
    private void MoveClaw(Vector2 direction)
    {
        // Calculate new position with movement
        Vector3 newPos = currentLocalPos;
        
        // X is left/right
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            newPos.x += direction.x * moveSpeed * Time.deltaTime;
        }
        
        // Y is forward/back (maps to Z in world space)
        if (Mathf.Abs(direction.y) > 0.01f)
        {
            newPos.z += direction.y * moveSpeed * Time.deltaTime;
        }
        
        // Clamp to machine bounds
        newPos.x = Mathf.Clamp(newPos.x, machineBoundsX.x, machineBoundsX.y);
        newPos.z = Mathf.Clamp(newPos.z, machineBoundsZ.x, machineBoundsZ.y);
        newPos.y = startLocalPos.y; // Keep at constant height
        
        // Apply the new position
        currentLocalPos = newPos;
        transform.localPosition = currentLocalPos;
        
        // Ensure physics updates
        Physics.SyncTransforms();
    }
    
    /// <summary>
    /// Start the drop sequence to attempt to grab a toy
    /// </summary>
    public void Drop()
    {
        if (!isDropping)
        {
            StartCoroutine(DropRoutine());
        }
    }
    
    /// <summary>
    /// Return the claw to the home position and release any grabbed toy
    /// </summary>
    public void ReturnHome()
    {
        if (!isDropping)
        {
            StartCoroutine(ReturnHomeRoutine());
        }
    }
    
    /// <summary>
    /// Check if the claw can currently be controlled
    /// </summary>
    public bool CanControl()
    {
        return !isDropping;
    }
    
    /// <summary>
    /// Coroutine for dropping the claw to grab toys
    /// </summary>
    private IEnumerator DropRoutine()
    {
        isDropping = true;
        
        // Store starting position
        Vector3 dropStartPos = currentLocalPos;
        Vector3 dropTargetPos = dropStartPos + Vector3.down * dropDist;
        
        // Log the drop start if debugging
        if (showDebugInfo)
        {
            Debug.Log($"Dropping claw from {transform.position.y} to {transform.position.y - dropDist}");
        }
        
        // Lower the claw
        while (currentLocalPos.y > dropTargetPos.y)
        {
            currentLocalPos += Vector3.down * dropSpeed * Time.deltaTime;
            transform.localPosition = currentLocalPos;
            yield return null;
        }
        
        // Ensure we reach the target position exactly
        currentLocalPos.y = dropTargetPos.y;
        transform.localPosition = currentLocalPos;
        
        // Try to grab a toy
        TryGrabToy();
        
        // Wait at the bottom to allow physics to settle
        yield return new WaitForSeconds(bottomWaitTime);
        
        // Try again if we failed the first time
        if (grabbedToy == null)
        {
            TryGrabToy();
        }
        
        // Raise the claw back up
        while (currentLocalPos.y < startLocalPos.y)
        {
            currentLocalPos += Vector3.up * dropSpeed * Time.deltaTime;
            transform.localPosition = currentLocalPos;
            yield return null;
        }
        
        // Ensure we're at the right height
        currentLocalPos.y = startLocalPos.y;
        transform.localPosition = currentLocalPos;
        
        // Now the claw is back at the top with or without a toy
        isDropping = false;
    }
    
    /// <summary>
    /// Coroutine to return the claw to home position
    /// </summary>
    private IEnumerator ReturnHomeRoutine()
    {
        isDropping = true;
        
        // Get the horizontal distance to home
        Vector3 targetPos = homePosition.localPosition;
        targetPos.y = startLocalPos.y; // Keep same height
        
        float totalDistance = Vector3.Distance(
            new Vector3(currentLocalPos.x, 0, currentLocalPos.z),
            new Vector3(targetPos.x, 0, targetPos.z)
        );
        
        if (showDebugInfo)
        {
            Debug.Log($"Returning to home. Distance: {totalDistance}");
        }
        
        // Skip if we're already at home
        if (totalDistance < 0.01f)
        {
            currentLocalPos = targetPos;
            transform.localPosition = currentLocalPos;
        }
        else
        {
            // Slide back to home position
            while (totalDistance > 0.01f)
            {
                // Calculate direction to target (horizontal only)
                Vector3 toTarget = targetPos - currentLocalPos;
                toTarget.y = 0;
                Vector3 direction = toTarget.normalized;
                
                // Calculate move distance this frame
                float moveDistance = slideBackSpeed * Time.deltaTime;
                moveDistance = Mathf.Min(moveDistance, totalDistance);
                
                // Move towards target
                currentLocalPos += direction * moveDistance;
                currentLocalPos.y = startLocalPos.y; // Keep height constant
                
                // Apply the position
                transform.localPosition = currentLocalPos;
                
                // Recalculate distance
                totalDistance = Vector3.Distance(
                    new Vector3(currentLocalPos.x, 0, currentLocalPos.z),
                    new Vector3(targetPos.x, 0, targetPos.z)
                );
                
                yield return null;
            }
            
            // Ensure we're exactly at the target
            currentLocalPos = targetPos;
            transform.localPosition = currentLocalPos;
        }
        
        // Release any grabbed toy
        if (grabbedToy != null)
        {
            ReleaseGrabbedToy();
        }
        
        isDropping = false;
    }
    
    /// <summary>
    /// Try to grab a toy under the claw
    /// </summary>
    private void TryGrabToy()
    {
        // Simple implementation using a raycast
        RaycastHit hit;
        if (Physics.Raycast(clawGrabPoint.position, Vector3.down, out hit, grabRadius, toyLayers))
        {
            // Check if we hit a toy
            GameObject hitObject = hit.collider.gameObject;
            
            // Skip if it's part of the claw
            if (hitObject.transform.IsChildOf(transform))
                return;
            
            // Get the rigidbody from the hit object or its parent
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb == null)
                rb = hit.collider.GetComponentInParent<Rigidbody>();
            
            if (rb != null)
            {
                // This is a valid toy with physics
                grabbedToy = rb.gameObject;
                
                // Make the toy kinematic and attach it to the grab point
                rb.isKinematic = true;
                rb.useGravity = false;
                
                // Parent to grab point
                Transform originalParent = rb.transform.parent;
                rb.transform.SetParent(clawGrabPoint);
                
                // Fire grabbed event
                OnToyGrabbed?.Invoke(grabbedToy);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Grabbed toy: {grabbedToy.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Release currently grabbed toy
    /// </summary>
    private void ReleaseGrabbedToy()
    {
        if (grabbedToy == null) return;
        
        // Get the rigidbody
        Rigidbody rb = grabbedToy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Unparent from claw
            rb.transform.SetParent(null);
            
            // Restore physics
            rb.isKinematic = false;
            rb.useGravity = true;
            
            // Add a small downward force to start falling
            rb.AddForce(Vector3.down * 0.5f, ForceMode.Impulse);
        }
        
        // Fire event
        OnToyReleased?.Invoke(grabbedToy);
        
        if (showDebugInfo)
        {
            Debug.Log($"Released toy: {grabbedToy.name}");
        }
        
        // Clear reference
        grabbedToy = null;
    }
    
    // Variables needed for toy grabbing
    private float grabRadius = 0.15f;
    private LayerMask toyLayers = -1;
    
    /// <summary>
    /// Draw debug visualization of the movement bounds
    /// </summary>
    private void DrawDebugBounds()
    {
        // Get the world position for the bounds
        Vector3 center = transform.parent != null ? transform.parent.position : Vector3.zero;
        float y = startLocalPos.y;
        
        // Draw the bounds as a rectangle
        Debug.DrawLine(
            new Vector3(machineBoundsX.x + center.x, y, machineBoundsZ.x + center.z),
            new Vector3(machineBoundsX.y + center.x, y, machineBoundsZ.x + center.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(machineBoundsX.y + center.x, y, machineBoundsZ.x + center.z),
            new Vector3(machineBoundsX.y + center.x, y, machineBoundsZ.y + center.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(machineBoundsX.y + center.x, y, machineBoundsZ.y + center.z),
            new Vector3(machineBoundsX.x + center.x, y, machineBoundsZ.y + center.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(machineBoundsX.x + center.x, y, machineBoundsZ.y + center.z),
            new Vector3(machineBoundsX.x + center.x, y, machineBoundsZ.x + center.z),
            Color.red
        );
        
        // Draw movement indicator if moving
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Debug.DrawRay(transform.position, new Vector3(moveInput.x, 0, moveInput.y) * 0.5f, Color.blue);
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw the grab radius
        if (clawGrabPoint != null)
        {
            Gizmos.color = grabbedToy != null ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(clawGrabPoint.position, grabRadius);
        }
        
        // Draw a line to grabbed toy
        if (grabbedToy != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, grabbedToy.transform.position);
        }
    }
} 