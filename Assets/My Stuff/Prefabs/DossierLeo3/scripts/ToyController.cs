using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ToyController : MonoBehaviour
{
    [Header("Primary Teleportation")]
    public Transform destinationPoint;
    
    public Collider dropZoneCollider;
    public string dropZoneTag = "DropZone";
    
    public bool resetRotationOnTeleport = true;
    
    public bool makeKinematicAfterTeleport = true;
    
    [Header("Secondary Teleportation")]
    public Transform destinationPoint2;
    
    [Range(0f, 10f)]
    public float secondTeleportDelay = 0f; // delay before teleportation
    
    public bool resetRotationOnSecondTeleport = true;
    
    public bool useGravityAfterSecondTeleport = true; //enable gravoty after 2nd teleportation
    
    [Header("Initial Position")]
    public bool storeInitialPosition = true;
    
    public bool randomRotationOnReset = true;
    
    [Header("Golden Teddy Settings")]
    public bool isGoldenTeddy = false; // check if the toy is golden teddy
    
    [Header("Haptic Feedback")]
    [Range(0f, 1f)]
    public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.2f;
    
    [Header("Audio")]
    public AudioClip goldenTeddySound;
    public AudioClip secondTeleportSound;
    
    // Privates
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool hasTeleported = false;
    private bool hasSecondTeleported = false;
    private Collider[] myColliders;
    private Coroutine secondTeleportCoroutine;
    
    // Initial position storage
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    // method to initializes component + sets up audio, and store the toys initial position
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
        }
        
        // Validate drop zone reference
        if (dropZoneCollider == null)
        {
        }
        
        // Store initial position and rotation
        if (storeInitialPosition)
        {
            StoreInitialPosition();
        }
    }
    
    // ensures the initial position and rotation are stored at the start of the game
    void Start()
    {
        // Store initial position if not done in Awake
        if (storeInitialPosition && initialPosition == Vector3.zero)
        {
            StoreInitialPosition();
        }
    }
    
    /// Stores the current position and rotation as the initial state
    public void StoreInitialPosition()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }
    
    // method that cancels any ongoing teleport coroutines when the toy is disabled
    void OnDisable()
    {
        // Cancel any pending teleports if object is disabled
        if (secondTeleportCoroutine != null)
        {
            StopCoroutine(secondTeleportCoroutine);
            secondTeleportCoroutine = null;
        }
    }
    
    // called when the toy is grabbed by the socket interactor
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
            
        }
    }
    
    // called when the toy enters the drop zone
    private void OnTriggerEnter(Collider other)
    {
        // Skip if already teleported to prevent multiple teleports
        if (hasTeleported)
            return;
            
        // Check if this is the specific drop zone collider we referenced
        if (dropZoneCollider != null && other == dropZoneCollider)
        {
            TeleportToDestination();
            return;
        }
        
        // Otherwise check by tag
        if (other.CompareTag(dropZoneTag))
        {
            TeleportToDestination();
        }
    }
    
    // Teleports the toy to the destination point 1 handles all the complex physics 
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
            
        }
        else
        {
        }
    }
    
    // Teleports the toy to the 2nd destination after a delay same as before but for the win table
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
    
    // to stabilize the toy after teleportation 
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
    
    // reenables collision between two colliders after a delay
    private IEnumerator ReenableCollision(Collider col1, Collider col2, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Only re-enable if both colliders still exist
        if (col1 != null && col2 != null)
        {
            Physics.IgnoreCollision(col1, col2, false);
        }
    }
    
    // for haptic feedback to the controller
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
        }
    }
    
    // Force the toy to teleport to destination if the other doesnt work
    public void ForceTeleport()
    {
        hasTeleported = false; // Reset flag to allow teleportation
        hasSecondTeleported = false; // Reset second teleport flag
        TeleportToDestination();
    }
    
    // Force the toy to teleport directly to the second destination
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
            
        }
        else
        {
        }
    }
    
    // to check if toy is currently in the drop zone
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
    
    // Reset teleport flags to allow the toy to be teleported again
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
        
    }
    
    // back the toy to its initial position
    public void ResetToInitialPosition()
    {
        // Make sure we have a valid initial position
        if (initialPosition == Vector3.zero && storeInitialPosition)
        {
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
        
    }
}

