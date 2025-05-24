using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

/// <summary>
/// Alternative joystick controller that supports four-directional movement (left, right, forward, back)
/// without modifying the original XRJoystickController
/// </summary>
public class JoystickMovementScript : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Maximum angle the joystick can tilt")]
    public float maxTiltAngle = 25f;
    
    [Tooltip("How quickly the joystick returns to center")]
    public float returnSpeed = 5f;
    
    [Tooltip("Threshold to activate claw movement (0-1)")]
    [Range(0f, 1f)]
    public float movementThreshold = 0.1f;
    
    [Tooltip("Smoothing applied to joystick movement")]
    [Range(0f, 1f)]
    public float inputSmoothing = 0.1f;
    
    [Header("References")]
    [Tooltip("Transform used for joystick rotation")]
    public Transform pivotTransform;
    
    [Tooltip("Claw script to control")]
    public ClawMovementScript clawController;
    
    [Tooltip("Prevents movement past these angles")]
    public Vector2 forwardLimits = new Vector2(-25f, 25f); // Negative is forward, positive is back
    public Vector2 sidewaysLimits = new Vector2(-25f, 25f); // Negative is left, positive is right
    
    [Header("Haptic Feedback")]
    public bool enableHaptics = true;
    [Range(0f, 1f)]
    public float grabHapticIntensity = 0.2f;
    public float grabHapticDuration = 0.1f;
    
    [Header("Debug")]
    public bool showDebugVisuals = true;
    public Color normalColor = Color.white;
    public Color grabbedColor = Color.green;
    
    // XR Interaction components
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
    
    // State tracking
    private bool isGrabbed = false;
    private Quaternion originalRotation;
    private Vector2 currentDirection = Vector2.zero;
    private Vector2 smoothedDirection = Vector2.zero;
    
    // Haptic feedback
    public XRBaseController hapticFeedbackDevice;
    
    // Visuals
    private MeshRenderer joystickRenderer;
    private Material joystickMaterial;
    private Color originalColor;
    
    void Start()
    {
        // Find pivot transform if not assigned
        if (pivotTransform == null)
        {
            pivotTransform = transform;
            Debug.LogWarning("No pivot transform assigned, using this transform.");
        }
        
        // Store original rotation
        originalRotation = pivotTransform.localRotation;
        
        // Set up XR interaction components
        SetupInteraction();
        
        // Set up visuals
        SetupVisuals();
    }
    
    void Update()
    {
        // Handle joystick return to center when not grabbed
        if (!isGrabbed)
        {
            ReturnToCenter();
        }
        else
        {
            // Apply movement direction to claw controller
            if (clawController != null && clawController.CanControl())
            {
                clawController.SetMovementInput(smoothedDirection);
            }
        }
    }
    
    void SetupInteraction()
    {
        // Get or add XR Grab Interactable
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            
            // Configure for joystick behavior
            grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
            grabInteractable.trackPosition = false;
            grabInteractable.trackRotation = false;
            grabInteractable.throwOnDetach = false;
            
            Debug.Log("Added XR Grab Interactable component to joystick");
        }
        
        // Subscribe to events
        grabInteractable.selectEntered.AddListener(OnJoystickGrabbed);
        grabInteractable.selectExited.AddListener(OnJoystickReleased);
    }
    
    void SetupVisuals()
    {
        // Get renderer if available
        joystickRenderer = GetComponentInChildren<MeshRenderer>();
        if (joystickRenderer != null)
        {
            joystickMaterial = joystickRenderer.material;
            originalColor = joystickMaterial.color;
        }
    }
    
    void OnJoystickGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;
        
        // Store haptic device
        UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor = interactor as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor;
        if (controllerInteractor != null)
        {
            hapticFeedbackDevice = controllerInteractor.xrController;
        }
        
        // Visual feedback
        UpdateJoystickVisuals(true);
        
        if (showDebugVisuals)
        {
            Debug.Log("Joystick grabbed");
        }
    }
    
    void OnJoystickReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        
        // Reset movement direction
        currentDirection = Vector2.zero;
        smoothedDirection = Vector2.zero;
        
        // Stop the claw from moving
        if (clawController != null)
        {
            clawController.SetMovementInput(Vector2.zero);
        }
        
        // Visual feedback
        UpdateJoystickVisuals(false);
        
        if (showDebugVisuals)
        {
            Debug.Log("Joystick released");
        }
    }
    
    void LateUpdate()
    {
        if (isGrabbed && interactor != null)
        {
            // Calculate joystick tilt based on controller position
            CalculateJoystickTilt();
        }
    }
    
    void CalculateJoystickTilt()
    {
        // Get the controller position relative to joystick base
        Vector3 controllerPos = interactor.transform.position;
        Vector3 joystickPos = transform.position;
        Vector3 relativePos = transform.InverseTransformPoint(controllerPos);
        
        // Calculate forward/back tilt (Z axis in local space)
        float forwardTilt = Mathf.Clamp(relativePos.z * maxTiltAngle, forwardLimits.x, forwardLimits.y);
        
        // Calculate left/right tilt (X axis in local space)
        float sidewaysTilt = Mathf.Clamp(relativePos.x * maxTiltAngle, sidewaysLimits.x, sidewaysLimits.y);
        
        // Create the target rotation
        Quaternion targetRotation = Quaternion.Euler(forwardTilt, 0, -sidewaysTilt);
        
        // Apply the rotation
        pivotTransform.localRotation = targetRotation;
        
        // Calculate movement direction (normalized -1 to 1)
        float forwardFactor = -forwardTilt / forwardLimits.y; // Invert so forward is positive
        float sidewaysFactor = sidewaysTilt / sidewaysLimits.y;
        
        // Apply deadzone
        forwardFactor = Mathf.Abs(forwardFactor) > movementThreshold ? forwardFactor : 0f;
        sidewaysFactor = Mathf.Abs(sidewaysFactor) > movementThreshold ? sidewaysFactor : 0f;
        
        // Update current direction
        currentDirection = new Vector2(sidewaysFactor, forwardFactor);
        
        // Smooth the direction
        smoothedDirection = Vector2.Lerp(smoothedDirection, currentDirection, 1f - inputSmoothing);
        
        // Send haptics when crossing threshold
        if (enableHaptics && hapticFeedbackDevice != null)
        {
            bool wasMoving = (Mathf.Abs(smoothedDirection.x) > movementThreshold || 
                             Mathf.Abs(smoothedDirection.y) > movementThreshold);
            
            bool isMoving = (Mathf.Abs(currentDirection.x) > movementThreshold || 
                            Mathf.Abs(currentDirection.y) > movementThreshold);
            
            if (!wasMoving && isMoving)
            {
                SendHapticFeedback(grabHapticIntensity, grabHapticDuration);
            }
        }
        
        // Debug output
        if (showDebugVisuals && (currentDirection.x != 0 || currentDirection.y != 0))
        {
            Debug.DrawRay(transform.position, new Vector3(currentDirection.x, 0, currentDirection.y) * 0.2f, Color.yellow);
        }
    }
    
    void ReturnToCenter()
    {
        // Gradually return to original rotation
        pivotTransform.localRotation = Quaternion.Slerp(
            pivotTransform.localRotation, 
            originalRotation, 
            returnSpeed * Time.deltaTime
        );
        
        // Reset direction
        currentDirection = Vector2.zero;
        smoothedDirection = Vector2.Lerp(smoothedDirection, Vector2.zero, 1f - inputSmoothing);
        
        // Update the claw if still slightly moving
        if (clawController != null && smoothedDirection.sqrMagnitude > 0.01f)
        {
            clawController.SetMovementInput(smoothedDirection);
        }
    }
    
    void UpdateJoystickVisuals(bool grabbed)
    {
        if (joystickRenderer != null && joystickMaterial != null)
        {
            joystickMaterial.color = grabbed ? grabbedColor : originalColor;
        }
    }
    
    public void SendHapticFeedback(float intensity, float duration)
    {
        if (hapticFeedbackDevice != null)
        {
            hapticFeedbackDevice.SendHapticImpulse(intensity, duration);
        }
    }
    
    public Vector2 GetJoystickDirection()
    {
        return smoothedDirection;
    }
    
    void OnDrawGizmos()
    {
        if (showDebugVisuals)
        {
            // Draw movement direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, new Vector3(smoothedDirection.x, 0, smoothedDirection.y) * 0.2f);
            
            // Draw limits
            Gizmos.color = Color.red;
            
            // Forward/back limits
            float limitLength = 0.1f;
            Gizmos.DrawRay(transform.position, transform.forward * (forwardLimits.y/maxTiltAngle) * limitLength);
            Gizmos.DrawRay(transform.position, -transform.forward * (Mathf.Abs(forwardLimits.x)/maxTiltAngle) * limitLength);
            
            // Left/right limits
            Gizmos.DrawRay(transform.position, transform.right * (sidewaysLimits.y/maxTiltAngle) * limitLength);
            Gizmos.DrawRay(transform.position, -transform.right * (Mathf.Abs(sidewaysLimits.x)/maxTiltAngle) * limitLength);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnJoystickGrabbed);
            grabInteractable.selectExited.RemoveListener(OnJoystickReleased);
        }
    }
} 