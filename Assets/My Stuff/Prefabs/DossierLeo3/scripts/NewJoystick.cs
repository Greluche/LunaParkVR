// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;
// using System;

// [RequireComponent(typeof(Collider))]
// public class NewJoystick : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
// {
//     [Header("Claw Control")]
//     public ClawScript clawController;

//     [Header("Visual Feedback")]
//     public bool changeColorWhenGrabbed = true;
//     public Renderer joystickRenderer;
//     public Color selectedColor = new Color(0.2f, 0.8f, 0.2f, 1f);
//     public Color defaultColor = new Color(0.5f, 0.5f, 0.5f, 1f);
//     public int materialIndex = 0;

//     [Header("Joystick Settings")]
//     public float deadZone = 5f;
//     public float tiltToMoveMultiplier = 0.02f;
//     public bool enableXMovement = true;
//     public bool enableZMovement = true;

//     [Header("Auto-Centering")]
//     public float returnSpeed = 3f;
//     private Quaternion initialRotation;
//     private bool isBeingHeld = false;

//     [Header("Haptic Feedback")]
//     public bool enableHaptics = true;
//     public float grabHapticIntensity = 0.3f;
//     public float releaseHapticIntensity = 0.2f;
//     public float movementHapticIntensity = 0.1f;
//     public bool enableMovementHaptics = true;
//     public float movementHapticInterval = 0.2f;
//     private float movementHapticTimer;
//     public UnityEngine.Object hapticFeedbackDevice;

//     [Header("Debug")]
//     public bool showDebugVisuals = true;

//     // Internal state
//     private Transform grabbingHand;
//     private string currentDirection = "";
//     private Material joystickMaterial;
//     private Color originalColor;
//     private bool hasSetOriginalColor = false;

//     protected override void Awake()
//     {
//         base.Awake();
//         trackPosition = false;
//         trackRotation = false;
//         throwOnDetach = false;

//         initialRotation = transform.rotation;
//         SetupRenderer();
//     }

//     void SetupRenderer()
//     {
//         if (!changeColorWhenGrabbed) return;
//         if (joystickRenderer == null) joystickRenderer = GetComponentInChildren<Renderer>();
//         if (joystickRenderer != null)
//         {
//             var mats = joystickRenderer.materials;
//             if (materialIndex < mats.Length)
//             {
//                 joystickMaterial = mats[materialIndex];
//                 originalColor = joystickMaterial.color;
//                 hasSetOriginalColor = true;
//             }
//         }
//     }

//     void Update()
//     {
//         // Auto-center when not held
//         if (!isBeingHeld)
//             transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * returnSpeed);

//         if (isSelected && grabbingHand != null && clawController != null)
//         {
//             isBeingHeld = true;
//             // SCRIPT 2 MOVEMENT HANDLING
//             // Rotate the joystick object toward the hand
//             transform.LookAt(grabbingHand.position, transform.up);

//             // Read local tilt angles
//             Vector3 angles = transform.localEulerAngles;
//             float forwardBackwardTilt = (angles.x > 180f ? angles.x - 360f : angles.x);
//             float sideToSideTilt = (angles.z > 180f ? angles.z - 360f : angles.z);

//             if (showDebugVisuals && Time.frameCount % 60 == 0)
//                 Debug.Log($"Tilt: FB={forwardBackwardTilt:F1}, SS={sideToSideTilt:F1}");

//             // Convert tilt into movement
//             float moveX = Mathf.Abs(sideToSideTilt) > deadZone && enableXMovement ? -sideToSideTilt * tiltToMoveMultiplier : 0f;
//             float moveZ = Mathf.Abs(forwardBackwardTilt) > deadZone && enableZMovement ? forwardBackwardTilt * tiltToMoveMultiplier : 0f;

//             Vector3 moveDir = new Vector3(moveX, 0f, moveZ);

//             if (moveDir != Vector3.zero)
//             {
//                 Debug.Log($"[Joystick] moveDir = {moveDir}");

//                 clawController.SetDirection(moveDir);
//                 ProvideMovementHaptics(moveDir);

//                 string newDir = moveX > 0 ? "Right" : moveX < 0 ? "Left"
//                                 : moveZ > 0 ? "Forward" : moveZ < 0 ? "Backward" : "";
//                 if (newDir != currentDirection)
//                 {
//                     Debug.Log("Joystick direction: " + newDir);
//                     currentDirection = newDir;
//                 }
//             }
//             else if (currentDirection != "")
//             {
//                 clawController.StopMovement();
//                 Debug.Log("Stopping movement - deadzone");
//                 currentDirection = "";
//             }
//         }
//         else
//         {
//             isBeingHeld = false;
//             movementHapticTimer = 0f;
//             if (currentDirection != "")
//                 clawController.StopMovement();
//             currentDirection = "";
//         }
//     }

//     protected override void OnSelectEntered(SelectEnterEventArgs args)
//     {
//         base.OnSelectEntered(args);
//         grabbingHand = args.interactorObject.transform;
//         isBeingHeld = true;

//         if (changeColorWhenGrabbed && joystickMaterial != null)
//             joystickMaterial.color = selectedColor;
//         if (enableHaptics)
//             SendHapticFeedback(grabHapticIntensity, 0.1f);
//     }

//     protected override void OnSelectExited(SelectExitEventArgs args)
//     {
//         base.OnSelectExited(args);
//         isBeingHeld = false;

//         if (changeColorWhenGrabbed && joystickMaterial != null)
//             joystickMaterial.color = hasSetOriginalColor ? originalColor : defaultColor;
//         if (enableHaptics)
//             SendHapticFeedback(releaseHapticIntensity, 0.1f);

//         clawController.StopMovement();
//         grabbingHand = null;
//     }

//     void ProvideMovementHaptics(Vector3 moveDir)
//     {
//         if (!enableHaptics || !enableMovementHaptics || hapticFeedbackDevice == null) return;
//         movementHapticTimer -= Time.deltaTime;
//         if (movementHapticTimer <= 0f)
//         {
//             float intensity = movementHapticIntensity * Mathf.Max(Mathf.Abs(moveDir.x), Mathf.Abs(moveDir.z));
//             SendHapticFeedback(intensity, movementHapticInterval * 0.5f);
//             movementHapticTimer = movementHapticInterval;
//         }
//     }

//     public void SendHapticFeedback(float intensity, float duration)
//     {
//         if (!enableHaptics || hapticFeedbackDevice == null) return;
//         if (hapticFeedbackDevice is ActionBasedController ac)
//             ac.SendHapticImpulse(intensity, duration);
// #pragma warning disable CS0618
//         else if (hapticFeedbackDevice is XRBaseController bc)
//             bc.SendHapticImpulse(intensity, duration);
// #pragma warning restore CS0618
//     }
// }
