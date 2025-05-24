using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events; // Add this for UnityEvent

public class ClawScript : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float slideBackSpeed = 1.5f; // Speed for sliding back to initial position
    public float dropDist = 2f;
    public float dropSpeed = 2f;
    
    [Header("Machine Bounds")]
    [Tooltip("The bounds of the machine in local space")]
    public Vector2 machineBoundsX = new Vector2(-1f, 1f);
    public Vector2 machineBoundsZ = new Vector2(-1f, 1f);
    public float machineHeight = 2f;
    
    [Header("Return Position")]
    [Tooltip("Should the claw return to its initial position after dropping")]
    public bool returnToInitialPosition = true;
    
    [Tooltip("The position the claw should return to after dropping (if null, uses starting position)")]
    public Transform returnPosition;
    
    [Header("XR Socket Interaction")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor; // Reference to the socket interactor
    
    [Header("Haptic Feedback")]
    public bool enableHaptics = true;
    [Range(0f, 1f)]
    public float grabHapticIntensity = 0.8f;
    public float grabHapticDuration = 0.3f;
    [Range(0f, 1f)]
    public float dropHapticIntensity = 0.4f;
    public float dropHapticDuration = 0.2f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool useLocalCoordinates = true;
    
    // Add this event for UI integration
    public UnityEvent<GameObject> OnDropCompleted = new UnityEvent<GameObject>();
    
    // For tracking socket interaction
    private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable socketedInteractable = null;
    
    Vector3 moveDir;
    bool isDropping;
    Vector3 startLocalPos;
    Vector3 currentLocalPos;
    
    // Reference to the joystick controller for haptic feedback
    private XRJoystickController joystickController;
    
    void Start()
    {
        startLocalPos = transform.localPosition;
        currentLocalPos = startLocalPos;
        
        // Try to find the joystick controller for haptic feedback
        joystickController = FindFirstObjectByType<XRJoystickController>();
        
        // Setup socket interactor if not assigned
        if (socketInteractor == null)
        {
            // Try to find socket interactor on this object or its children
            socketInteractor = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            
            if (socketInteractor == null)
            {
                Debug.LogError("Socket interactor not found! Please assign a socket interactor in the inspector.");
            }
            else if (showDebugInfo)
            {
                Debug.Log("Found socket interactor: " + socketInteractor.name);
            }
        }
        
        // Subscribe to socket events
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnSocketSelect);
            socketInteractor.selectExited.AddListener(OnSocketRelease);
            
            // Make sure socket is disabled at start
            socketInteractor.socketActive = false;
            
            if (showDebugInfo)
            {
                Debug.Log("Socket interactor initialized and events registered.");
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from socket events
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnSocketSelect);
            socketInteractor.selectExited.RemoveListener(OnSocketRelease);
        }
    }
    
    void Update()
    {
        // Check if we should move the claw (not dropping and has direction)
        if (!isDropping && moveDir != Vector3.zero)
        {
            SlideClaw();
        }
        
        // Debug collider positions
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            DebugColliderPositions();
        }
    }
    
    void LateUpdate()
    {
        // Force synchronize physics transforms after any movement
        Physics.SyncTransforms();
    }
    
    void DebugColliderPositions()
    {
        // Log claw position
        Debug.Log($"Claw Position: {transform.position}, Local: {transform.localPosition}");
        
        // Log socket interactor state
        if (socketInteractor != null)
        {
            var selected = socketInteractor.hasSelection ? "Yes" : "No";
            Debug.Log($"Socket Interactor: Has selection: {selected}, Interactable: {socketedInteractable?.transform?.name ?? "None"}");
            Debug.Log($"Socket Active: {socketInteractor.socketActive}");
        }
        
        // Check if we're in the dropping state
        Debug.Log($"Claw state: isDropping={isDropping}");
    }
    
    private void OnSocketSelect(SelectEnterEventArgs args)
    {
        // Store reference to the selected interactable
        socketedInteractable = args.interactableObject;
        
        // Send haptic feedback when grabbing a toy
        if (enableHaptics)
        {
            SendHapticFeedback(grabHapticIntensity, grabHapticDuration);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Socket grabbed: {socketedInteractable.transform.name}");
        }
    }
    
    private void OnSocketRelease(SelectExitEventArgs args)
    {
        // Fire the drop completed event with the released object
        if (args.interactableObject != null && args.interactableObject.transform != null)
        {
            OnDropCompleted?.Invoke(args.interactableObject.transform.gameObject);
        }
        
        // Clear reference
        socketedInteractable = null;
        
        if (showDebugInfo)
        {
            Debug.Log("Socket released toy");
        }
    }
    
    /// <summary>
    /// Force the socket to release any held toy
    /// </summary>
    public void ForceSocketRelease()
    {
        if (socketInteractor != null && socketInteractor.hasSelection)
        {
            // Store reference to the rigidbody before releasing
            GameObject toyObject = socketedInteractable?.transform?.gameObject;
            Rigidbody toyRb = toyObject?.GetComponent<Rigidbody>();
            
            // Disable the socket to force release
            socketInteractor.socketActive = false;
            
            // Apply downward force to help the toy fall
            if (toyRb != null)
            {
                StartCoroutine(ApplyForceAfterDelay(toyRb));
            }
            
            // Re-enable the socket after a delay
            StartCoroutine(ResetSocketAfterDelay());
            
            if (showDebugInfo)
            {
                Debug.Log($"Forced socket to release toy: {toyObject?.name ?? "unknown"}");
            }
        }
    }
    
    private IEnumerator ApplyForceAfterDelay(Rigidbody rb)
    {
        // Wait a moment for the physics to stabilize
        yield return new WaitForSeconds(0.1f);
        
        if (rb != null)
        {
            // Apply downward force to help the toy fall
            rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
            
            if (showDebugInfo)
            {
                Debug.Log($"Applied downward force to {rb.gameObject.name}");
            }
        }
    }
    
    private IEnumerator ResetSocketAfterDelay()
    {
        // Wait before re-enabling the socket
        yield return new WaitForSeconds(0.5f);
        
        if (socketInteractor != null)
        {
            // Only re-enable if we're not in the middle of a drop
            if (!isDropping)
            {
                socketInteractor.socketActive = true;
                
                if (showDebugInfo)
                {
                    Debug.Log("Re-enabled socket interactor");
                }
            }
        }
    }
    
    void SlideClaw()
    {
        // Calculate new position with movement
        Vector3 newPos = currentLocalPos;
        
        // Apply movement based on direction
        newPos.x += moveDir.x * moveSpeed * Time.deltaTime;
        newPos.z += moveDir.z * moveSpeed * Time.deltaTime;
        
        // Clamp to machine bounds
        newPos.x = Mathf.Clamp(newPos.x, machineBoundsX.x, machineBoundsX.y);
        newPos.z = Mathf.Clamp(newPos.z, machineBoundsZ.x, machineBoundsZ.y);
        
        // Keep Y position constant during sliding
        newPos.y = startLocalPos.y;
        
        // Apply the new position
        currentLocalPos = newPos;
        
        // Apply local or world position based on setting
        if (useLocalCoordinates)
        {
            transform.localPosition = currentLocalPos;
        }
        else
        {
            // Convert local bounds to world position
            Vector3 worldPos = transform.parent.TransformPoint(currentLocalPos);
            transform.position = worldPos;
        }
        
        // Visualize the bounds if debugging
        if (showDebugInfo)
        {
            DrawDebugBounds();
        }
    }
    
    void DrawDebugBounds()
    {
        // Get parent position for offset
        Vector3 parentPos = transform.parent != null ? transform.parent.position : Vector3.zero;
        float y = transform.position.y;
        
        // Draw the bounds as a rectangle
        Debug.DrawLine(
            new Vector3(machineBoundsX.x + parentPos.x, y, machineBoundsZ.x + parentPos.z),
            new Vector3(machineBoundsX.y + parentPos.x, y, machineBoundsZ.x + parentPos.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(machineBoundsX.y + parentPos.x, y, machineBoundsZ.x + parentPos.z),
            new Vector3(machineBoundsX.y + parentPos.x, y, machineBoundsZ.y + parentPos.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(machineBoundsX.y + parentPos.x, y, machineBoundsZ.y + parentPos.z),
            new Vector3(machineBoundsX.x + parentPos.x, y, machineBoundsZ.y + parentPos.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(machineBoundsX.x + parentPos.x, y, machineBoundsZ.y + parentPos.z),
            new Vector3(machineBoundsX.x + parentPos.x, y, machineBoundsZ.x + parentPos.z),
            Color.red
        );
        
        // Draw movement direction
        if (moveDir != Vector3.zero)
        {
            Debug.DrawRay(transform.position, moveDir * 0.5f, Color.blue);
        }
    }
    
    public void SetDirection(Vector3 d)
    {
        // Store the movement direction
        moveDir = d;
        
        // Normalize if needed
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();
        
        // Zero out Y component to prevent vertical movement
        moveDir.y = 0;
        
        if (showDebugInfo && d != Vector3.zero)
        {
            Debug.Log($"Claw direction set to: {moveDir}");
        }
    }
    
    public void StopMovement()
    {
        moveDir = Vector3.zero;
    }
    
    public void Drop()
    {
        if (!isDropping)
        {
            StartCoroutine(DropRoutine());
        }
    }
    
    IEnumerator DropRoutine()
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
            
            // Apply the position
            if (useLocalCoordinates)
            {
                transform.localPosition = currentLocalPos;
            }
            else
            {
                Vector3 worldPos = transform.parent.TransformPoint(currentLocalPos);
                transform.position = worldPos;
            }
            
            yield return null;
        }
        
        // Ensure we reach the target position exactly
        currentLocalPos.y = dropTargetPos.y;
        if (useLocalCoordinates)
        {
            transform.localPosition = currentLocalPos;
        }
        else
        {
            Vector3 worldPos = transform.parent.TransformPoint(currentLocalPos);
            transform.position = worldPos;
        }
        
        // Enable the socket interactor to grab toys
        if (socketInteractor != null)
        {
            socketInteractor.socketActive = true;
            
            if (showDebugInfo)
            {
                Debug.Log("Socket interactor activated at bottom position");
            }
        }
        
        // Wait at the bottom to allow physics to settle and socket to grab
        yield return new WaitForSeconds(0.5f);
        
        // Raise the claw back up
        while (currentLocalPos.y < startLocalPos.y)
        {
            currentLocalPos += Vector3.up * dropSpeed * Time.deltaTime;
            
            // Apply the position
            if (useLocalCoordinates)
            {
                transform.localPosition = currentLocalPos;
            }
            else
            {
                Vector3 worldPos = transform.parent.TransformPoint(currentLocalPos);
                transform.position = worldPos;
            }
            
            yield return null;
        }
        
        // Ensure we're at the right height
        currentLocalPos.y = startLocalPos.y;
        if (useLocalCoordinates)
        {
            transform.localPosition = currentLocalPos;
        }
        else
        {
            Vector3 worldPos = transform.parent.TransformPoint(currentLocalPos);
            transform.position = worldPos;
        }
        
        // If we should return to the initial position, do so now
        if (returnToInitialPosition)
        {
            yield return StartCoroutine(ReturnToInitialPositionRoutine());
        }
        
        // Get the currently grabbed toy (if any)
        GameObject grabbedToy = null;
        if (socketInteractor != null && socketInteractor.hasSelection && socketedInteractable != null)
        {
            grabbedToy = socketedInteractable.transform.gameObject;
        }
        
        // Release any grabbed toy after returning to initial position
        ForceSocketRelease();
        
        // Always invoke the OnDropCompleted event, even if no toy was grabbed
        OnDropCompleted.Invoke(grabbedToy);
        
        if (showDebugInfo)
        {
            Debug.Log($"Drop completed. Grabbed toy: {(grabbedToy != null ? grabbedToy.name : "none")}");
        }
        
        // Send haptic feedback when drop is complete
        if (enableHaptics)
        {
            SendHapticFeedback(dropHapticIntensity, dropHapticDuration);
        }
        
        // Now the claw is back at the top with or without a toy
        isDropping = false;
        
        if (showDebugInfo)
        {
            Debug.Log("Claw drop completed");
        }
    }
    
    /// <summary>
    /// Coroutine to return the claw to its initial position
    /// </summary>
    private IEnumerator ReturnToInitialPositionRoutine()
    {
        // Determine target position
        Vector3 targetPos;
        
        if (returnPosition != null)
        {
            // Use the specified return position
            if (useLocalCoordinates)
            {
                targetPos = transform.parent.InverseTransformPoint(returnPosition.position);
                targetPos.y = startLocalPos.y; // Keep same height
            }
            else
            {
                targetPos = returnPosition.position;
                targetPos.y = transform.position.y; // Keep same height
            }
        }
        else
        {
            // Use the starting position
            targetPos = startLocalPos;
        }
        
        // Calculate distance to target (ignoring Y)
        Vector3 currentPosFlat = new Vector3(currentLocalPos.x, 0, currentLocalPos.z);
        Vector3 targetPosFlat = new Vector3(targetPos.x, 0, targetPos.z);
        float distanceToTarget = Vector3.Distance(currentPosFlat, targetPosFlat);
        
        if (showDebugInfo)
        {
            Debug.Log($"Returning to initial position. Distance: {distanceToTarget}");
        }
        
        // If already at target, skip
        if (distanceToTarget < 0.01f)
        {
            if (showDebugInfo)
            {
                Debug.Log("Already at initial position, skipping return");
            }
            yield break;
        }
        
        // Move towards target position
        float startTime = Time.time;
        while (distanceToTarget > 0.01f && Time.time - startTime < 5f) // Add timeout for safety
        {
            // Calculate direction to target (horizontal only)
            Vector3 direction = targetPosFlat - currentPosFlat;
            direction.Normalize();
            
            // Calculate move distance this frame
            float moveDistance = slideBackSpeed * Time.deltaTime;
            moveDistance = Mathf.Min(moveDistance, distanceToTarget);
            
            // Move towards target
            currentLocalPos.x += direction.x * moveDistance;
            currentLocalPos.z += direction.z * moveDistance;
            
            // Apply position
            if (useLocalCoordinates)
            {
                transform.localPosition = currentLocalPos;
            }
            else
            {
                Vector3 worldPos = new Vector3(currentLocalPos.x, transform.position.y, currentLocalPos.z);
                transform.position = worldPos;
            }
            
            // Recalculate distance
            currentPosFlat = new Vector3(currentLocalPos.x, 0, currentLocalPos.z);
            distanceToTarget = Vector3.Distance(currentPosFlat, targetPosFlat);
            
            yield return null;
        }
        
        // Ensure we're exactly at the target position
        if (useLocalCoordinates)
        {
            currentLocalPos.x = targetPos.x;
            currentLocalPos.z = targetPos.z;
            transform.localPosition = currentLocalPos;
        }
        else
        {
            Vector3 finalPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            transform.position = finalPos;
            currentLocalPos = transform.localPosition;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("Returned to initial position");
        }
    }
    
    public bool CanControl()
    {
        return !isDropping;
    }
    
    private void SendHapticFeedback(float intensity, float duration)
    {
        if (joystickController != null)
        {
            joystickController.SendHapticFeedback(intensity, duration);
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw machine bounds in the editor
        if (Application.isEditor && !Application.isPlaying)
        {
            // Get parent position for offset
            Vector3 parentPos = transform.parent != null ? transform.parent.position : Vector3.zero;
            float y = transform.position.y;
            
            // Draw the bounds as a rectangle
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                new Vector3(machineBoundsX.x + parentPos.x, y, machineBoundsZ.x + parentPos.z),
                new Vector3(machineBoundsX.y + parentPos.x, y, machineBoundsZ.x + parentPos.z)
            );
            Gizmos.DrawLine(
                new Vector3(machineBoundsX.y + parentPos.x, y, machineBoundsZ.x + parentPos.z),
                new Vector3(machineBoundsX.y + parentPos.x, y, machineBoundsZ.y + parentPos.z)
            );
            Gizmos.DrawLine(
                new Vector3(machineBoundsX.y + parentPos.x, y, machineBoundsZ.y + parentPos.z),
                new Vector3(machineBoundsX.x + parentPos.x, y, machineBoundsZ.y + parentPos.z)
            );
            Gizmos.DrawLine(
                new Vector3(machineBoundsX.x + parentPos.x, y, machineBoundsZ.y + parentPos.z),
                new Vector3(machineBoundsX.x + parentPos.x, y, machineBoundsZ.x + parentPos.z)
            );
            
            // Draw return position if set
            if (returnPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(returnPosition.position, 0.05f);
                Gizmos.DrawLine(transform.position, returnPosition.position);
            }
        }
    }
}

