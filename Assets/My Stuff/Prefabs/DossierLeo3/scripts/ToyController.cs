using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ToyController : MonoBehaviour
{
    [Header("Primary Teleportation")]
    [Tooltip("The transform where the toy will be teleported when it enters the drop zone")]
    public Transform destinationPoint;
    
    [Tooltip("Direct reference to the drop zone collider (optional)")]
    public Collider dropZoneCollider;
    
    [Tooltip("Tag of the drop zone (default: DropZone)")]
    public string dropZoneTag = "DropZone";
    
    [Tooltip("Should the toy reset its rotation when teleported")]
    public bool resetRotationOnTeleport = true;
    
    [Tooltip("Make toy kinematic after teleporting to prevent jittering")]
    public bool makeKinematicAfterTeleport = true;
    
    [Header("Secondary Teleportation")]
    [Tooltip("Optional second destination point that toy will teleport to after delay")]
    public Transform destinationPoint2;
    
    [Tooltip("Time in seconds before teleporting to second destination (0 = disabled)")]
    [Range(0f, 10f)]
    public float secondTeleportDelay = 0f;
    
    [Tooltip("Should the toy reset its rotation when teleported to second destination")]
    public bool resetRotationOnSecondTeleport = true;
    
    [Tooltip("Enable gravity after teleporting to second destination")]
    public bool useGravityAfterSecondTeleport = true;
    
    [Header("Initial Position")]
    [Tooltip("Store the initial position on start to allow resetting")]
    public bool storeInitialPosition = true;
    
    [Tooltip("Should the toy use a random rotation when reset to initial position")]
    public bool randomRotationOnReset = true;
    
    [Header("Golden Teddy Settings")]
    [Tooltip("Is this a golden teddy (special prize)")]
    public bool isGoldenTeddy = false;
    
    [Header("Haptic Feedback")]
    [Tooltip("Haptic intensity when a golden teddy is grabbed")]
    [Range(0f, 1f)]
    public float hapticIntensity = 0.5f;
    
    [Tooltip("Duration of haptic feedback")]
    public float hapticDuration = 0.2f;
    
    [Header("Audio")]
    [Tooltip("Sound played when a golden teddy is grabbed")]
    public AudioClip goldenTeddySound;
    
    [Tooltip("Sound played when teleporting to second destination")]
    public AudioClip secondTeleportSound;
    
    // Private references
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool hasTeleported = false;
    private bool hasSecondTeleported = false;
    private Collider[] myColliders;
    private Coroutine secondTeleportCoroutine;
    
    // Initial position storage
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    void Awake()
    {
        // Get required components
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        // Get all colliders on this object
        myColliders = GetComponents<Collider>();
        
        // Create audio source if needed and we have sounds
        if (audioSource == null && (goldenTeddySound != null || secondTeleportSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
        }
        
        // Validate destination point
        if (destinationPoint == null)
        {
            Debug.LogWarning($"No destination point set for {gameObject.name}. Toy won't teleport when it enters drop zone.");
        }
        
        // Validate drop zone reference
        if (dropZoneCollider == null)
        {
            Debug.Log($"No drop zone collider directly assigned to {gameObject.name}. Will use tag '{dropZoneTag}' for detection.");
        }
        
        // Store initial position and rotation
        if (storeInitialPosition)
        {
            StoreInitialPosition();
        }
    }
    
    void Start()
    {
        // Store initial position if not done in Awake
        if (storeInitialPosition && initialPosition == Vector3.zero)
        {
            StoreInitialPosition();
        }
    }
    
    /// <summary>
    /// Stores the current position and rotation as the initial state
    /// </summary>
    public void StoreInitialPosition()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        Debug.Log($"Stored initial position for {gameObject.name}: {initialPosition}");
    }
    
    void OnDisable()
    {
        // Cancel any pending teleports if object is disabled
        if (secondTeleportCoroutine != null)
        {
            StopCoroutine(secondTeleportCoroutine);
            secondTeleportCoroutine = null;
        }
    }
    
    /// <summary>
    /// Called when the toy is grabbed by the socket interactor
    /// </summary>
    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Reset teleport flags when grabbed
        hasTeleported = false;
        hasSecondTeleported = false;
        
        // Cancel any pending teleports
        if (secondTeleportCoroutine != null)
        {
            StopCoroutine(secondTeleportCoroutine);
            secondTeleportCoroutine = null;
        }
        
        if (isGoldenTeddy)
        {
            // Play golden teddy sound
            if (audioSource != null && goldenTeddySound != null)
            {
                audioSource.clip = goldenTeddySound;
                audioSource.Play();
            }
            
            // Send haptic feedback
            SendHapticFeedback(args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor);
            
            Debug.Log($"Golden teddy {gameObject.name} grabbed!");
        }
    }
    
    /// <summary>
    /// Called when the toy enters a trigger collider (the drop zone)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Skip if already teleported to prevent multiple teleports
        if (hasTeleported)
            return;
            
        // Check if this is the specific drop zone collider we referenced
        if (dropZoneCollider != null && other == dropZoneCollider)
        {
            Debug.Log($"Toy {gameObject.name} entered the referenced drop zone");
            TeleportToDestination();
            return;
        }
        
        // Otherwise check by tag
        if (other.CompareTag(dropZoneTag))
        {
            Debug.Log($"Toy {gameObject.name} entered a drop zone with tag: {dropZoneTag}");
            TeleportToDestination();
        }
    }
    
    /// <summary>
    /// Teleports the toy to the destination point
    /// </summary>
    private void TeleportToDestination()
    {
        if (destinationPoint != null)
        {
            // Mark as teleported to prevent multiple teleports
            hasTeleported = true;
            
            // Disable physics temporarily
            bool wasKinematic = false;
            if (rb != null)
            {
                wasKinematic = rb.isKinematic;
                rb.isKinematic = true;
            }
            
            // Teleport with or without rotation reset
            if (resetRotationOnTeleport)
            {
                transform.SetPositionAndRotation(
                    destinationPoint.position,
                    destinationPoint.rotation
                );
            }
            else
            {
                transform.position = destinationPoint.position;
            }
            
            // Restore physics state or keep kinematic based on setting
            if (rb != null)
            {
                if (makeKinematicAfterTeleport)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    
                    // Disable collision detection to prevent jittering
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    rb.interpolation = RigidbodyInterpolation.None;
                }
                else
                {
                    rb.isKinematic = wasKinematic;
                    
                    // Ensure velocity is reset
                    if (!wasKinematic)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                }
            }
            
            // Start a coroutine to handle post-teleport stabilization
            StartCoroutine(StabilizeAfterTeleport());
            
            // Schedule second teleport if enabled
            if (destinationPoint2 != null && secondTeleportDelay > 0f && !hasSecondTeleported)
            {
                // Cancel any existing second teleport
                if (secondTeleportCoroutine != null)
                {
                    StopCoroutine(secondTeleportCoroutine);
                }
                
                // Start new teleport coroutine
                secondTeleportCoroutine = StartCoroutine(TeleportToSecondDestination(secondTeleportDelay));
            }
            
            Debug.Log($"Teleported {gameObject.name} to destination point");
        }
        else
        {
            Debug.LogWarning($"No destination point set for {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Teleports the toy to the second destination after a delay
    /// </summary>
    private IEnumerator TeleportToSecondDestination(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);
        
        // Check if we have a valid second destination
        if (destinationPoint2 != null && !hasSecondTeleported)
        {
            // Mark as second teleported
            hasSecondTeleported = true;
            
            // Play sound if available
            if (audioSource != null && secondTeleportSound != null)
            {
                audioSource.clip = secondTeleportSound;
                audioSource.Play();
            }
            
            Debug.Log($"Teleporting {gameObject.name} to second destination after {delay} seconds");
            
            // Teleport with or without rotation reset
            if (resetRotationOnSecondTeleport)
            {
                transform.SetPositionAndRotation(
                    destinationPoint2.position,
                    destinationPoint2.rotation
                );
            }
            else
            {
                transform.position = destinationPoint2.position;
            }
            
            // Make sure physics is set correctly
            if (rb != null)
            {
                // Set kinematic to false if gravity should be used
                rb.isKinematic = !useGravityAfterSecondTeleport;
                rb.useGravity = useGravityAfterSecondTeleport;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                // Set appropriate collision detection mode for dynamic objects
                if (useGravityAfterSecondTeleport)
                {
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                }
            }
            
            // Start stabilization for second teleport
            StartCoroutine(StabilizeAfterTeleport());
        }
        
        // Clear the coroutine reference
        secondTeleportCoroutine = null;
    }
    
    /// <summary>
    /// Stabilizes the toy after teleportation to prevent jittering
    /// </summary>
    private IEnumerator StabilizeAfterTeleport()
    {
        // Wait a frame for physics to update
        yield return null;
        
        // Make sure we're still at the correct destination point (might have moved due to physics)
        Transform targetPoint = hasSecondTeleported ? destinationPoint2 : destinationPoint;
        
        if (targetPoint != null)
        {
            transform.position = targetPoint.position;
            
            if ((hasSecondTeleported && resetRotationOnSecondTeleport) || 
                (!hasSecondTeleported && resetRotationOnTeleport))
            {
                transform.rotation = targetPoint.rotation;
            }
        }
        
        // Wait another frame
        yield return null;
        
        // Ignore collisions with other toys for a short time to prevent jittering
        if (makeKinematicAfterTeleport && myColliders != null && myColliders.Length > 0)
        {
            // Find all nearby toys
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 1.0f);
            
            foreach (Collider myCol in myColliders)
            {
                foreach (Collider otherCol in nearbyColliders)
                {
                    // Skip self-collision
                    if (otherCol.gameObject == gameObject)
                        continue;
                        
                    // Temporarily ignore collision with other toys
                    if (otherCol.GetComponent<ToyController>() != null)
                    {
                        Physics.IgnoreCollision(myCol, otherCol, true);
                        
                        // Schedule to re-enable collision after some time
                        StartCoroutine(ReenableCollision(myCol, otherCol, 1.0f));
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Re-enables collision between two colliders after a delay
    /// </summary>
    private IEnumerator ReenableCollision(Collider col1, Collider col2, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Only re-enable if both colliders still exist
        if (col1 != null && col2 != null)
        {
            Physics.IgnoreCollision(col1, col2, false);
        }
    }
    
    /// <summary>
    /// Sends haptic feedback to the controller
    /// </summary>
    private void SendHapticFeedback(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        if (interactor == null) return;
        
        XRBaseController controller = null;
        
        // Try to get controller from interactor
        if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
        {
            controller = controllerInteractor.xrController;
        }
        
        if (controller != null)
        {
            controller.SendHapticImpulse(hapticIntensity, hapticDuration);
            Debug.Log($"Sent haptic feedback for golden teddy grab: {hapticIntensity} intensity for {hapticDuration}s");
        }
    }
    
    /// <summary>
    /// Force the toy to teleport to destination (can be called from other scripts)
    /// </summary>
    public void ForceTeleport()
    {
        hasTeleported = false; // Reset flag to allow teleportation
        hasSecondTeleported = false; // Reset second teleport flag
        TeleportToDestination();
    }
    
    /// <summary>
    /// Force the toy to teleport directly to the second destination
    /// </summary>
    public void ForceSecondTeleport()
    {
        if (destinationPoint2 != null)
        {
            // Cancel any pending teleports
            if (secondTeleportCoroutine != null)
            {
                StopCoroutine(secondTeleportCoroutine);
                secondTeleportCoroutine = null;
            }
            
            // Mark as teleported
            hasTeleported = true;
            hasSecondTeleported = true;
            
            // Teleport directly to second destination
            if (resetRotationOnSecondTeleport)
            {
                transform.SetPositionAndRotation(
                    destinationPoint2.position,
                    destinationPoint2.rotation
                );
            }
            else
            {
                transform.position = destinationPoint2.position;
            }
            
            // Make sure physics is set correctly
            if (rb != null)
            {
                // Set kinematic to false if gravity should be used
                rb.isKinematic = !useGravityAfterSecondTeleport;
                rb.useGravity = useGravityAfterSecondTeleport;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                // Set appropriate collision detection mode for dynamic objects
                if (useGravityAfterSecondTeleport)
                {
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                }
            }
            
            // Stabilize
            StartCoroutine(StabilizeAfterTeleport());
            
            Debug.Log($"Force teleported {gameObject.name} to second destination");
        }
        else
        {
            Debug.LogWarning($"Cannot force teleport to second destination - destinationPoint2 is not set");
        }
    }
    
    /// <summary>
    /// Check if this toy is currently in the drop zone
    /// </summary>
    public bool IsInDropZone()
    {
        if (dropZoneCollider == null)
        {
            // Can't check without a reference
            return false;
        }
        
        // Check if the toy's collider is overlapping with the drop zone
        Collider toyCollider = GetComponent<Collider>();
        if (toyCollider != null)
        {
            // Simple bounds check
            return toyCollider.bounds.Intersects(dropZoneCollider.bounds);
        }
        
        return false;
    }
    
    /// <summary>
    /// Reset teleport flags to allow the toy to be teleported again
    /// </summary>
    public void ResetTeleportFlags()
    {
        hasTeleported = false;
        hasSecondTeleported = false;
        
        // Cancel any pending teleports
        if (secondTeleportCoroutine != null)
        {
            StopCoroutine(secondTeleportCoroutine);
            secondTeleportCoroutine = null;
        }
        
        Debug.Log($"Reset teleport flags for {gameObject.name}");
    }
    
    /// <summary>
    /// Reset the toy to its initial position
    /// </summary>
    public void ResetToInitialPosition()
    {
        // Make sure we have a valid initial position
        if (initialPosition == Vector3.zero && storeInitialPosition)
        {
            Debug.LogWarning($"Initial position for {gameObject.name} is Vector3.zero. This might be incorrect. Setting current position as initial.");
            StoreInitialPosition();
        }
        
        // Reset teleport flags
        ResetTeleportFlags();
        
        // Stop any ongoing coroutines
        StopAllCoroutines();
        
        // Make kinematic temporarily to prevent physics while teleporting
        bool wasKinematic = false;
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Teleport to initial position
        transform.position = initialPosition;
        
        // Use either initial or random rotation
        if (randomRotationOnReset)
        {
            transform.rotation = Quaternion.Euler(
                Random.Range(0, 360),
                Random.Range(0, 360),
                Random.Range(0, 360)
            );
        }
        else
        {
            transform.rotation = initialRotation;
        }
        
        // Restore physics
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }
        
        Debug.Log($"Reset {gameObject.name} to initial position: {initialPosition}");
    }
}

