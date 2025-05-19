using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ClawScript : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float dropDist = 2f;
    public float dropSpeed = 2f;
    
    [Header("Delivery")]
    public Transform dropZone;
    public Transform dropZoneFloor; // Reference drop zone floor
    public float deliverTime = 1f;
    
    [Header("Grabbing")]
    [Tooltip("How firmly the claw grabs toys (higher = stronger grip)")]
    [Range(0f, 1f)]
    public float grabStrength = 0.8f;
    [Tooltip("How close the toy should be to the claw when grabbed")]
    public float toyGrabOffset = 0.05f;
    [Tooltip("How long to wait at the bottom of the drop")]
    public float grabWaitTime = 0.3f;
    
    [Header("Machine Bounds")]
    [Tooltip("The bounds of the machine in local space")]
    public Vector2 machineBoundsX = new Vector2(-1f, 1f);
    public Vector2 machineBoundsZ = new Vector2(-1f, 1f);
    public float machineHeight = 2f;
    
    [Header("Haptic Feedback")]
    public bool enableHaptics = true;
    [Range(0f, 1f)]
    public float grabHapticIntensity = 0.8f;
    public float grabHapticDuration = 0.3f;
    [Range(0f, 1f)]
    public float dropHapticIntensity = 0.4f;
    public float dropHapticDuration = 0.2f;
    [Range(0f, 1f)]
    public float deliveryHapticIntensity = 0.6f;
    public float deliveryHapticDuration = 0.4f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool useLocalCoordinates = true; // Set to true if claw should move in its local coordinate system
    
    // Layer settings for toys
    private const string GRABBED_LAYER_NAME = "GrabbedToy";
    private int grabbedLayer;
    
    Vector3 moveDir;
    bool isDropping, hasPrize;
    Vector3 startLocalPos;
    Vector3 currentLocalPos;
    private Transform caughtToy = null;
    private Vector3 toyOffset = Vector3.zero;
    
    // Reference to the joystick controller for haptic feedback
    private XRJoystickController joystickController;
    
    void Start()
    {
        startLocalPos = transform.localPosition;
        currentLocalPos = startLocalPos;
        
        // Try to find the joystick controller for haptic feedback
        joystickController = FindFirstObjectByType<XRJoystickController>();
        
        // Initialize the grabbed layer
        InitializeGrabbedLayer();
    }
    
    // Initialize the GrabbedToy layer
    private void InitializeGrabbedLayer()
    {
        grabbedLayer = LayerMask.NameToLayer(GRABBED_LAYER_NAME);
        if (grabbedLayer == -1)
        {
            // Use default layer as fallback
            grabbedLayer = 0;
        }
    }
    
    // Helper to set layer recursively on all children
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    
    void Update()
    {
        // Check if we should move the claw (not dropping and has direction)
        if (!isDropping && moveDir != Vector3.zero)
        {
            SlideClaw();
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
        
        // Store old position for debugging
        Vector3 oldPos = currentLocalPos;
        
        // Clamp to machine bounds
        newPos.x = Mathf.Clamp(newPos.x, machineBoundsX.x, machineBoundsX.y);
        newPos.z = Mathf.Clamp(newPos.z, machineBoundsZ.x, machineBoundsZ.y);
        newPos.y = startLocalPos.y; // Keep at constant height
        
        // Check if at boundary and provide visual feedback
        bool atBoundaryX = (Mathf.Approximately(newPos.x, machineBoundsX.x) && moveDir.x < 0) || 
                          (Mathf.Approximately(newPos.x, machineBoundsX.y) && moveDir.x > 0);
        bool atBoundaryZ = (Mathf.Approximately(newPos.z, machineBoundsZ.x) && moveDir.z < 0) || 
                          (Mathf.Approximately(newPos.z, machineBoundsZ.y) && moveDir.z > 0);
        
        // Apply the new position
        currentLocalPos = newPos;
        transform.localPosition = currentLocalPos;
        
        // Draw debug visualization of movement bounds
        if (showDebugInfo)
        {
            // Check if parent exists before using TransformPoint
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
    
    IEnumerator DropRoutine()
    {
        isDropping = true;
        StopMovement();
        
        // Send haptic feedback for drop start
        SendHapticFeedback(dropHapticIntensity, dropHapticDuration);
        
        // Store current position
        Vector3 dropStartPos = currentLocalPos;
        Vector3 dropTargetPos = dropStartPos + Vector3.down * dropDist;
        
        // Lower
        while (currentLocalPos.y > dropTargetPos.y)
        {
            currentLocalPos += Vector3.down * dropSpeed * Time.deltaTime;
            transform.localPosition = currentLocalPos;
            yield return null;
        }
        
        // Wait at the bottom to give time to grab toys
        yield return new WaitForSeconds(grabWaitTime);
        
        // Raise
        while (currentLocalPos.y < startLocalPos.y)
        {
            currentLocalPos += Vector3.up * dropSpeed * Time.deltaTime;
            transform.localPosition = currentLocalPos;
            yield return null;
        }
        
        // Reset position
        currentLocalPos = startLocalPos;
        transform.localPosition = currentLocalPos;
        
        // Deliver if caught
        if (caughtToy != null)
        {
            yield return DeliverRoutine();
        }
        else
        {
            isDropping = false;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isDropping && !hasPrize && other.CompareTag("Toy"))
        {
            hasPrize = true;
            caughtToy = other.transform;
            
            // Get the toy controller first
            ToyController toyScript = caughtToy.GetComponent<ToyController>();
            
            // Send haptic feedback for successful grab
            SendHapticFeedback(grabHapticIntensity, grabHapticDuration);
            
            // Store the original layer
            int originalLayer = caughtToy.gameObject.layer;
            
            // First make the toy kinematic to prevent physics issues
            Rigidbody toyRb = caughtToy.GetComponent<Rigidbody>();
            if (toyRb != null)
            {
                toyRb.isKinematic = true; // Force kinematic immediately
                toyRb.useGravity = false; // Disable gravity
                toyRb.interpolation = RigidbodyInterpolation.None; // Disable interpolation
            }
            
            // Store the original scale
            Vector3 originalScale = caughtToy.localScale;
            
            // Now use the ToyController to handle the caught state
            if (toyScript != null)
            {
                toyScript.SetCaught(transform, grabStrength);
            }
            else
            {
                // Fallback if no ToyController: position directly at the claw
                // Don't parent - just position in world space
                caughtToy.SetParent(null);
                
                // CRITICAL: Set layer to ensure visibility
                SetLayerRecursively(caughtToy.gameObject, grabbedLayer);
                
                // CRITICAL: Force exact position match - no offset
                caughtToy.position = transform.position;
                caughtToy.rotation = transform.rotation;
                caughtToy.localScale = originalScale; // Restore original scale
                
                // Ensure renderers are enabled
                Renderer[] renderers = caughtToy.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
                
                // Disable any colliders temporarily to prevent physics issues
                Collider[] colliders = caughtToy.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    col.enabled = false;
                }
                
                // Add a simple follow script to the toy
                FollowObject followScript = caughtToy.gameObject.AddComponent<FollowObject>();
                if (followScript != null)
                {
                    followScript.target = transform;
                    followScript.offset = Vector3.zero; // NO OFFSET
                    followScript.originalLayer = originalLayer; // Store original layer
                }
                
                // Force immediate update of all transforms in the scene
                Physics.SyncTransforms();
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Grabbed toy: {caughtToy.name} FORCIBLY positioned at {caughtToy.position}, Claw: {transform.position}");
            }
        }
    }
    
    // Helper class for toys without ToyController
    public class FollowObject : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = Vector3.zero;
        public int originalLayer;
        
        // Track last known good position
        private Vector3 lastGoodPosition;
        private bool initialized = false;
        
        void Start()
        {
            if (target != null)
            {
                lastGoodPosition = target.position + offset;
                transform.position = lastGoodPosition;
                initialized = true;
                
                Debug.Log($"FollowObject initialized at {transform.position}");
            }
        }
        
        void Update()
        {
            if (target != null)
            {
                // CRITICAL: Force exact position match every frame
                Vector3 targetPosition = target.position + offset;
                
                // Store this as a good position
                lastGoodPosition = targetPosition;
                
                // Set position directly - no interpolation
                transform.position = targetPosition;
                transform.rotation = target.rotation;
                
                // Debug position every 30 frames
                if (Time.frameCount % 30 == 0)
                {
                    Debug.Log($"FollowObject - FORCED Toy position: {transform.position}, Target position: {target.position}");
                    
                    // Check if renderers are still enabled
                    Renderer[] renderers = GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        if (!renderer.enabled)
                        {
                            Debug.LogWarning($"Renderer {renderer.name} was disabled! Re-enabling.");
                            renderer.enabled = true;
                        }
                    }
                }
            }
            else if (initialized)
            {
                // If we lost our target but had one before, stay at last good position
                transform.position = lastGoodPosition;
            }
        }
        
        // Make sure we're always in the right position even outside of Update
        void LateUpdate()
        {
            if (target != null)
            {
                // Double-check position
                Vector3 targetPosition = target.position + offset;
                if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
                {
                    transform.position = targetPosition;
                }
            }
        }
        
        // Reset layer and re-enable colliders when destroyed
        void OnDestroy()
        {
            // Reset layer to original
            SetLayerRecursively(gameObject, originalLayer);
            
            // Re-enable colliders
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = true;
            }
        }
        
        // Helper to set layer recursively on all children
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
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
            // Use the joystickController's SendHapticFeedback method instead
            joystickController.SendHapticFeedback(intensity, duration);
        }
    }
    
    IEnumerator DeliverRoutine()
    {
        Vector3 from = transform.position;
        // Go slightly above the drop zone
        Vector3 to = new Vector3(dropZone.position.x, from.y, dropZone.position.z);
        float t = 0f;
        
        // SPEED IMPROVEMENT: Increase movement speed to drop zone by reducing delivery time
        float fastDeliveryTime = deliverTime * 0.5f; // Cut delivery time in half
        
        // Move claw to position above drop zone
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(from, to, t);
            t += Time.deltaTime / fastDeliveryTime;
            yield return null;
        }
        transform.position = to;
        
        // Lower to drop zone level
        if (dropZoneFloor != null)
        {
            // Position slightly above the drop zone floor
            Vector3 lowerTo = new Vector3(to.x, dropZoneFloor.position.y + 0.2f, to.z);
            
            // SPEED IMPROVEMENT: Use faster drop speed
            float fasterDropSpeed = dropSpeed * 1.5f;
            
            while (Vector3.Distance(transform.position, lowerTo) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, lowerTo, fasterDropSpeed * Time.deltaTime);
                yield return null;
            }
            
            // Make sure we're actually at the target position
            transform.position = lowerTo;
            
            // Reduced stabilization wait time
            yield return new WaitForSeconds(0.1f);
        }
        
        // Release toy
        if (caughtToy != null)
        {
            // Send haptic feedback for delivery
            SendHapticFeedback(deliveryHapticIntensity, deliveryHapticDuration);
            
            // Calculate the drop position precisely at the drop zone center
            // Use the dropZoneFloor's position for Y if available
            Vector3 dropPosition = dropZone.position;
            if (dropZoneFloor != null)
            {
                // Position slightly above the floor to prevent clipping
                dropPosition.y = dropZoneFloor.position.y + 0.05f;
            }
            
            // CRITICAL: Make sure the toy is still with us and at our position
            caughtToy.position = transform.position;
            
            // Get the toy controller
            ToyController toyController = caughtToy.GetComponent<ToyController>();
            
            // Store the toy transform for later use
            Transform toyTransform = caughtToy;
            
            // First call ToyController to prepare for drop zone placement
            if (toyController != null)
            {
                // IMPORTANT: Make sure the toy is at the claw position first
                caughtToy.position = transform.position;
                toyController.SetInDropZone(transform.position);
            }
            else
            {
                // Remove any follow script we might have added
                FollowObject followScript = caughtToy.GetComponent<FollowObject>();
                if (followScript != null)
                {
                    Destroy(followScript);
                }
                
                // IMPORTANT: Make sure the toy is at the claw position first
                caughtToy.position = transform.position;
                
                // Configure physics
                Rigidbody toyRb = caughtToy.GetComponent<Rigidbody>();
                if (toyRb != null)
                {
                    // First zero out velocities
                    toyRb.linearVelocity = Vector3.zero;
                    toyRb.angularVelocity = Vector3.zero;
                    
                    // Make it kinematic first to prevent immediate falling
                    toyRb.isKinematic = true;
                    
                    // Apply a small delay before enabling physics
                    StartCoroutine(EnablePhysicsAfterDelay(toyRb, 0.1f, transform.position)); // Reduced delay
                }
            }
            
            // Clear references
            hasPrize = false;
            caughtToy = null;
        }
        
        // Reduced wait time before raising back up
        yield return new WaitForSeconds(0.1f);
        
        // Raise back up
        if (dropZoneFloor != null)
        {
            Vector3 raiseBack = new Vector3(to.x, from.y, to.z);
            
            // SPEED IMPROVEMENT: Use faster raise speed
            float fasterRaiseSpeed = dropSpeed * 1.5f;
            
            while (Vector3.Distance(transform.position, raiseBack) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, raiseBack, fasterRaiseSpeed * Time.deltaTime);
                yield return null;
            }
        }
        
        // Reset claw position
        currentLocalPos = startLocalPos;
        transform.localPosition = startLocalPos;
        
        isDropping = false;
    }
    
    // Helper to enable physics after a delay
    private IEnumerator EnablePhysicsAfterDelay(Rigidbody rb, float delay, Vector3 dropPosition)
    {
        yield return new WaitForSeconds(delay);
        
        if (rb != null)
        {
            // Check if we're above the drop zone before enabling gravity
            bool isAboveDropZone = false;
            bool isInsideMachine = false;
            
            // Check if we're inside the machine bounds
            if (transform.parent != null)
            {
                // Convert world position to local space relative to machine
                Vector3 localPos = transform.parent.InverseTransformPoint(rb.transform.position);
                
                // Check if within machine bounds
                isInsideMachine = 
                    localPos.x >= machineBoundsX.x && localPos.x <= machineBoundsX.y &&
                    localPos.z >= machineBoundsZ.x && localPos.z <= machineBoundsZ.y;
            }
            
            if (dropZone != null)
            {
                // Create a ray from the toy position downward
                Ray ray = new Ray(rb.transform.position, Vector3.down);
                RaycastHit[] hits = Physics.RaycastAll(ray, 3.0f);
                
                foreach (RaycastHit hit in hits)
                {
                    // Check if any hit object is the drop zone, has a DropZoneController component, or is tagged as a drop zone
                    if (hit.transform == dropZone || 
                        hit.transform == dropZoneFloor || 
                        hit.transform.GetComponent<DropZoneController>() != null ||
                        hit.transform.CompareTag("DropZone"))
                    {
                        isAboveDropZone = true;
                        break;
                    }
                }
            }
            
            // Always enable physics initially when released
            rb.isKinematic = false;
            rb.useGravity = true;
            
            if (isAboveDropZone)
            {
                // Apply a small controlled downward force
                rb.AddForce(Vector3.down * 0.05f, ForceMode.Impulse);
                
                // Add some damping to prevent excessive bouncing
                rb.linearDamping = 0.5f;
                rb.angularDamping = 0.5f;
                
                // Add constraints to prevent horizontal movement initially
                rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                
                // Start a coroutine to remove constraints after stabilization
                StartCoroutine(RemoveConstraintsAfterStabilization(rb));
            }
            else
            {
                // If not above drop zone, start a coroutine to check for boundary exit
                StartCoroutine(CheckForDropZoneArrival(rb, rb.transform.position));
            }
        }
    }
    
    // New method to check when the toy reaches the drop zone
    private IEnumerator CheckForDropZoneArrival(Rigidbody rb, Vector3 currentPosition)
    {
        float checkInterval = 0.1f; // Check more frequently
        float maxWaitTime = 3.0f; // Reduced max wait time
        float elapsedTime = 0f;
        
        while (elapsedTime < maxWaitTime)
        {
            if (rb == null) yield break;
            
            // Check if we're above the drop zone
            bool isAboveDropZone = false;
            bool isInsideMachine = false;
            
            // Check if we're inside the machine bounds
            if (transform.parent != null && rb != null)
            {
                // Convert world position to local space relative to machine
                Vector3 localPos = transform.parent.InverseTransformPoint(rb.transform.position);
                
                // Check if within machine bounds
                isInsideMachine = 
                    localPos.x >= machineBoundsX.x && localPos.x <= machineBoundsX.y &&
                    localPos.z >= machineBoundsZ.x && localPos.z <= machineBoundsZ.y;
                
                // If outside machine bounds, make kinematic
                if (!isInsideMachine)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            
            if (dropZone != null && rb != null)
            {
                // Create a ray from the toy position downward
                Ray ray = new Ray(rb.transform.position, Vector3.down);
                RaycastHit[] hits = Physics.RaycastAll(ray, 3.0f);
                
                foreach (RaycastHit hit in hits)
                {
                    // Check if any hit object is the drop zone, has a DropZoneController component, or is tagged as a drop zone
                    if (hit.transform == dropZone || 
                        hit.transform == dropZoneFloor || 
                        hit.transform.GetComponent<DropZoneController>() != null ||
                        hit.transform.CompareTag("DropZone"))
                    {
                        isAboveDropZone = true;
                        
                        // Enable gravity and physics
                        rb.isKinematic = false;
                        rb.useGravity = true;
                        
                        // Apply a small controlled downward force
                        rb.AddForce(Vector3.down * 0.05f, ForceMode.Impulse);
                        
                        // Add some damping to prevent excessive bouncing
                        rb.linearDamping = 0.5f;
                        rb.angularDamping = 0.5f;
                        
                        // Add constraints to prevent horizontal movement initially
                        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                        
                        // Start a coroutine to remove constraints after stabilization
                        StartCoroutine(RemoveConstraintsAfterStabilization(rb));
                        
                        // Exit the loop
                        break;
                    }
                }
            }
            
            if (isAboveDropZone)
            {
                break;
            }
            
            // Wait before checking again
            yield return new WaitForSeconds(checkInterval);
            elapsedTime += checkInterval;
        }
        
        // If we've waited too long and still haven't reached the drop zone
        if (elapsedTime >= maxWaitTime && rb != null)
        {
            bool isInsideMachine = false;
            
            // Check if we're inside the machine bounds
            if (transform.parent != null)
            {
                // Convert world position to local space relative to machine
                Vector3 localPos = transform.parent.InverseTransformPoint(rb.transform.position);
                
                // Check if within machine bounds
                isInsideMachine = 
                    localPos.x >= machineBoundsX.x && localPos.x <= machineBoundsX.y &&
                    localPos.z >= machineBoundsZ.x && localPos.z <= machineBoundsZ.y;
            }
            
            if (isInsideMachine)
            {
                // Only enable gravity if inside the machine
                rb.isKinematic = false;
                rb.useGravity = true;
                
                // Apply a small controlled downward force
                rb.AddForce(Vector3.down * 0.05f, ForceMode.Impulse);
            }
            else
            {
                // Outside the machine, make kinematic
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
    
    // New method to remove constraints after the toy has stabilized
    private IEnumerator RemoveConstraintsAfterStabilization(Rigidbody rb)
    {
        float stabilityTimer = 0f;
        float requiredStableTime = 1.0f;
        
        while (stabilityTimer < requiredStableTime)
        {
            // Check if the toy is relatively stable
            if (rb != null && rb.linearVelocity.magnitude < 0.05f)
            {
                stabilityTimer += Time.deltaTime;
            }
            else
            {
                stabilityTimer = 0f;
            }
            
            yield return null;
        }
        
        // Toy is stable, remove position constraints but keep some rotation damping
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }
}

