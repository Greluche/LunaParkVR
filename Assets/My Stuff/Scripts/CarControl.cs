using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

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
    
    [Header("Haptics")]
    public HapticImpulsePlayer leftHaptics;
    public HapticImpulsePlayer rightHaptics;
    
    [Header("Engine Audio")]
    public AudioSource engineAudioSource;
    public AudioClip engineLoopClip;
    public float minPitch = 0.9f;
    public float maxPitch = 1.3f;
    public float minVolume = 0.05f;
    public float maxVolume = 0.6f;
    
    [Header("References")]
    public SteeringWheel steeringWheel;
    
    [Header("Visual Effects")]
    public ParticleSystem speedLineVFX;
    public float maxSpeedForVFX = 5f;
    public float maxEmissionRate = 100f;
    
    private float currentSpeed = 0f;

    private void Start()
    {
        accelerateButton.action.Enable();
        reverseButton.action.Enable();
        speedLineVFX.gameObject.SetActive(true);
        if (engineAudioSource != null && engineLoopClip != null)
        {
            engineAudioSource.clip = engineLoopClip;
            engineAudioSource.loop = true;
            engineAudioSource.playOnAwake = false;
            engineAudioSource.spatialBlend = 1f; // fully 3D
            engineAudioSource.Play();
        }
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
        
        if (engineAudioSource != null)
        {
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxForwardSpeed);

            engineAudioSource.volume = Mathf.Lerp(minVolume, maxVolume, normalizedSpeed);
            engineAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, normalizedSpeed);
        }
        if (Mathf.Abs(currentSpeed) < 0.05f)
        {
            if (speedLineVFX.isPlaying)
                speedLineVFX.Stop();
        }
        else
        {
            if (!speedLineVFX.isPlaying)
                speedLineVFX.Play();

            float normalizedSpeed = Mathf.InverseLerp(0f, maxSpeedForVFX, Mathf.Abs(currentSpeed));
            var emission = speedLineVFX.emission;
            emission.rateOverTime = Mathf.Lerp(0f, maxEmissionRate, normalizedSpeed);
        }
        // HAPTIC FEEDBACK BASED ON SPEED & STEERING
        float hapticSteerInput = steeringWheel != null ? steeringWheel.WheelAngleNormalized() : 0f;
        float baseIntensity = Mathf.Abs(currentSpeed) / maxForwardSpeed;

        float leftIntensity  = baseIntensity * Mathf.Clamp01(-hapticSteerInput);
        float rightIntensity = baseIntensity * Mathf.Clamp01(hapticSteerInput);

// Forward-driving feedback (both hands)
        float forwardIntensity = baseIntensity * (1f - Mathf.Abs(hapticSteerInput));
        leftIntensity += forwardIntensity * 0.5f;
        rightIntensity += forwardIntensity * 0.5f;
        if (baseIntensity > 0.1f)
        {
            if (leftHaptics != null)
                leftHaptics.SendHapticImpulse(leftIntensity, 0.05f);

            if (rightHaptics != null)
                rightHaptics.SendHapticImpulse(rightIntensity, 0.05f);
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
    
    private void SendHaptic(XRBaseController controller, float amplitude, float duration = 0.05f)
    {
        if (controller != null)
        {
            controller.SendHapticImpulse(amplitude, duration);
        }
    }
}

