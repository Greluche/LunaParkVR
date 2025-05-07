using UnityEngine;
using UnityEngine.InputSystem;

public class CarControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 5f;
    public float maxBackspeed = 3f;
    public float acceleration = 3f;
    public float deceleration = 4f;
    public float maxTurnSpeed = 90f;

    [Header("References")]
    public SteeringWheel steeringWheel;
    public InputActionProperty accelerateButton; // Input action for the button
    public InputActionProperty retromarchButton;
        
    private float currentSpeed = 0f;
    
    private void Start()
    {
        accelerateButton.action.Enable(); // <- critical
        retromarchButton.action.Enable();
    }
    
    private void Update()
    {
        float dt = Time.deltaTime;

        // --- Read input from button ---
        bool isAccelerating = accelerateButton.action.IsPressed();
        
        bool isRetromarching = retromarchButton.action.IsPressed();
        
        // --- Acceleration logic ---
        if (isAccelerating)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * dt);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * dt);
        }

        // --- Move forward based on current speed ---
        transform.Translate(Vector3.forward * currentSpeed * dt);

        // --- Steering ---
        if (steeringWheel != null)
        {
            float steerInput = steeringWheel.WheelAngleNormalized(); // -1 to 1
            float turnAmount = steerInput * maxTurnSpeed * dt;
            transform.Rotate(Vector3.up, turnAmount);
        }
    }
}