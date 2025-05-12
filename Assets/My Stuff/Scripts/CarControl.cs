using UnityEngine;
using UnityEngine.InputSystem;

public class CarControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxForwardSpeed = 5f;
    public float maxReverseSpeed = 3f;
    public float acceleration = 3f;
    public float deceleration = 4f;
    public float maxTurnSpeed = 90f;

    [Header("Arena Bounds")]
    public Transform arenaCenter;       // Center of the arena
    public float arenaWidth = 20f;      // Total width of square arena (X axis)
    public float arenaHeight = 20f;     // Total height of square arena (Z axis)
    public float dampingZoneWidth = 2f; // How close to wall damping starts
    
    [Header("Input Actions")]
    public InputActionProperty accelerateButton; // Forward button
    public InputActionProperty reverseButton;    // Reverse button

    [Header("References")]
    public SteeringWheel steeringWheel;

    private float currentSpeed = 0f;

    private void Start()
    {
        accelerateButton.action.Enable();
        reverseButton.action.Enable();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        bool isAccelerating = accelerateButton.action.IsPressed();
        bool isReversing = reverseButton.action.IsPressed();

        if (isAccelerating && !isReversing)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxForwardSpeed, acceleration * dt);
        }
        else if (isReversing && !isAccelerating)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, -maxReverseSpeed, acceleration * dt);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * dt);
        }

        // Move car
        float damping = GetDampingFactor(transform.position);
        float effectiveSpeed = currentSpeed * damping;
        transform.Translate(Vector3.forward * effectiveSpeed * dt, Space.Self);

        // Steering
        if (steeringWheel != null)
        {
            float steerInput = steeringWheel.WheelAngleNormalized();
            float turnAmount = steerInput * maxTurnSpeed * dt;
            transform.Rotate(Vector3.up, turnAmount);
        }
    }
    
    private float GetDampingFactor(Vector3 position)
    {
        Vector3 localPos = position - arenaCenter.position;

        float halfWidth = arenaWidth / 2f;
        float halfHeight = arenaHeight / 2f;

        // Distance from each edge
        float dx = halfWidth - Mathf.Abs(localPos.x);
        float dz = halfHeight - Mathf.Abs(localPos.z);

        // Compute per-axis damping factors
        float fx = dx > dampingZoneWidth ? 1f : Mathf.Clamp01(dx / dampingZoneWidth);
        float fz = dz > dampingZoneWidth ? 1f : Mathf.Clamp01(dz / dampingZoneWidth);

        // Take the minimum damping factor
        return Mathf.Min(fx, fz);
    }
}

