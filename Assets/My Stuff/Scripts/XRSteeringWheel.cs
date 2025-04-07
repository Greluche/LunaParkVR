using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
public class XRSteeringWheel : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] Transform steeringWheelVisual;

    [Header("Rotation Settings")]
    [SerializeField] float maxRotationAngle = 450f;
    [SerializeField] float minRotationAngle = -450f;
    [SerializeField] float sensitivity = 1.5f;

    [Header("Events")]
    public UnityEvent<float> onSteeringValueChanged;

    UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor;
    float currentRotation = 0f;
    float baseAngle = 0f;
    float currentSteeringValue = 0f;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject;
        baseAngle = GetInteractorAngle();
    }

    void OnRelease(SelectExitEventArgs args)
    {
        interactor = null;
    }

    void Update()
    {
        if (interactor != null)
        {
            float currentAngle = GetInteractorAngle();
            float delta = Mathf.DeltaAngle(baseAngle, currentAngle);

            currentRotation += delta * sensitivity;
            currentRotation = Mathf.Clamp(currentRotation, minRotationAngle, maxRotationAngle);

            baseAngle = currentAngle;

            ApplyRotation();
            EmitSteeringValue();
        }
    }

    float GetInteractorAngle()
    {
        var interactorTransform = interactor.GetAttachTransform(grabInteractable);
        Vector3 localDir = transform.InverseTransformPoint(interactorTransform.position) - Vector3.zero;
        float angle = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
        return angle;
    }

    void ApplyRotation()
    {
        if (steeringWheelVisual != null)
        {
            steeringWheelVisual.localRotation = Quaternion.Euler(0f, 0f, -currentRotation);
        }
    }

    void EmitSteeringValue()
    {
        // Normalize between -1 and 1
        float normalized = Mathf.InverseLerp(minRotationAngle, maxRotationAngle, currentRotation) * 2f - 1f;
        if (Mathf.Abs(normalized - currentSteeringValue) > 0.001f)
        {
            currentSteeringValue = normalized;
            onSteeringValueChanged?.Invoke(currentSteeringValue);
        }
    }
}