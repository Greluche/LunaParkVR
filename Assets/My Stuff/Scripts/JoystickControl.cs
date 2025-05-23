using UnityEngine;
using System;

public class JoystickControl : MonoBehaviour
{
    public Transform topOfJoystick;

    [SerializeField] private float forwardBackwardTilt = 0;
    [SerializeField] private float sideToSideTilt = 0;

    private Quaternion initialRotation;
    private bool isBeingHeld = false;

    [SerializeField] private float returnSpeed = 3f; // Speed at which the joystick returns to neutral

    void Start()
    {
        // Save the starting rotation
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Tilt Debug
        forwardBackwardTilt = topOfJoystick.rotation.eulerAngles.x;
        if (forwardBackwardTilt < 355 && forwardBackwardTilt > 290)
        {
            forwardBackwardTilt = Math.Abs(forwardBackwardTilt - 360);
            Debug.Log("Backward " + forwardBackwardTilt);
        }
        else if (forwardBackwardTilt > 5 && forwardBackwardTilt < 74)
        {
            Debug.Log("Forward " + forwardBackwardTilt);
        }

        sideToSideTilt = topOfJoystick.rotation.eulerAngles.z;
        if (sideToSideTilt < 355 && sideToSideTilt > 290)
        {
            sideToSideTilt = Math.Abs(sideToSideTilt - 360);
            Debug.Log("Right " + sideToSideTilt);
        }
        else if (sideToSideTilt > 5 && sideToSideTilt < 74)
        {
            Debug.Log("Left " + sideToSideTilt);
        }

        // Reset rotation if not held
        if (!isBeingHeld)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * returnSpeed);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
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
}