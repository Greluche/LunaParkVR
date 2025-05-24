using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class XRJoystickController : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    [Header("References")]
    public Transform handle;
    public ClawScript clawController;
    
    // Haptic feedback device - can be assigned in the inspector
    [Tooltip("The controller that will receive haptic feedback. Assign this in the inspector.")]
    public Object hapticFeedbackDevice;
    
    [Header("Visual Feedback")]
    [Tooltip("Change the material color when joystick is grabbed")]
    public bool changeColorWhenGrabbed = true;
    [Tooltip("Renderer to change color on (leave empty to find automatically)")]
    public Renderer joystickRenderer;
    [Tooltip("Color when joystick is grabbed")]
    public Color selectedColor = new Color(0.2f, 0.8f, 0.2f, 1.0f); // Default bright green
    [Tooltip("Default color when not grabbed")]
    public Color defaultColor = new Color(0.5f, 0.5f, 0.5f, 1.0f); // Default gray
    [Tooltip("Material index to change color (for renderers with multiple materials)")]
    public int materialIndex = 0;
    
    [Header("Settings")]
    public float maxAngle = 30f;
    public float deadZone = 0.1f;
    public float returnSpeed = 5f;
    
    [Header("Direction Controls")]
    [Tooltip("Set to true to enable left/right movement (X axis)")]
    public bool enableXMovement = true;
    [Tooltip("Set to true to enable forward/back movement (Z axis)")]
    public bool enableZMovement = true;
    [Tooltip("Set to true to invert left/right movement")]
    public bool invertXAxis = false;
    [Tooltip("Set to true to invert forward/back movement")]
    public bool invertZAxis = false;
    
    [Header("Haptic Feedback")]
    [Tooltip("Enable haptic feedback for controller")]
    public bool enableHaptics = true;
    [Tooltip("Vibration strength when grabbing the joystick")]
    [Range(0f, 1f)]
    public float grabHapticIntensity = 0.3f;
    [Tooltip("Vibration strength when releasing the joystick")]
    [Range(0f, 1f)]
    public float releaseHapticIntensity = 0.2f;
    [Tooltip("Vibration strength when hitting machine boundaries")]
    [Range(0f, 1f)]
    public float boundaryHapticIntensity = 0.7f;
    [Tooltip("Duration of boundary haptic pulse in seconds")]
    public float boundaryHapticDuration = 0.1f;
    [Tooltip("Enable continuous subtle haptic feedback during movement")]
    public bool enableMovementHaptics = true;
    [Tooltip("Strength of continuous movement haptic feedback")]
    [Range(0f, 0.5f)]
    public float movementHapticIntensity = 0.1f;
    
    // Internal haptic variables
    private float hapticCooldown = 0f;
    private const float HAPTIC_COOLDOWN_TIME = 0.2f; // Prevent haptic spam
    private float movementHapticTimer = 0f;
    private float movementHapticInterval = 0.2f;
    
    [Header("Debug")]
    public bool showDebugVisuals = true;
    
    // Internal variables
    private Transform grabbingHand;
    private string currentDirection = "";
    private Vector3 initialHandPosition;
    private Vector3 initialJoystickPosition;
    private Vector3 initialHandleRotation = Vector3.zero;
    private bool isLocked = false; // Toggle to lock on one axis for more stability
    private bool atBoundaryLastFrame = false;
    private Material joystickMaterial;
    private Color originalColor;
    private bool hasSetOriginalColor = false;

    protected override void Awake()
    {
        base.Awake();
        
        // Make sure joystick doesn't move when grabbed
        trackPosition = false;
        trackRotation = false;
        throwOnDetach = false;
        
        // Store the initial joystick position to prevent it from moving
        initialJoystickPosition = transform.position;
        
        // Store initial handle rotation if handle exists
        if (handle != null)
        {
            initialHandleRotation = handle.localRotation.eulerAngles;
        }
        
        // Find joystick renderer if not assigned
        SetupRenderer();
        
        // Log which axes are enabled
        string enabledAxes = "";
        if (enableXMovement) enabledAxes += "X";
        if (enableZMovement) enabledAxes += (enabledAxes.Length > 0 ? " and Z" : "Z");
        if (enabledAxes.Length == 0) enabledAxes = "none (both axes disabled)";
        
        Debug.Log($"Joystick controller initialized with enabled axes: {enabledAxes}");
    }
    
    void SetupRenderer()
    {
        // Find renderer if not assigned and color change is enabled
        if (changeColorWhenGrabbed)
        {
            if (joystickRenderer == null)
            {
                // Try to find renderer on this object
                joystickRenderer = GetComponent<Renderer>();
                
                // If not found, try to find on handle
                if (joystickRenderer == null && handle != null)
                {
                    joystickRenderer = handle.GetComponent<Renderer>();
                }
                
                // If still not found, try children
                if (joystickRenderer == null)
                {
                    joystickRenderer = GetComponentInChildren<Renderer>();
                }
                
                if (joystickRenderer != null && showDebugVisuals)
                {
                    Debug.Log($"Found joystick renderer: {joystickRenderer.name}");
                }
            }
            
            // Get material and store original color
            if (joystickRenderer != null)
            {
                if (joystickRenderer.materials.Length > materialIndex)
                {
                    joystickMaterial = joystickRenderer.materials[materialIndex];
                    if (joystickMaterial != null)
                    {
                        originalColor = joystickMaterial.color;
                        hasSetOriginalColor = true;
                        if (showDebugVisuals)
                        {
                            Debug.Log($"Original joystick color: {originalColor}");
                        }
                    }
                }
                else if (showDebugVisuals)
                {
                    Debug.LogWarning($"Material index {materialIndex} out of range. Renderer has {joystickRenderer.materials.Length} materials.");
                }
            }
        }
    }

    void Update()
    {
        // Force joystick to stay at initial position
        transform.position = initialJoystickPosition;
        
        // Always keep the handle in its initial rotation
        if (handle != null)
        {
            handle.localRotation = Quaternion.Euler(initialHandleRotation);
        }
        
        if (isSelected && grabbingHand != null && clawController != null && clawController.CanControl())
        {
            // Get local position of hand relative to joystick
            Vector3 local = transform.InverseTransformPoint(grabbingHand.position);
            float dx = local.x;
            float dz = local.z;
            
            // Draw debug rays showing the raw input direction
            if (showDebugVisuals)
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(dx, 0, dz).normalized) * 0.2f, Color.red, 0.01f);
                
                // Draw X component in blue if enabled, gray if disabled
                Color xColor = enableXMovement ? Color.blue : Color.gray;
                Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(dx, 0, 0).normalized) * 0.15f, xColor, 0.01f);
                
                // Draw Z component in green if enabled, gray if disabled
                Color zColor = enableZMovement ? Color.green : Color.gray;
                Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(0, 0, dz).normalized) * 0.15f, zColor, 0.01f);
            }
            
            // Log raw values occasionally
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"Joystick raw values - dx: {dx:F3}, dz: {dz:F3}");
            }
            
            // Process movement
            ProcessJoystickMovement(dx, dz);
            
            // Update haptic cooldown
            if (hapticCooldown > 0)
            {
                hapticCooldown -= Time.deltaTime;
            }
            
            // Apply continuous haptic feedback during movement if enabled
            if (enableHaptics && enableMovementHaptics && hapticFeedbackDevice != null)
            {
                bool isMoving = false;
                
                if (enableXMovement && Mathf.Abs(dx) > deadZone)
                    isMoving = true;
                    
                if (enableZMovement && Mathf.Abs(dz) > deadZone)
                    isMoving = true;
                    
                if (isMoving)
                {
                    movementHapticTimer -= Time.deltaTime;
                    if (movementHapticTimer <= 0f)
                    {
                        // Calculate intensity based on how far the joystick is pushed
                        float intensity = movementHapticIntensity * 
                            Mathf.Max(
                                enableXMovement ? Mathf.Abs(dx) : 0f, 
                                enableZMovement ? Mathf.Abs(dz) : 0f
                            );
                        SendHapticFeedback(intensity, 0.05f);
                        movementHapticTimer = movementHapticInterval;
                    }
                }
            }
        }
        else if (!isSelected)
        {
            // Reset lock state when released
            isLocked = false;
            atBoundaryLastFrame = false;
        }
    }
    
    private void ProcessJoystickMovement(float dx, float dz)
    {
        string newDirection = "";
        Vector3 moveDir = Vector3.zero;
        
        // Determine if we should lock to a single axis for stability
        // If movement exceeds deadzone in both axes, lock to the dominant one
        if (!isLocked && enableXMovement && enableZMovement && 
            Mathf.Abs(dx) > deadZone && Mathf.Abs(dz) > deadZone)
        {
            // Lock to the axis with the larger input
            isLocked = true;
            if (Mathf.Abs(dx) >= Mathf.Abs(dz))
            {
                // Lock to X-axis
                if (showDebugVisuals)
                    Debug.Log("Joystick locked to X-axis");
            }
            else
            {
                // Lock to Z-axis
                if (showDebugVisuals)
                    Debug.Log("Joystick locked to Z-axis");
            }
        }
        
        // Process X-axis movement (left/right) if enabled
        if (enableXMovement && Mathf.Abs(dx) > deadZone && (!isLocked || Mathf.Abs(dx) >= Mathf.Abs(dz)))
        {
            // Apply inversion if needed
            float dirX = invertXAxis ? -dx : dx;
            
            // Add to direction string
            newDirection += dirX > 0 ? "right" : "left";
            
            // Scale magnitude based on how far joystick is pushed
            float normalizedMagnitude = Mathf.Clamp01((Mathf.Abs(dx) - deadZone) / (1.0f - deadZone));
            moveDir.x = dirX > 0 ? normalizedMagnitude : -normalizedMagnitude;
        }
        
        // Process Z-axis movement (forward/back) if enabled
        if (enableZMovement && Mathf.Abs(dz) > deadZone && (!isLocked || Mathf.Abs(dz) > Mathf.Abs(dx)))
        {
            // Apply inversion if needed
            float dirZ = invertZAxis ? -dz : dz;
            
            // Add to direction string
            if (newDirection.Length > 0) newDirection += "+";
            newDirection += dirZ > 0 ? "forward" : "back";
            
            // Scale magnitude based on how far joystick is pushed
            float normalizedMagnitude = Mathf.Clamp01((Mathf.Abs(dz) - deadZone) / (1.0f - deadZone));
            moveDir.z = dirZ > 0 ? normalizedMagnitude : -normalizedMagnitude;
        }
        
        // If we return to deadzone in both axes, unlock
        if (isLocked && Mathf.Abs(dx) <= deadZone && Mathf.Abs(dz) <= deadZone)
        {
            isLocked = false;
            if (showDebugVisuals)
                Debug.Log("Joystick unlocked");
        }
        
        // Check if we're at a boundary for haptic feedback
        bool atBoundaryX = false;
        bool atBoundaryZ = false;
        
        if (clawController != null)
        {
            // Check X boundary
            if (enableXMovement && Mathf.Abs(moveDir.x) > 0.01f)
            {
                Vector3 testPos = clawController.transform.localPosition;
                testPos.x += moveDir.x * clawController.moveSpeed * Time.deltaTime;
                
                if ((moveDir.x > 0 && testPos.x >= clawController.machineBoundsX.y) || 
                    (moveDir.x < 0 && testPos.x <= clawController.machineBoundsX.x))
                {
                    atBoundaryX = true;
                }
            }
            
            // Check Z boundary
            if (enableZMovement && Mathf.Abs(moveDir.z) > 0.01f)
            {
                Vector3 testPos = clawController.transform.localPosition;
                testPos.z += moveDir.z * clawController.moveSpeed * Time.deltaTime;
                
                if ((moveDir.z > 0 && testPos.z >= clawController.machineBoundsZ.y) || 
                    (moveDir.z < 0 && testPos.z <= clawController.machineBoundsZ.x))
                {
                    atBoundaryZ = true;
                }
            }
        }
        
        // Send haptic feedback if we hit a boundary
        bool atBoundary = atBoundaryX || atBoundaryZ;
        if (enableHaptics && atBoundary && !atBoundaryLastFrame && hapticCooldown <= 0)
        {
            SendHapticFeedback(boundaryHapticIntensity, boundaryHapticDuration);
            hapticCooldown = HAPTIC_COOLDOWN_TIME;
            
            if (showDebugVisuals)
            {
                Debug.Log("Boundary haptic feedback sent: " + 
                    (atBoundaryX ? "X-axis " : "") + 
                    (atBoundaryZ ? "Z-axis" : ""));
            }
        }
        atBoundaryLastFrame = atBoundary;
        
        // Send movement to claw controller if we have a direction
        if (moveDir != Vector3.zero)
        {
            clawController.SetDirection(moveDir);
            
            // Update direction state if changed
            if (newDirection != currentDirection)
            {
                Debug.Log("Joystick direction changed to: " + newDirection);
                currentDirection = newDirection;
            }
        }
        else if (currentDirection != "")
        {
            // Stop if we were previously moving but now in deadzone
            currentDirection = "";
            clawController.StopMovement();
            Debug.Log("Stopping movement - in deadzone");
        }
        
        // Update handle visuals to match movement
        UpdateHandleVisuals(dx, dz);
    }
    
    private void UpdateHandleVisuals(float dx, float dz)
    {
        if (handle != null)
        {
            // Always keep the handle in its initial rotation, regardless of joystick movement
            handle.localRotation = Quaternion.Euler(initialHandleRotation);
            
            // Log if debugging is enabled
            if (showDebugVisuals && Time.frameCount % 60 == 0)
            {
                Debug.Log("Keeping joystick handle at initial rotation");
            }
        }
    }
    
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        grabbingHand = args.interactorObject.transform;
        
        // Change color when grabbed
        if (changeColorWhenGrabbed && joystickMaterial != null)
        {
            joystickMaterial.color = selectedColor;
        }
        
        // Send haptic feedback when grabbing the joystick
        if (enableHaptics)
        {
            SendHapticFeedback(grabHapticIntensity, 0.1f);
            
            if (showDebugVisuals)
            {
                Debug.Log("Grab haptic feedback sent");
            }
        }
        
        // Store initial hand position for relative movement
        initialHandPosition = transform.InverseTransformPoint(grabbingHand.position);
        
        // Log when joystick is grabbed
        if (showDebugVisuals)
        {
            Debug.Log("Joystick grabbed");
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        
        // Reset color when released
        if (changeColorWhenGrabbed && joystickMaterial != null)
        {
            if (hasSetOriginalColor)
            {
                joystickMaterial.color = originalColor;
            }
            else
            {
                joystickMaterial.color = defaultColor;
            }
        }
        
        // Send haptic feedback when releasing joystick
        if (enableHaptics)
        {
            SendHapticFeedback(releaseHapticIntensity, 0.1f);
            
            if (showDebugVisuals)
            {
                Debug.Log("Release haptic feedback sent");
            }
        }
        
        grabbingHand = null;
        currentDirection = "";
        
        // Stop claw movement when joystick is released
        if (clawController != null)
        {
            clawController.StopMovement();
        }
        
        // Log when joystick is released
        if (showDebugVisuals)
        {
            Debug.Log("Joystick released");
        }
        
        // Reset lock state when released
        isLocked = false;
        atBoundaryLastFrame = false;
    }

    // Helper method to send haptic feedback that works with different controller types
    public void SendHapticFeedback(float intensity, float duration)
    {
        if (!enableHaptics || hapticFeedbackDevice == null) return;
        
        // Try different controller types based on what's available
        if (hapticFeedbackDevice is ActionBasedController actionController)
        {
            actionController.SendHapticImpulse(intensity, duration);
        }
        #pragma warning disable CS0618 // Disable obsolete warning
        else if (hapticFeedbackDevice is XRBaseController baseController)
        {
            baseController.SendHapticImpulse(intensity, duration);
        }
        #pragma warning restore CS0618
        else
        {
            Debug.LogWarning("Haptic feedback device type not supported. Please assign a compatible controller.");
        }
        
        if (showDebugVisuals)
        {
            Debug.Log($"Haptic feedback sent - Intensity: {intensity}, Duration: {duration}");
        }
    }
}