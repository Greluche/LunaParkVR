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
    
    [Header("Toy Grabbing")]
    public Transform grabPoint; // The point where toys will be attached
    public float grabRadius = 0.15f; // Radius to detect toys
    public LayerMask toyLayers = -1; // Layers that contain toys
    public Vector3 grabPointOffset = new Vector3(0, -0.05f, 0); // Offset from claw center
    
    [Header("XR Socket Interaction")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor; // Reference to the socket interactor
    public bool useSocketInteraction = true; // Whether to use socket interaction instead of physics grabbing
    
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
    
    // Current grabbed toy
    private GameObject grabbedToy = null;
    private Rigidbody toyRigidbody = null;
    private bool toyWasKinematic = false;
    private bool toyHadGravity = true;
    private Vector3 attachedToyOffset = Vector3.zero;
    
    // For tracking socket interaction
    private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable socketedInteractable = null;
    
    Vector3 moveDir;
    bool isDropping;
    Vector3 startLocalPos;
    Vector3 currentLocalPos;
    
    // Reference to the joystick controller for haptic feedback
    private XRJoystickController joystickController;
    
    // Caching claw colliders for better physics handling
    private Collider[] clawColliders;
    
    void Start()
    {
        startLocalPos = transform.localPosition;
        currentLocalPos = startLocalPos;
        
        // Try to find the joystick controller for haptic feedback
        joystickController = FindFirstObjectByType<XRJoystickController>();
        
        // Create grab point if not assigned
        if (grabPoint == null)
        {
            GameObject grabPointObj = new GameObject("GrabPoint");
            grabPoint = grabPointObj.transform;
            grabPoint.SetParent(transform);
            grabPoint.localPosition = grabPointOffset;
            
            if (showDebugInfo)
            {
                Debug.Log($"Created grab point at local position: {grabPointOffset}");
            }
        }
        
        // Cache all colliders attached to the claw
        clawColliders = GetComponentsInChildren<Collider>();
        
        if (clawColliders.Length == 0)
        {
            Debug.LogWarning("No colliders found on claw or its children. Physics detection may not work properly.");
        }
        else if (showDebugInfo)
        {
            Debug.Log($"Found {clawColliders.Length} colliders on the claw and its children.");
        }
        
        // Setup socket interactor if using it
        if (useSocketInteraction && socketInteractor == null)
        {
            // Try to find socket interactor on this object or its children
            socketInteractor = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            
            if (socketInteractor == null)
            {
                // Try to find on the grab point
                if (grabPoint != null)
                {
                    socketInteractor = grabPoint.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
                }
                
                if (socketInteractor == null && showDebugInfo)
                {
                    Debug.LogWarning("Socket interactor not found but socket interaction is enabled. Using physics grabbing instead.");
                    useSocketInteraction = false;
                }
            }
        }
        
        // Subscribe to socket events if using socket interaction
        if (useSocketInteraction && socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnSocketSelect);
            socketInteractor.selectExited.AddListener(OnSocketRelease);
            
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
        
        // Update grabbed toy position if we have one (only used for non-socket interaction)
        if (!useSocketInteraction && grabbedToy != null)
        {
            // Always keep the toy at the grab point
            if (grabbedToy.transform != null && grabPoint != null)
            {
                grabbedToy.transform.position = grabPoint.position + attachedToyOffset;
            }
            else
            {
                // Safety check - if toy or grab point was destroyed, clear references
                grabbedToy = null;
                toyRigidbody = null;
            }
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
        
        // Log grab point position
        if (grabPoint != null)
        {
            Debug.Log($"Grab Point Position: {grabPoint.position}, Offset from claw: {grabPoint.position - transform.position}");
        }
        else
        {
            Debug.LogWarning("Grab point is missing!");
        }
        
        // Check for any colliders on the claw
        Collider[] clawColliders = GetComponentsInChildren<Collider>();
        if (clawColliders.Length > 0)
        {
            foreach (var col in clawColliders)
            {
                if (col is SphereCollider sphere)
                {
                    // Calculate world space center of sphere collider
                    Vector3 worldCenter = col.transform.TransformPoint(sphere.center);
                    Debug.Log($"Claw SphereCollider: Center={worldCenter}, Radius={sphere.radius}, " +
                             $"Offset from claw={worldCenter - transform.position}, IsTrigger={col.isTrigger}");
                }
                else if (col is BoxCollider box)
                {
                    // Calculate world space center of box collider
                    Vector3 worldCenter = col.transform.TransformPoint(box.center);
                    Debug.Log($"Claw BoxCollider: Center={worldCenter}, Size={box.size}, " +
                             $"Offset from claw={worldCenter - transform.position}, IsTrigger={col.isTrigger}");
                }
                else
                {
                    Debug.Log($"Claw has {col.GetType().Name} collider, IsTrigger={col.isTrigger}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Claw has no colliders attached to it!");
        }
        
        // Check for colliders on the grab point
        if (grabPoint != null)
        {
            Collider[] grabPointColliders = grabPoint.GetComponents<Collider>();
            if (grabPointColliders.Length > 0)
            {
                Debug.Log($"Grab point has {grabPointColliders.Length} colliders attached");
            }
        }
        
        // Log socket interactor state
        if (useSocketInteraction && socketInteractor != null)
        {
            var selected = socketInteractor.hasSelection ? "Yes" : "No";
            Debug.Log($"Socket Interactor: Has selection: {selected}, Interactable: {socketedInteractable?.transform?.name ?? "None"}");
        }
        
        // Check if we're in the dropping state
        Debug.Log($"Claw state: isDropping={isDropping}");
    }
    
    // Socket interaction event handlers
    private void OnSocketSelect(SelectEnterEventArgs args)
    {
        if (showDebugInfo)
        {
            Debug.Log($"Socket selected: {args.interactableObject.transform.name}");
        }
        
        // Store a reference to the socketed interactable
        socketedInteractable = args.interactableObject;
        
        // Get the GameObject for compatibility with the existing code
        GameObject interactableObj = args.interactableObject.transform.gameObject;
        
        // For compatibility with old code, store as grabbedToy
        grabbedToy = interactableObj;
        
        // Send haptic feedback
        SendHapticFeedback(grabHapticIntensity, grabHapticDuration);
    }
    
    private void OnSocketRelease(SelectExitEventArgs args)
    {
        if (showDebugInfo)
        {
            Debug.Log($"Socket released: {args.interactableObject.transform.name}");
        }
        
        // Clear the reference to the socketed interactable
        socketedInteractable = null;
        
        // For compatibility with old code
        grabbedToy = null;
        
        // Send haptic feedback
        SendHapticFeedback(dropHapticIntensity, dropHapticDuration);
    }
    
    // Manually force socket interactor to release current selection
    private void ForceSocketRelease()
    {
        if (!useSocketInteraction || socketInteractor == null || !socketInteractor.hasSelection) return;
        
        if (showDebugInfo)
        {
            Debug.Log("Forcing socket to release toy");
        }
        
        // Store a local reference to the interactable before releasing
        var interactableToRelease = socketedInteractable;
        
        // Tell the socket to release its selection
        socketInteractor.allowSelect = false;
        
        // Apply small downward force to help the toy fall naturally
        if (interactableToRelease != null)
        {
            var rbToRelease = interactableToRelease.transform.GetComponent<Rigidbody>();
            if (rbToRelease != null)
            {
                // Ensure physics is enabled
                rbToRelease.isKinematic = false;
                rbToRelease.useGravity = true;
                
                // Apply downward force after a short delay to avoid collision issues
                StartCoroutine(ApplyForceAfterDelay(rbToRelease));
            }
        }
        
        // Reset socket state after a short delay
        StartCoroutine(ResetSocketAfterDelay());
    }
    
    // Helper to apply force after a short delay
    private IEnumerator ApplyForceAfterDelay(Rigidbody rb)
    {
        // Wait a short time for the physics to update
        yield return new WaitForSeconds(0.1f);
        
        if (rb != null)
        {
            rb.AddForce(Vector3.down * 0.5f, ForceMode.Impulse);
            
            if (showDebugInfo)
            {
                Debug.Log($"Applied downward force to released toy: {rb.gameObject.name}");
            }
        }
    }
    
    // Helper to reset socket state after a delay
    private IEnumerator ResetSocketAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        
        if (socketInteractor != null)
        {
            socketInteractor.allowSelect = true;
            
            if (showDebugInfo)
            {
                Debug.Log("Reset socket interactor state");
            }
        }
    }
    
    void SlideClaw()
    {
        // Extract X and Z components for clearer debugging
        float xDir = moveDir.x;
        float zDir = moveDir.z;
        
        // Calculate new position with movement speed scaled by direction magnitude
        Vector3 newPos = currentLocalPos;
        
        // Apply X movement if there's any X input
        if (Mathf.Abs(xDir) > 0.01f)
        {
            newPos.x += xDir * moveSpeed * Time.deltaTime;
        }
        
        // Apply Z movement if there's any Z input
        if (Mathf.Abs(zDir) > 0.01f)
        {
            newPos.z += zDir * moveSpeed * Time.deltaTime;
        }
        
        // Clamp to machine bounds
        newPos.x = Mathf.Clamp(newPos.x, machineBoundsX.x, machineBoundsX.y);
        newPos.z = Mathf.Clamp(newPos.z, machineBoundsZ.x, machineBoundsZ.y);
        newPos.y = startLocalPos.y; // Keep at constant height
        
        // Debug old and new positions
        if (showDebugInfo && Vector3.Distance(currentLocalPos, newPos) > 0.01f)
        {
            Debug.Log($"Moving claw: {currentLocalPos} -> {newPos}");
        }
        
        // Apply the new position
        currentLocalPos = newPos;
        transform.localPosition = currentLocalPos;
        
        // Make sure the grab point moves with the claw
        if (grabPoint != null && grabPoint.parent != transform)
        {
            Debug.LogWarning("Grab point is not a child of the claw! This could cause positioning issues.");
        }
        
        // Make sure child colliders update their positions
        Physics.SyncTransforms();
        
        // Draw debug visualization of movement bounds
        if (showDebugInfo)
        {
            DrawDebugBounds();
        }
    }
    
    void DrawDebugBounds()
    {
        if (transform.parent != null)
        {
            // Draw bounds
            Debug.DrawLine(
                transform.parent.TransformPoint(new Vector3(machineBoundsX.x, startLocalPos.y, machineBoundsZ.x)),
                transform.parent.TransformPoint(new Vector3(machineBoundsX.y, startLocalPos.y, machineBoundsZ.x)),
                Color.red
            );
            Debug.DrawLine(
                transform.parent.TransformPoint(new Vector3(machineBoundsX.y, startLocalPos.y, machineBoundsZ.x)),
                transform.parent.TransformPoint(new Vector3(machineBoundsX.y, startLocalPos.y, machineBoundsZ.y)),
                Color.red
            );
            Debug.DrawLine(
                transform.parent.TransformPoint(new Vector3(machineBoundsX.y, startLocalPos.y, machineBoundsZ.y)),
                transform.parent.TransformPoint(new Vector3(machineBoundsX.x, startLocalPos.y, machineBoundsZ.y)),
                Color.red
            );
            Debug.DrawLine(
                transform.parent.TransformPoint(new Vector3(machineBoundsX.x, startLocalPos.y, machineBoundsZ.y)),
                transform.parent.TransformPoint(new Vector3(machineBoundsX.x, startLocalPos.y, machineBoundsZ.x)),
                Color.red
            );
        }
        else
        {
            // If no parent, draw bounds in world space
            Vector3 worldPos = transform.position;
            float y = worldPos.y;
            
            Debug.DrawLine(
                new Vector3(machineBoundsX.x + worldPos.x, y, machineBoundsZ.x + worldPos.z),
                new Vector3(machineBoundsX.y + worldPos.x, y, machineBoundsZ.x + worldPos.z),
                Color.red
            );
            Debug.DrawLine(
                new Vector3(machineBoundsX.y + worldPos.x, y, machineBoundsZ.x + worldPos.z),
                new Vector3(machineBoundsX.y + worldPos.x, y, machineBoundsZ.y + worldPos.z),
                Color.red
            );
            Debug.DrawLine(
                new Vector3(machineBoundsX.y + worldPos.x, y, machineBoundsZ.y + worldPos.z),
                new Vector3(machineBoundsX.x + worldPos.x, y, machineBoundsZ.y + worldPos.z),
                Color.red
            );
            Debug.DrawLine(
                new Vector3(machineBoundsX.x + worldPos.x, y, machineBoundsZ.y + worldPos.z),
                new Vector3(machineBoundsX.x + worldPos.x, y, machineBoundsZ.x + worldPos.z),
                Color.red
            );
        }
        
        // Draw current movement direction
        Debug.DrawRay(transform.position, moveDir * 0.5f, Color.blue);
        // Draw X component
        if (Mathf.Abs(moveDir.x) > 0.01f)
            Debug.DrawRay(transform.position, new Vector3(moveDir.x, 0, 0) * 0.4f, Color.cyan);
        // Draw Z component
        if (Mathf.Abs(moveDir.z) > 0.01f)
            Debug.DrawRay(transform.position, new Vector3(0, 0, moveDir.z) * 0.4f, Color.yellow);
    }
    
    public void SetDirection(Vector3 d)
    {
        // Store previous direction for debugging
        Vector3 prevDir = moveDir;
        
        // Set the movement direction
        moveDir = d;
        
        // Debug direction changes
        if (moveDir != prevDir && showDebugInfo)
        {
            string directionInfo = "Claw direction changed to: ";
            if (Mathf.Abs(moveDir.x) > 0.01f)
                directionInfo += $"X={moveDir.x:F2} ";
            if (Mathf.Abs(moveDir.z) > 0.01f)
                directionInfo += $"Z={moveDir.z:F2}";
            
            Debug.Log(directionInfo);
        }
    }
    
    public void StopMovement()
    {
        moveDir = Vector3.zero;
    }
    
    public void Drop()
    {
        if (!isDropping) StartCoroutine(DropRoutine());
    }
    
    // Try to grab a toy - returns true if successful
    private bool TryGrabToy()
    {
        // Don't try to grab if we already have a toy
        if (grabbedToy != null) return false;
        
        // Make sure the grab point exists
        if (grabPoint == null)
        {
            Debug.LogError("Grab point is missing! Cannot grab toys.");
            return false;
        }
        
        // Force physics update to ensure all transforms are in sync
        Physics.SyncTransforms();
        
        // Debug output - show where we're checking for toys
        if (showDebugInfo)
        {
            Debug.Log($"Checking for toys at {grabPoint.position} with radius {grabRadius}");
            // Draw a red sphere at the grab position when debugging
            Debug.DrawRay(grabPoint.position, Vector3.up * 0.1f, Color.red, 2.0f);
        }
        
        // If toy layers is set to nothing, use all layers
        if (toyLayers.value == 0)
        {
            toyLayers = Physics.AllLayers;
            if (showDebugInfo)
            {
                Debug.Log("No toy layers specified, using all layers.");
            }
        }
        
        // Find all colliders in grab radius - using non-allocating version for better performance
        Collider[] hitColliders = new Collider[20]; // Preallocate array
        int hitCount = Physics.OverlapSphereNonAlloc(grabPoint.position, grabRadius, hitColliders, toyLayers);
        
        // Debug the detection
        if (showDebugInfo)
        {
            Debug.Log($"Found {hitCount} potential toys to grab");
            
            // List found colliders for debugging
            for (int i = 0; i < hitCount; i++)
            {
                var col = hitColliders[i];
                Debug.Log($"Found collider: {col.name} on layer {LayerMask.LayerToName(col.gameObject.layer)}");
            }
            
            // Visualize the grab sphere
            DebugDrawSphere(grabPoint.position, grabRadius, Color.red, 2.0f);
        }
        
        // Try to grab the first valid toy
        for (int i = 0; i < hitCount; i++)
        {
            var hitCollider = hitColliders[i];
            
            // Skip null entries
            if (hitCollider == null) continue;
            
            // Skip if this is part of the claw itself
            if (hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform))
            {
                if (showDebugInfo)
                    Debug.Log($"Skipping {hitCollider.name} - it's part of the claw");
                continue;
            }
                
            // Get the toy controller if it exists
            ToyController toyController = hitCollider.GetComponentInParent<ToyController>();
            if (toyController == null)
            {
                if (showDebugInfo)
                    Debug.Log($"No ToyController found on {hitCollider.name}");
            }
            
            // Get rigidbody from the toy's root object
            Rigidbody rb = null;
            
            // Try to get rigidbody from the collider's gameobject first
            rb = hitCollider.attachedRigidbody;
            
            // If that didn't work, try getting it from parent
            if (rb == null)
                rb = hitCollider.GetComponentInParent<Rigidbody>();
            
            if (rb == null)
            {
                if (showDebugInfo)
                    Debug.Log($"No Rigidbody found on {hitCollider.name} or its parent");
                continue;
            }
            
            // Grab this toy
            grabbedToy = rb.gameObject; // Use the rigidbody's gameobject to ensure we grab the root
            toyRigidbody = rb;
            
            // Store the original physics state
            toyWasKinematic = toyRigidbody.isKinematic;
            toyHadGravity = toyRigidbody.useGravity;
            
            // Disable physics while grabbed
            toyRigidbody.isKinematic = true;
            toyRigidbody.useGravity = false;
            
            // Calculate offset to maintain relative position
            attachedToyOffset = rb.transform.position - grabPoint.position;
            
            // Keep the offset small
            attachedToyOffset = Vector3.ClampMagnitude(attachedToyOffset, 0.1f);
            
            // Notify toy controller if available
            if (toyController != null)
            {
                // Call OnGrabbed on the toy
                toyController.OnGrabbed(this);
            }
            else
            {
                if (showDebugInfo)
                    Debug.Log($"No ToyController on {grabbedToy.name}, but grabbing anyway");
            }
            
            // Send haptic feedback
            SendHapticFeedback(grabHapticIntensity, grabHapticDuration);
            
            if (showDebugInfo)
            {
                Debug.Log($"Successfully grabbed toy: {grabbedToy.name}");
            }
            
            return true;
        }
        
        // If we get here, no valid toy was found
        if (showDebugInfo)
        {
            Debug.Log("No valid toys found to grab");
        }
        
        return false;
    }
    
    // Release the currently grabbed toy
    private void ReleaseToy()
    {
        if (grabbedToy == null) return;
        
        if (showDebugInfo)
        {
            Debug.Log($"Releasing toy: {grabbedToy.name}");
        }
        
        // Restore physics properties
        if (toyRigidbody != null)
        {
            toyRigidbody.isKinematic = toyWasKinematic;
            toyRigidbody.useGravity = toyHadGravity;
            
            // Add a small downward force
            toyRigidbody.AddForce(Vector3.down * 0.5f, ForceMode.Impulse);
        }
        
        // Notify toy controller if available
        ToyController toyController = grabbedToy.GetComponentInParent<ToyController>();
        if (toyController != null)
        {
            // Call OnReleased on the toy
            toyController.OnReleased(this);
        }
        
        // Send haptic feedback
        SendHapticFeedback(dropHapticIntensity, dropHapticDuration);
        
        // Clear references
        grabbedToy = null;
        toyRigidbody = null;
    }
    
    IEnumerator DropRoutine()
    {
        isDropping = true;
        StopMovement();
        
        // Send haptic feedback for drop start
        SendHapticFeedback(dropHapticIntensity, dropHapticDuration);
        
        if (showDebugInfo)
        {
            Debug.Log("Starting claw drop routine");
        }
        
        // Store current position
        Vector3 dropStartPos = currentLocalPos;
        Vector3 dropTargetPos = dropStartPos + Vector3.down * dropDist;
        
        // Lower
        if (showDebugInfo)
        {
            Debug.Log($"Lowering claw from {currentLocalPos.y} to {dropTargetPos.y}");
        }
        
        while (currentLocalPos.y > dropTargetPos.y)
        {
            currentLocalPos += Vector3.down * dropSpeed * Time.deltaTime;
            transform.localPosition = currentLocalPos;
            yield return null;
        }
        
        // Make sure we reach exactly the target position
        currentLocalPos.y = dropTargetPos.y;
        transform.localPosition = currentLocalPos;
        
        if (showDebugInfo)
        {
            Debug.Log("Claw reached bottom position, attempting to grab toy...");
        }
        
        // Socket interactor should automatically grab toys, so we don't need to do anything for socket mode
        // For non-socket mode, try to grab a toy manually
        bool toyGrabbed = false;
        if (!useSocketInteraction)
        {
            toyGrabbed = TryGrabToy();
        }
        else
        {
            // For socket mode, just check if something was grabbed
            toyGrabbed = socketInteractor != null && socketInteractor.hasSelection;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Grab attempt result: {(toyGrabbed ? "Success" : "Failed")}");
        }
        
        // Wait at the bottom to give time for physics interactions
        float bottomWaitTime = 0.3f;
        if (showDebugInfo)
        {
            Debug.Log($"Waiting at bottom for {bottomWaitTime} seconds");
        }
        yield return new WaitForSeconds(bottomWaitTime);
        
        // For non-socket mode, try grabbing one more time
        if (!useSocketInteraction && !toyGrabbed)
        {
            if (showDebugInfo)
            {
                Debug.Log("Trying one more grab attempt after wait...");
            }
            toyGrabbed = TryGrabToy();
            if (showDebugInfo)
            {
                Debug.Log($"Second grab attempt result: {(toyGrabbed ? "Success" : "Failed")}");
            }
        }
        else if (useSocketInteraction)
        {
            // For socket mode, check if something was grabbed
            toyGrabbed = socketInteractor != null && socketInteractor.hasSelection;
            
            if (showDebugInfo)
            {
                Debug.Log($"Socket selection state after wait: {(toyGrabbed ? "Has selection" : "No selection")}");
            }
        }
        
        // Raise
        if (showDebugInfo)
        {
            Debug.Log($"Raising claw back to {startLocalPos.y}");
        }
        while (currentLocalPos.y < startLocalPos.y)
        {
            currentLocalPos += Vector3.up * dropSpeed * Time.deltaTime;
            transform.localPosition = currentLocalPos;
            yield return null;
        }

        // Debug start of sliding back
        if (showDebugInfo)
        {
            Debug.Log($"Starting slide back from {currentLocalPos} to {startLocalPos}, distance: {Vector3.Distance(currentLocalPos, startLocalPos)}");
        }
        
        // Make sure we're at the right height before sliding back
        currentLocalPos.y = startLocalPos.y;
        transform.localPosition = currentLocalPos;
        
        // Store the horizontal distance that needs to be traveled
        Vector3 targetPos = new Vector3(startLocalPos.x, startLocalPos.y, startLocalPos.z);
        float totalDistance = Vector3.Distance(
            new Vector3(currentLocalPos.x, 0, currentLocalPos.z), 
            new Vector3(targetPos.x, 0, targetPos.z)
        );
        
        // If there's no significant horizontal distance to travel, skip the sliding
        if (totalDistance < 0.01f)
        {
            if (showDebugInfo)
            {
                Debug.Log("No horizontal distance to travel, skipping slide back");
            }
            // Ensure exact position
            currentLocalPos = startLocalPos;
            transform.localPosition = currentLocalPos;
        }
        else
        {
            // Slide back to initial position (horizontal movement only)
            float remainingDistance = totalDistance;
            float startTime = Time.time;
            
            while (remainingDistance > 0.01f)
            {
                // Calculate direction to target (horizontal only)
                Vector3 toTarget = targetPos - currentLocalPos;
                toTarget.y = 0; // Ignore vertical difference
                Vector3 direction = toTarget.normalized;
                
                // Calculate move distance this frame
                float moveDistance = slideBackSpeed * Time.deltaTime;
                
                // Don't overshoot
                moveDistance = Mathf.Min(moveDistance, remainingDistance);
                
                // Move towards target using slideBackSpeed
                currentLocalPos += direction * moveDistance;
                
                // Keep Y position constant
                currentLocalPos.y = startLocalPos.y;
                
                // Apply the new position
                transform.localPosition = currentLocalPos;
                
                // Recalculate remaining distance
                remainingDistance = Vector3.Distance(
                    new Vector3(currentLocalPos.x, 0, currentLocalPos.z), 
                    new Vector3(targetPos.x, 0, targetPos.z)
                );
                
                // Debug progress
                if (showDebugInfo && Time.frameCount % 10 == 0)
                {
                    Debug.Log($"Sliding back: distance remaining = {remainingDistance}, position = {currentLocalPos}");
                }
                
                // Safety check - timeout after 5 seconds to prevent infinite loops
                if (Time.time - startTime > 5.0f)
                {
                    Debug.LogWarning("Slide back timeout - forcing to final position");
                    break;
                }
                
                yield return null;
            }
            
            // Ensure we're exactly at the target position
            currentLocalPos = startLocalPos;
            transform.localPosition = currentLocalPos;
            
            if (showDebugInfo)
            {
                Debug.Log($"Finished sliding back, final position: {currentLocalPos}");
            }
        }
        
        // Release the toy
        if (useSocketInteraction)
        {
            // For socket mode, force release it
            if (socketInteractor != null && socketInteractor.hasSelection)
            {
                if (showDebugInfo)
                {
                    Debug.Log("Releasing toy from socket interactor");
                }
                
                // Force the socket to release the toy
                ForceSocketRelease();
            }
            else if (showDebugInfo)
            {
                Debug.Log("No toy in socket to release");
            }
        }
        else
        {
            // For physics mode, use the existing release method
            if (grabbedToy != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Releasing toy {grabbedToy.name} at final position");
                }
                ReleaseToy();
            }
            else if (showDebugInfo)
            {
                Debug.Log("No toy was grabbed during this drop");
            }
        }
        
        // Determine which toy to report as grabbed
        GameObject toyToReport = null;
        
        if (useSocketInteraction && socketInteractor != null && socketInteractor.hasSelection)
        {
            // For socket interaction, get the actual interactable GameObject
            if (socketedInteractable != null)
            {
                toyToReport = socketedInteractable.transform.gameObject;
                Debug.Log($"ClawScript: Reporting socketed toy as grabbed: {toyToReport.name}, ID: {toyToReport.GetInstanceID()}");
            }
        }
        else if (grabbedToy != null)
        {
            // For non-socket interaction, use the grabbed toy
            toyToReport = grabbedToy;
            Debug.Log($"ClawScript: Reporting directly grabbed toy: {toyToReport.name}, ID: {toyToReport.GetInstanceID()}");
        }
        else
        {
            Debug.Log("ClawScript: No toy to report as grabbed");
        }
        
        // Invoke the drop completed event
        Debug.Log($"ClawScript: Invoking OnDropCompleted with toy: {(toyToReport != null ? toyToReport.name : "none")}");
        OnDropCompleted.Invoke(toyToReport);
        
        isDropping = false;
        
        if (showDebugInfo)
        {
            Debug.Log("Drop routine complete");
        }
    }
    
    public bool CanControl()
    {
        return !isDropping;
    }
    
    // Helper method to send haptic feedback through the joystick controller
    private void SendHapticFeedback(float intensity, float duration)
    {
        if (enableHaptics && joystickController != null && joystickController.hapticFeedbackDevice != null)
        {
            joystickController.SendHapticFeedback(intensity, duration);
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw the grab radius
        Gizmos.color = grabbedToy != null ? Color.green : Color.yellow;
        if (grabPoint != null)
        {
            Gizmos.DrawWireSphere(grabPoint.position, grabRadius);
            
            // Show toys in range for debugging
            VisualizeToysInRange();
        }
        else
        {
            // Draw at default position if grab point is not set
            Vector3 defaultGrabPos = transform.position + grabPointOffset;
            Gizmos.DrawWireSphere(defaultGrabPos, grabRadius);
        }
        
        // Draw a line to the grabbed toy if we have one
        if (grabbedToy != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, grabbedToy.transform.position);
        }
    }
    
    // Visualize toys in the grab radius
    void VisualizeToysInRange()
    {
        if (grabPoint == null) return;
        
        // If toy layers is set to nothing, use all layers
        LayerMask visualizeLayers = toyLayers.value == 0 ? Physics.AllLayers : toyLayers;
        
        // Find all colliders in grab radius
        Collider[] hitColliders = Physics.OverlapSphere(grabPoint.position, grabRadius, visualizeLayers);
        
        foreach (var hitCollider in hitColliders)
        {
            // Skip if this is part of the claw itself
            if (hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform))
                continue;
            
            // Check if it has a toy controller or rigidbody
            bool hasToyController = hitCollider.GetComponentInParent<ToyController>() != null;
            bool hasRigidbody = hitCollider.attachedRigidbody != null || hitCollider.GetComponentInParent<Rigidbody>() != null;
            
            // Valid toy = red, invalid = orange
            Gizmos.color = hasRigidbody ? Color.red : Color.yellow;
            
            // Draw a small sphere at each collider location
            Gizmos.DrawSphere(hitCollider.bounds.center, 0.02f);
            
            // Draw a line from the grab point to each detected collider
            Gizmos.DrawLine(grabPoint.position, hitCollider.bounds.center);
            
            // Draw box showing the collider bounds
            Gizmos.DrawWireCube(hitCollider.bounds.center, hitCollider.bounds.size);
        }
    }
    
    // Helper for visualizing spheres in debug
    void DebugDrawSphere(Vector3 position, float radius, Color color, float duration)
    {
        if (!showDebugInfo) return;
        
        float angle = 0;
        Vector3 x, y;
        
        // Draw three circles
        x = new Vector3(0, 0, 1);
        y = new Vector3(1, 0, 0);
        DrawCircle(position, x, y, radius, color, duration);
        
        x = new Vector3(0, 1, 0);
        y = new Vector3(0, 0, 1);
        DrawCircle(position, x, y, radius, color, duration);
        
        x = new Vector3(0, 1, 0);
        y = new Vector3(1, 0, 0);
        DrawCircle(position, x, y, radius, color, duration);
    }
    
    // Helper for drawing debug circles
    void DrawCircle(Vector3 position, Vector3 x, Vector3 y, float radius, Color color, float duration)
    {
        Vector3 lastPoint = position + radius * (x * Mathf.Cos(0) + y * Mathf.Sin(0));
        float resolution = 0.1f; // Adjust for smoother circles
        
        for (float theta = resolution; theta < 2 * Mathf.PI; theta += resolution)
        {
            Vector3 nextPoint = position + radius * (x * Mathf.Cos(theta) + y * Mathf.Sin(theta));
            Debug.DrawLine(lastPoint, nextPoint, color, duration);
            lastPoint = nextPoint;
        }
        
        // Connect the last and first point
        Vector3 firstPoint = position + radius * (x * Mathf.Cos(0) + y * Mathf.Sin(0));
        Debug.DrawLine(lastPoint, firstPoint, color, duration);
    }
}

