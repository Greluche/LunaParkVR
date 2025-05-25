// using UnityEngine;
// using System;

// public class JoystickControl : MonoBehaviour
// {
//     [Header("Joystick Parts")]
//     public Transform topOfJoystick;

//     [Header("Claw Control")]
//     public ClawScript clawController; // Drag it in the Inspector
//     public bool enableXMovement = true;
//     public bool enableZMovement = true;

//     [Header("Joystick Settings")]
//     [SerializeField] private float forwardBackwardTilt = 0f;
//     [SerializeField] private float sideToSideTilt = 0f;
//     [SerializeField] private float deadZone = 5f;
//     [SerializeField] private float tiltToMoveMultiplier = 0.02f;

//     [Header("Auto-Centering")]
//     private Quaternion initialRotation;
//     private bool isBeingHeld = false;
//     [SerializeField] private float returnSpeed = 3f;

//     private string currentDirection = "";

//     void Start()
//     {
//         initialRotation = transform.rotation;
//     }

//     void Update()
//     {
//         // Compute local Euler angles
//         Vector3 angles = topOfJoystick.localEulerAngles;
//         forwardBackwardTilt = NormalizeAngle(angles.x);
//         sideToSideTilt = NormalizeAngle(angles.z);

//         // Convert tilt into movement direction
//         float moveX = Mathf.Abs(sideToSideTilt) > deadZone ? -sideToSideTilt * tiltToMoveMultiplier : 0f;
//         float moveZ = Mathf.Abs(forwardBackwardTilt) > deadZone ? forwardBackwardTilt * tiltToMoveMultiplier : 0f;

//         Vector3 moveDir = new Vector3(
//             enableXMovement ? moveX : 0f,
//             0f,
//             enableZMovement ? moveZ : 0f
//         );

//         // Send movement to claw controller
//         if (moveDir != Vector3.zero)
//         {
//             clawController.SetDirection(moveDir);

//             string newDir = moveDir.x > 0 ? "Right" :
//                             moveDir.x < 0 ? "Left" :
//                             moveDir.z > 0 ? "Forward" :
//                             moveDir.z < 0 ? "Backward" : "";

//             if (newDir != currentDirection)
//             {
//                 Debug.Log("Joystick direction changed to: " + newDir);
//                 currentDirection = newDir;
//             }
//         }
//         else if (currentDirection != "")
//         {
//             clawController.StopMovement();
//             currentDirection = "";
//             Debug.Log("Stopping movement - in deadzone");
//         }

//         // Reset rotation when not grabbed
//         if (!isBeingHeld)
//         {
//             transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * returnSpeed);
//         }
//     }

//     private void OnTriggerStay(Collider other)
//     {
//         Debug.Log("OnTriggerStay");
//         if (other.CompareTag("PlayerHand"))
//         {
//             Debug.Log("Is being held");
//             isBeingHeld = true;
//             transform.LookAt(other.transform.position, transform.up);
//         }
//     }

//     private void OnTriggerExit(Collider other)
//     {
//         if (other.CompareTag("PlayerHand"))
//         {
//             isBeingHeld = false;
//         }
//     }

//     private float NormalizeAngle(float angle)
//     {
//         if (angle > 180f) angle -= 360f;
//         return angle;
//     }
// }

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class JoystickControl : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onGrab;
    public UnityEvent onRelease;

    [Header("Joystick Parts")]
    public Transform topOfJoystick;

    [Header("Claw Control")]
    public ClawScript clawController;
    public bool enableXMovement = true;
    public bool enableZMovement = true;

    [Header("Joystick Settings")]
    [SerializeField] private float deadZone = 5f;
    [SerializeField] private float tiltToMoveMultiplier = 0.02f;
    [SerializeField] private float maxTiltAngle = 30f;

    [Header("Auto-Centering")]
    [SerializeField] private float returnSpeed = 3f;

    private bool isBeingHeld = false;
    private bool wasHeld = false;
    private Quaternion initialHandleRotation;
    private string currentDirection = "";
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // If no handle assigned, use this transform
        if (topOfJoystick == null)
        {
            topOfJoystick = transform;
            Debug.LogWarning("topOfJoystick was null; defaulting to own transform.");
        }
    }

    void Update()
    {
        if (isBeingHeld)
        {
            Vector3 eul = topOfJoystick.localEulerAngles;
            float fb = eul.x > 180f ? eul.x - 360f : eul.x;
            float ss = eul.z > 180f ? eul.z - 360f : eul.z;

            float moveX = Mathf.Abs(ss) > deadZone ? ss * tiltToMoveMultiplier : 0f;
            float moveZ = Mathf.Abs(fb) > deadZone ? fb * tiltToMoveMultiplier : 0f;
            Vector3 moveDir = new Vector3(
                enableXMovement ? moveX : 0f,
                0f,
                enableZMovement ? moveZ : 0f
            );

            if (moveDir != Vector3.zero)
            {
                clawController.SetDirection(moveDir);
                string newDir = moveX > 0 ? "Right" : moveX < 0 ? "Left"
                                : moveZ > 0 ? "Forward" : moveZ < 0 ? "Backward" : "";
                if (newDir != currentDirection)
                {
                    currentDirection = newDir;
                    Debug.Log("Joystick direction: " + newDir);
                }
            }
            else if (currentDirection != "")
            {
                clawController.StopMovement();
                currentDirection = "";
                Debug.Log("Stopping - deadzone");
            }
        }
        else
        {
            if (currentDirection != "")
            {
                clawController.StopMovement();
                currentDirection = "";
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * returnSpeed);
        }
    }

                private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            if (!wasHeld)
            {
                onGrab?.Invoke();
                wasHeld = true;
            }
            isBeingHeld = true;

                        // Rotate the joystick body toward the hand
            transform.LookAt(other.transform.position, transform.up);

            // Clamp tilt on the joystick to maxTiltAngle
            Vector3 eul = transform.localEulerAngles;
            float x = eul.x > 180f ? eul.x - 360f : eul.x;
            float z = eul.z > 180f ? eul.z - 360f : eul.z;
            x = Mathf.Clamp(x, -maxTiltAngle, maxTiltAngle);
            z = Mathf.Clamp(z, -maxTiltAngle, maxTiltAngle);
            transform.localRotation = Quaternion.Euler(x, eul.y, z);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand") && wasHeld)
        {
            onRelease?.Invoke();
            wasHeld = false;
        }
        isBeingHeld = false;
    }

    /// <summary>
    /// Stub for haptic feedback so ClawScript can invoke it.
    /// </summary>
    /// <param name="intensity">Vibration intensity</param>
    /// <param name="duration">Duration in seconds</param>
    public void SendHapticFeedback(float intensity, float duration)
    {
        // Optional: implement haptics here
    }
}
