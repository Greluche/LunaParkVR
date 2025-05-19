using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class ToyController : MonoBehaviour
{
    public enum ToyState { Idle, Caught, InDropZone, Grabbed }
    
    [Header("Settings")]
    public ToyState state = ToyState.Idle;
    public float weight = 1.0f; // Heavier toys are harder to grab
    
    [Header("Physics")]
    public bool useGravity = false; // Changed to false by default to prevent toys from falling initially
    public bool stayInDropZone = true; // Whether toy should stay in drop zone or can be removed
    
    [Header("Debug")]
    public bool showDebugVisuals = true;
    public Color debugColor = Color.green;
    
    // Layer settings
    private const string GRABBED_LAYER_NAME = "GrabbedToy";
    private int originalLayer;
    private int grabbedLayer;
    
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Vector3 initialPosition; // Store the initial position
    private Quaternion initialRotation; // Store the initial rotation
    private Transform originalParent; // Store the original parent
    private float grabbedByClawTime = 0f; // Time when grabbed by the claw
    private float dropZoneEntryTime = 0f; // Time when entered the drop zone
    private const float MIN_DROP_ZONE_STAY_TIME = 1.0f; // Minimum time to stay in drop zone
    private bool wasInDropZone = false;
    
    // Add variables to track the claw for following
    private Transform followTarget = null;
    private bool isFollowing = false;
    private Vector3 followOffset = Vector3.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        // Store initial position, rotation and parent
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        originalParent = transform.parent;
        originalLayer = gameObject.layer;
        
        // Make sure the GrabbedToy layer exists
        CreateGrabbedLayer();
        
        // Set up XR interactions
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
        
        // Initialize physics
        if (rb != null)
        {
            // Start with kinematic to prevent falling
            rb.isKinematic = true;
            rb.useGravity = useGravity;
        }
        
        SetIdle();
    }
    
    // Create the GrabbedToy layer if it doesn't exist
    private void CreateGrabbedLayer()
    {
        grabbedLayer = LayerMask.NameToLayer(GRABBED_LAYER_NAME);
        if (grabbedLayer == -1)
        {
            // Use default layer as fallback
            grabbedLayer = 0;
        }
    }

    // Called by Claw when caught
    public void SetCaught(Transform claw, float grabStrength = 0.8f)
    {
        state = ToyState.Caught;
        
        // Store original scale
        Vector3 originalScale = transform.localScale;
        
        // Make the toy kinematic so it doesn't fall
        if (rb != null)
        {
            rb.isKinematic = true; // Force kinematic immediately
            rb.useGravity = false; // Disable gravity
            rb.interpolation = RigidbodyInterpolation.None; // Disable interpolation
        }
        
        // Disable XR grab interaction while caught
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }
        
        // CRITICAL: Set layer to ensure visibility
        SetLayerRecursively(gameObject, grabbedLayer);
        
        // CRITICAL: Check for any renderer components and ensure they're enabled
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            
            // Check if any materials have transparency issues
            foreach (Material mat in renderer.materials)
            {
                // Make sure the material is visible
                Color color = mat.color;
                if (color.a < 1.0f)
                {
                    color.a = 1.0f;
                    mat.color = color;
                }
            }
        }
        
        // IMPORTANT: Don't parent to the claw - instead, follow it independently
        transform.SetParent(null); // Detach from any parent
        
        // CRITICAL: Force exact position match - no offset
        transform.position = claw.position;
        transform.rotation = claw.rotation;
        
        // CRITICAL: Ensure scale is maintained
        transform.localScale = originalScale;
        
        // Store the claw reference to follow it in Update
        followTarget = claw;
        isFollowing = true;
        followOffset = Vector3.zero; // NO OFFSET
        
        // Force immediate update of all transforms in the scene
        Physics.SyncTransforms();
        
        // Disable any colliders temporarily to prevent physics issues
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            // Store original state
            StartCoroutine(ToggleCollider(col, false));
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

    // Helper to toggle colliders with delay
    private IEnumerator ToggleCollider(Collider col, bool enabled)
    {
        // Wait a frame to ensure transform updates are processed
        yield return null;
        col.enabled = enabled;
    }

    // Called by Claw when released in drop zone
    public void SetInDropZone(Vector3 dropPosition)
    {
        state = ToyState.InDropZone;
        wasInDropZone = true;
        dropZoneEntryTime = Time.time;
        
        // Stop following the claw
        isFollowing = false;
        followTarget = null;
        
        // First ensure we're not parented to anything
        transform.SetParent(null);
        
        // IMPORTANT: Keep the toy at its current position (where the claw released it)
        // instead of teleporting it to a different position
        // This ensures the toy is physically above the drop zone
        
        // Reset layer to original
        SetLayerRecursively(gameObject, originalLayer);
        
        // Configure physics for drop zone with controlled forces
        if (rb != null)
        {
            // First zero out all velocities
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // Make it kinematic first to prevent immediate physics reactions
            rb.isKinematic = true;
            
            // Apply a small delay before enabling physics - reduced for speed
            StartCoroutine(EnablePhysicsAfterDelay(0.2f, transform.position));
        }
        
        // Re-enable colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
        
        // Re-enable XR grab interaction after a delay - reduced for speed
        StartCoroutine(EnableGrabAfterDelay(0.5f));
    }
    
    // Helper to enable physics after a delay with controlled force
    private IEnumerator EnablePhysicsAfterDelay(float delay, Vector3 dropPosition)
    {
        yield return new WaitForSeconds(delay);
        
        if (rb != null)
        {
            // Check if we're actually above the drop zone with a wider ray
            bool isAboveDropZone = false;
            bool isInsideMachine = false;
            
            // Check if we're inside a machine by looking for a parent with MachineBoundsHelper
            MachineBoundsHelper machineBounds = FindMachineBounds();
            if (machineBounds != null)
            {
                isInsideMachine = machineBounds.IsPointInBounds(transform.position);
            }
            
            // Cast a ray downward to see if we're above a drop zone
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit[] hits = Physics.RaycastAll(ray, 3.0f);
            
            foreach (RaycastHit hit in hits)
            {
                // Check if any hit object has a DropZoneController component or is tagged as a drop zone
                if (hit.transform.GetComponent<DropZoneController>() != null || 
                    hit.transform.CompareTag("DropZone"))
                {
                    isAboveDropZone = true;
                    break;
                }
            }
            
            // Always enable physics initially when released
            rb.isKinematic = false;
            rb.useGravity = true;
            
            if (isAboveDropZone)
            {
                // Apply a tiny controlled downward force
                rb.AddForce(Vector3.down * 0.02f, ForceMode.Impulse);
                
                // Add constraints to prevent horizontal movement initially
                rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                
                // Start monitoring for stability
                StartCoroutine(MonitorStability(dropPosition));
            }
            else
            {
                // If not above drop zone, start a coroutine to check for boundary exit
                StartCoroutine(CheckForDropZoneArrival(dropPosition));
            }
        }
    }
    
    // Find the machine bounds helper in the scene
    private MachineBoundsHelper FindMachineBounds()
    {
        // First try to find in parent hierarchy
        Transform current = transform.parent;
        while (current != null)
        {
            MachineBoundsHelper bounds = current.GetComponent<MachineBoundsHelper>();
            if (bounds != null)
                return bounds;
            
            current = current.parent;
        }
        
        // If not found in parents, try to find in scene
        return FindFirstObjectByType<MachineBoundsHelper>();
    }
    
    // New method to check when the toy reaches the drop zone
    private IEnumerator CheckForDropZoneArrival(Vector3 dropPosition)
    {
        float checkInterval = 0.1f; // Check more frequently
        float maxWaitTime = 3.0f; // Reduced max wait time
        float elapsedTime = 0f;
        
        // Find machine bounds
        MachineBoundsHelper machineBounds = FindMachineBounds();
        
        while (elapsedTime < maxWaitTime)
        {
            if (rb == null) yield break;
            
            // Check if we're above the drop zone with a wider ray
            bool isAboveDropZone = false;
            bool isInsideMachine = false;
            
            // Check if we're inside the machine
            if (machineBounds != null)
            {
                isInsideMachine = machineBounds.IsPointInBounds(transform.position);
            }
            
            // If we're outside the machine, make kinematic
            if (!isInsideMachine && rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            // Cast a ray downward to see if we're above a drop zone
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit[] hits = Physics.RaycastAll(ray, 3.0f);
            
            foreach (RaycastHit hit in hits)
            {
                // Check if any hit object has a DropZoneController component or is tagged as a drop zone
                if (hit.transform.GetComponent<DropZoneController>() != null || 
                    hit.transform.CompareTag("DropZone"))
                {
                    isAboveDropZone = true;
                    
                    // Enable gravity and physics
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    
                    // Apply a tiny controlled downward force
                    rb.AddForce(Vector3.down * 0.02f, ForceMode.Impulse);
                    
                    // Add constraints to prevent horizontal movement
                    rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                    
                    // Start monitoring for stability
                    StartCoroutine(MonitorStability(transform.position));
                    
                    // Exit the loop
                    break;
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
            
            // Check if we're inside the machine
            if (machineBounds != null)
            {
                isInsideMachine = machineBounds.IsPointInBounds(transform.position);
            }
            
            if (isInsideMachine)
            {
                // Only enable gravity if inside the machine
                rb.isKinematic = false;
                rb.useGravity = true;
                
                // Apply a tiny controlled downward force
                rb.AddForce(Vector3.down * 0.02f, ForceMode.Impulse);
                
                // Start monitoring for stability
                StartCoroutine(MonitorStability(transform.position));
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

    // Monitor the toy's stability and remove constraints when stable - reduced stabilization time
    private IEnumerator MonitorStability(Vector3 dropPosition)
    {
        float stabilityTimer = 0f;
        float requiredStableTime = 0.5f; // Reduced from 1.0f for faster stabilization
        
        while (stabilityTimer < requiredStableTime)
        {
            // Check if the toy is relatively stable
            if (rb != null && rb.linearVelocity.magnitude < 0.05f)
            {
                stabilityTimer += Time.deltaTime;
                
                // Ensure we stay within a small radius of the drop position
                if (Vector3.Distance(transform.position, dropPosition) > 0.2f)
                {
                    // If we've drifted too far, move back toward the drop position
                    transform.position = Vector3.Lerp(transform.position, dropPosition, 0.1f);
                    rb.linearVelocity = Vector3.zero;
                    stabilityTimer = 0f; // Reset stability timer
                }
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
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
        }
    }

    // Called when player grabs the toy
    private void OnGrab(SelectEnterEventArgs args)
    {
        state = ToyState.Grabbed;
        
        // Make non-kinematic for physics interaction
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true; // Enable gravity when grabbed by player
        }
        
        // If the toy was in the drop zone but hasn't been there long enough, prevent removal
        if (wasInDropZone && stayInDropZone && Time.time - dropZoneEntryTime < MIN_DROP_ZONE_STAY_TIME)
        {
            // Return to drop zone after a short delay
            StartCoroutine(ReturnToDropZone());
        }
    }

    // Called when player releases the toy
    private void OnRelease(SelectExitEventArgs args)
    {
        // If we're not in the drop zone, return to idle state
        if (state != ToyState.InDropZone)
        {
            SetIdle();
        }
    }

    // Called at start or when resetting
    public void SetIdle()
    {
        state = ToyState.Idle;
        
        // Reset physics
        if (rb != null)
        {
            // First make it non-kinematic to set velocities to zero
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // Then make it kinematic
            rb.isKinematic = true;
            rb.useGravity = useGravity;
        }
        
        // Enable XR grab interaction
        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;
        }
    }
    
    // Return to drop zone after a short delay
    System.Collections.IEnumerator ReturnToDropZone()
    {
        yield return new WaitForSeconds(0.2f);
        
        // Make sure the toy stays in drop zone
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
            yield return new WaitForSeconds(0.1f);
            grabInteractable.enabled = true;
        }
        
        state = ToyState.InDropZone;
    }
    
    // Re-enable grab interaction after a delay
    System.Collections.IEnumerator EnableGrabAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;
        }
    }
    
    // Reset the toy to its initial state and position
    public void ResetToy()
    {
        SetIdle();
        transform.SetParent(originalParent);
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        wasInDropZone = false;
    }

    // Update is called once per frame to follow the claw if needed
    void Update()
    {
        // If we're caught by the claw, follow it without parenting
        if (isFollowing && followTarget != null)
        {
            // CRITICAL: Force exact position match every frame
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
            
            // Check if renderers are still enabled
            if (Time.frameCount % 30 == 0)
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    if (!renderer.enabled)
                    {
                        renderer.enabled = true;
                    }
                }
            }
        }
        // If we're not being held by the claw, check if we're outside the machine
        else if (rb != null && !rb.isKinematic && state != ToyState.Grabbed)
        {
            // Find machine bounds
            MachineBoundsHelper machineBounds = FindMachineBounds();
            
            if (machineBounds != null)
            {
                // Check if we're outside the machine bounds
                if (!machineBounds.IsPointInBounds(transform.position))
                {
                    // We're outside the machine, make kinematic to prevent falling through floor
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    
                    // Stop any ongoing movement
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                else if (rb.isKinematic && state == ToyState.InDropZone)
                {
                    // We're back inside the machine and in the drop zone, re-enable physics
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }
        }
    }
}
