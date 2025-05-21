using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ToyController : MonoBehaviour
{
    [Header("Settings")]
    public float weight = 1.0f; // Heavier toys are harder to grab
    
    [Header("Physics")]
    public bool useGravity = true; // Changed to true by default
    public bool checkColliderBounds = true; // Whether to validate collider bounds
    
    [Header("Audio")]
    public AudioClip grabSound;
    public AudioClip releaseSound;
    
    [Header("Debug")]
    public bool showDebugVisuals = true;
    public Color debugColor = Color.green;
    public bool logPhysicsInteractions = true;
    
    private Rigidbody rb;
    private Vector3 initialPosition; // Store the initial position
    private Quaternion initialRotation; // Store the initial rotation
    private Transform originalParent; // Store the original parent
    private AudioSource audioSource;
    private bool isGrabbed = false;
    private Collider mainCollider; // Reference to the main collider

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        // Get the first collider attached to this object
        mainCollider = GetComponent<Collider>();
        
        // Create audio source if one doesn't exist
        if (audioSource == null && (grabSound != null || releaseSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
        }
        
        // Store initial position, rotation and parent
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        originalParent = transform.parent;
        
        // Initialize physics
        if (rb != null)
        {
            rb.useGravity = useGravity;
            rb.isKinematic = false; // Start as non-kinematic
            
            // Ensure rigidbody mass matches the weight parameter
            rb.mass = weight;
        }
        
        // Setup XR Grab Interactable if present
        SetupXRGrabInteractable();
        
        // Validate colliders
        ValidateColliders();
    }
    
    void Start()
    {
        // Secondary validation after all components have initialized
        StartCoroutine(DelayedValidation());
    }
    
    IEnumerator DelayedValidation()
    {
        // Wait a frame to ensure everything is initialized
        yield return null;
        
        // Check if collider bounds match the visible mesh
        if (checkColliderBounds)
        {
            ValidateColliderBounds();
        }
    }
    
    void ValidateColliders()
    {
        // Check for colliders
        Collider[] colliders = GetComponents<Collider>();
        
        if (colliders.Length == 0)
        {
            Debug.LogError($"Toy {gameObject.name} has no colliders! Adding a box collider.");
            
            // Add a box collider if none exists
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            
            // Try to get mesh renderer to size the collider
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                boxCollider.center = renderer.bounds.center - transform.position;
                boxCollider.size = renderer.bounds.size;
            }
            
            mainCollider = boxCollider;
        }
        else if (showDebugVisuals)
        {
            Debug.Log($"Toy {gameObject.name} has {colliders.Length} colliders.");
            mainCollider = colliders[0];
        }
    }
    
    void ValidateColliderBounds()
    {
        if (mainCollider == null) return;
        
        // Get the renderer bounds
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            // Try to find a child renderer
            renderer = GetComponentInChildren<Renderer>();
            if (renderer == null) return;
        }
        
        Bounds rendererBounds = renderer.bounds;
        Bounds colliderBounds = mainCollider.bounds;
        
        // Compare the sizes
        float sizeDifference = Mathf.Abs(rendererBounds.size.magnitude - colliderBounds.size.magnitude);
        
        // If the collider is significantly smaller or larger than the renderer, warn
        if (sizeDifference > 0.1f * rendererBounds.size.magnitude)
        {
            Debug.LogWarning($"Toy {gameObject.name}'s collider may not match its visual size. " +
                             $"Renderer bounds: {rendererBounds.size}, Collider bounds: {colliderBounds.size}");
            
            if (showDebugVisuals)
            {
                // Draw both bounds in the scene for visualization
                StartCoroutine(VisualizeColliderMismatch(rendererBounds, colliderBounds));
            }
        }
    }
    
    IEnumerator VisualizeColliderMismatch(Bounds rendererBounds, Bounds colliderBounds)
    {
        // Visualize the mismatch for a few seconds
        float endTime = Time.time + 5.0f;
        
        while (Time.time < endTime)
        {
            // Draw renderer bounds in green
            Debug.DrawLine(rendererBounds.min, new Vector3(rendererBounds.max.x, rendererBounds.min.y, rendererBounds.min.z), Color.green);
            Debug.DrawLine(rendererBounds.min, new Vector3(rendererBounds.min.x, rendererBounds.max.y, rendererBounds.min.z), Color.green);
            Debug.DrawLine(rendererBounds.min, new Vector3(rendererBounds.min.x, rendererBounds.min.y, rendererBounds.max.z), Color.green);
            Debug.DrawLine(rendererBounds.max, new Vector3(rendererBounds.min.x, rendererBounds.max.y, rendererBounds.max.z), Color.green);
            Debug.DrawLine(rendererBounds.max, new Vector3(rendererBounds.max.x, rendererBounds.min.y, rendererBounds.max.z), Color.green);
            Debug.DrawLine(rendererBounds.max, new Vector3(rendererBounds.max.x, rendererBounds.max.y, rendererBounds.min.z), Color.green);
            
            // Draw collider bounds in red
            Debug.DrawLine(colliderBounds.min, new Vector3(colliderBounds.max.x, colliderBounds.min.y, colliderBounds.min.z), Color.red);
            Debug.DrawLine(colliderBounds.min, new Vector3(colliderBounds.min.x, colliderBounds.max.y, colliderBounds.min.z), Color.red);
            Debug.DrawLine(colliderBounds.min, new Vector3(colliderBounds.min.x, colliderBounds.min.y, colliderBounds.max.z), Color.red);
            Debug.DrawLine(colliderBounds.max, new Vector3(colliderBounds.min.x, colliderBounds.max.y, colliderBounds.max.z), Color.red);
            Debug.DrawLine(colliderBounds.max, new Vector3(colliderBounds.max.x, colliderBounds.min.y, colliderBounds.max.z), Color.red);
            Debug.DrawLine(colliderBounds.max, new Vector3(colliderBounds.max.x, colliderBounds.max.y, colliderBounds.min.z), Color.red);
            
            yield return null;
        }
    }
    
    // Called when the claw grabs this toy
    public void OnGrabbed(ClawScript claw)
    {
        isGrabbed = true;
        
        // Log physics state for debugging
        if (logPhysicsInteractions)
        {
            Debug.Log($"Toy {gameObject.name} grabbed: Position={transform.position}, Collider bounds={mainCollider?.bounds}");
        }
        
        // Play grab sound
        if (audioSource != null && grabSound != null)
        {
            audioSource.clip = grabSound;
            audioSource.Play();
        }
        
        if (showDebugVisuals)
        {
            Debug.Log($"Toy {gameObject.name} grabbed by claw");
        }
    }
    
    // Called when the claw releases this toy
    public void OnReleased(ClawScript claw)
    {
        isGrabbed = false;
        
        // Log physics state for debugging
        if (logPhysicsInteractions)
        {
            Debug.Log($"Toy {gameObject.name} released: Position={transform.position}, Velocity={rb?.linearVelocity}");
        }
        
        // Play release sound
        if (audioSource != null && releaseSound != null)
        {
            audioSource.clip = releaseSound;
            audioSource.Play();
        }
        
        if (showDebugVisuals)
        {
            Debug.Log($"Toy {gameObject.name} released by claw");
        }
    }
    
    // Called by XR Interaction Toolkit when toy is grabbed by player
    public void OnPlayerGrabbed(SelectEnterEventArgs args)
    {
        // Only play sound if not already grabbed by claw
        if (!isGrabbed && audioSource != null && grabSound != null)
        {
            audioSource.clip = grabSound;
            audioSource.Play();
        }
        
        if (showDebugVisuals)
        {
            Debug.Log($"Toy {gameObject.name} grabbed by player: {args.interactorObject.transform.name}");
        }
    }
    
    // Called by XR Interaction Toolkit when toy is released by player
    public void OnPlayerReleased(SelectExitEventArgs args)
    {
        // Only play sound if not already grabbed by claw
        if (!isGrabbed && audioSource != null && releaseSound != null)
        {
            audioSource.clip = releaseSound;
            audioSource.Play();
        }
        
        if (showDebugVisuals)
        {
            Debug.Log($"Toy {gameObject.name} released by player: {args.interactorObject.transform.name}");
        }
    }
    
    // Reset the toy to its initial state and position
    public void ResetToy()
    {
        transform.SetParent(originalParent);
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        isGrabbed = false;
    }
    
    // For XR Grab Interactable integration (if you're using it)
    public void SetupXRGrabInteractable()
    {
        var grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnPlayerGrabbed);
            grabInteractable.selectExited.AddListener(OnPlayerReleased);
        }
    }
    
    void OnDrawGizmos()
    {
        // Visualize if the toy is grabbed
        if (isGrabbed)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
        
        // Visualize collider for debugging
        if (showDebugVisuals && Application.isPlaying)
        {
            // Get the first collider if we don't have a reference yet
            if (mainCollider == null)
                mainCollider = GetComponent<Collider>();
                
            if (mainCollider != null)
            {
                // Draw collider bounds
                Gizmos.color = isGrabbed ? Color.green : debugColor;
                
                if (mainCollider is BoxCollider box)
                {
                    // Draw box collider
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(box.center, box.size);
                    Gizmos.matrix = oldMatrix;
                }
                else if (mainCollider is SphereCollider sphere)
                {
                    // Draw sphere collider
                    Vector3 center = transform.TransformPoint(sphere.center);
                    Gizmos.DrawWireSphere(center, sphere.radius * transform.lossyScale.x);
                }
                else if (mainCollider is CapsuleCollider capsule)
                {
                    // Draw capsule endpoints
                    Vector3 center = transform.TransformPoint(capsule.center);
                    float height = capsule.height * 0.5f;
                    float radius = capsule.radius;
                    
                    Vector3 dir = Vector3.up;
                    if (capsule.direction == 0) dir = Vector3.right;
                    else if (capsule.direction == 2) dir = Vector3.forward;
                    
                    dir = transform.TransformDirection(dir);
                    
                    Vector3 top = center + dir * height;
                    Vector3 bottom = center - dir * height;
                    
                    Gizmos.DrawWireSphere(top, radius);
                    Gizmos.DrawWireSphere(bottom, radius);
                    Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
                    Gizmos.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
                    Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
                    Gizmos.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
                }
            }
        }
    }
}
