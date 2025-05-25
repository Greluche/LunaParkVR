using UnityEngine;
using UnityEngine.Events;
using System;

public class Joystick : MonoBehaviour
{
    public Transform topOfJoystick;

    [Header("Events")]
    public UnityEvent onGrab;
    public UnityEvent onRelease;

    [Header("Claw Control")]
    public ClawScript clawController;

    [SerializeField] private float forwardBackwardTilt = 0;
    [SerializeField] private float sideToSideTilt = 0;

    private Quaternion initialRotation;
    private bool isBeingHeld = false;

    [SerializeField] private float returnSpeed = 3f; // Speed at which the joystick returns to neutral

    void Start()
    {
        // Save the initial rotation
        initialRotation = transform.rotation;
    }


// This method is inspired from a tutorial to compute the direction of the joystick and pass them to the claw
    void Update()
    {
        // Calculate Forward/Backward Tilt (Z movement)
        float forwardBackwardTilt = topOfJoystick.rotation.eulerAngles.x;
        float forwardBackwardValue = 0f; // This will be signed

        if (forwardBackwardTilt < 355 && forwardBackwardTilt > 290)
        {

            // Backwards: away from player (positive Z movement, so positive value)
            forwardBackwardValue = Mathf.Abs(forwardBackwardTilt - 360); // e.g., 350 becomes 10
        }
        else if (forwardBackwardTilt > 5 && forwardBackwardTilt < 74)
        {
            
            // Forwards: towards player (negative Z movement, so negative value)
            forwardBackwardValue = -forwardBackwardTilt;
        }
        
        // Calculate Side-to-Side Tilt (X movement)
        float sideToSideTilt = topOfJoystick.rotation.eulerAngles.z;
        float sideToSideValue = 0f; // This will be signed

        if (sideToSideTilt < 355 && sideToSideTilt > 290)
        {
            // Right: negative X movement (so negative value)
            sideToSideValue = -Mathf.Abs(sideToSideTilt - 360); // e.g., 350 becomes -10
        }
        else if (sideToSideTilt > 5 && sideToSideTilt < 74)
        {
            // Left: positive X movement (so positive value)
            sideToSideValue = sideToSideTilt;
        }
        
        // Reset rotation if not held
        if (!isBeingHeld)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * returnSpeed);
        }
        

        Vector3 moveDirection = GetDirectionFromTilt(forwardBackwardValue, sideToSideValue);
        clawController.SetDirection(moveDirection);
        

    }

// method to chekck if the hand controller is closed to the joystick to move it
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            onGrab?.Invoke();
            isBeingHeld = true;
            transform.LookAt(other.transform.position, transform.up);
        }
    }

// method to make sure the player releases the claw and it can go back to its initial position
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            onRelease?.Invoke();
            isBeingHeld = false;
        }
    }

// method to convert the directions from the joystick to a vector3 d to pass to the setDirection method of the claw
    private Vector3 GetDirectionFromTilt(float forwardBackwardTilt, float sideToSideTilt, float maxTilt = 70f)
    {
        // normalized
        float x = Mathf.Clamp(sideToSideTilt, -maxTilt, maxTilt) / maxTilt;  // Left/Right
        float z = Mathf.Clamp(forwardBackwardTilt, -maxTilt, maxTilt) / maxTilt; // Back/Forward

        return new Vector3(x, 0f, z);
    }



    public void SendHapticFeedback(float intensity, float duration)
    {
    }

}