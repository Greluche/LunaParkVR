using UnityEngine;
using System;

public class JoystickControl : MonoBehaviour
{
    [Header("Joystick Parts")]
    public Transform topOfJoystick;

    [Header("Claw Control")]
    public ClawScript clawController; // Drag it in the Inspector
    public bool enableXMovement = true;
    public bool enableZMovement = true;

    [Header("Joystick Settings")]
    [SerializeField] private float forwardBackwardTilt = 0f;
    [SerializeField] private float sideToSideTilt = 0f;
    [SerializeField] private float deadZone = 5f;
    [SerializeField] private float tiltToMoveMultiplier = 0.02f;

    [Header("Auto-Centering")]
    private Quaternion initialRotation;
    private bool isBeingHeld = false;
    [SerializeField] private float returnSpeed = 3f;

    private string currentDirection = "";

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Compute local Euler angles
        Vector3 angles = topOfJoystick.localEulerAngles;
        forwardBackwardTilt = NormalizeAngle(angles.x);
        sideToSideTilt = NormalizeAngle(angles.z);

        // Convert tilt into movement direction
        float moveX = Mathf.Abs(sideToSideTilt) > deadZone ? -sideToSideTilt * tiltToMoveMultiplier : 0f;
        float moveZ = Mathf.Abs(forwardBackwardTilt) > deadZone ? forwardBackwardTilt * tiltToMoveMultiplier : 0f;

        Vector3 moveDir = new Vector3(
            enableXMovement ? moveX : 0f,
            0f,
            enableZMovement ? moveZ : 0f
        );

        // Send movement to claw controller
        if (moveDir != Vector3.zero)
        {
            clawController.SetDirection(moveDir);

            string newDir = moveDir.x > 0 ? "Right" :
                            moveDir.x < 0 ? "Left" :
                            moveDir.z > 0 ? "Forward" :
                            moveDir.z < 0 ? "Backward" : "";

            if (newDir != currentDirection)
            {
                Debug.Log("Joystick direction changed to: " + newDir);
                currentDirection = newDir;
            }
        }
        else if (currentDirection != "")
        {
            clawController.StopMovement();
            currentDirection = "";
            Debug.Log("Stopping movement - in deadzone");
        }

        // Reset rotation when not grabbed
        if (!isBeingHeld)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * returnSpeed);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay");
        if (other.CompareTag("PlayerHand"))
        {
            Debug.Log("Is being held");
            isBeingHeld = true;
            transform.LookAt(other.transform.position, transform.up);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            isBeingHeld = false;
        }
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}