using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class SteeringWheel : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
    [SerializeField] private Transform wheelTransform;

    [Header("Wheel Rotation Limits")]
    [SerializeField] private float minAngle = -180f;
    [SerializeField] private float maxAngle = 180f;

    [Header("Auto-Center Settings")]
    [SerializeField] private bool autoCenter = true;
    [SerializeField] private float autoCenterSpeed = 90f; // degrees per second

    public UnityEvent<float> OnWheelRotated;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor controllingInteractor = null;
    private float currentInteractorAngle = 0.0f;
    private float wheelAngle = 0.0f;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (controllingInteractor == null)
        {
            controllingInteractor = args.interactorObject;
            currentInteractorAngle = FindWheelAngle(controllingInteractor);
        }
        else
        {
            // If we already have a controller, prefer the one closest to "up" (top of wheel)
            controllingInteractor = FindPrimaryInteractor();
            currentInteractorAngle = FindWheelAngle(controllingInteractor);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (args.interactorObject == controllingInteractor)
        {
            controllingInteractor = null;

            if (interactorsSelecting.Count > 0)
            {
                controllingInteractor = FindPrimaryInteractor();
                currentInteractorAngle = FindWheelAngle(controllingInteractor);
            }
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (controllingInteractor != null)
            {
                RotateWheel();
            }
            else if (autoCenter && Mathf.Abs(wheelAngle) > 0.01f)
            {
                AutoCenterWheel();
            }
        }
    }

    private void RotateWheel()
    {
        float newInteractorAngle = FindWheelAngle(controllingInteractor);
        float angleDifference = Mathf.DeltaAngle(currentInteractorAngle, newInteractorAngle);

        float newWheelAngle = Mathf.Clamp(wheelAngle + angleDifference, minAngle, maxAngle);
        float allowedDifference = newWheelAngle - wheelAngle;

        wheelTransform.Rotate(transform.forward, allowedDifference, Space.World);

        wheelAngle = newWheelAngle;
        currentInteractorAngle = newInteractorAngle;

        OnWheelRotated?.Invoke(wheelAngle);
    }

    private void AutoCenterWheel()
    {
        float delta = autoCenterSpeed * Time.deltaTime;
        float newWheelAngle = Mathf.MoveTowards(wheelAngle, 0f, delta);

        float allowedDifference = newWheelAngle - wheelAngle;
        wheelTransform.Rotate(transform.forward, allowedDifference, Space.World);

        wheelAngle = newWheelAngle;

        OnWheelRotated?.Invoke(wheelAngle);
    }
    
    public float WheelAngleNormalized()
    {
        return Mathf.Clamp(wheelAngle / maxAngle, -1f, 1f);
    }
    
    private float FindWheelAngle(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor)
    {
        Vector2 localPos = FindLocalPoint(interactor.transform.position);
        return ConvertToAngle(localPos);
    }

    private Vector2 FindLocalPoint(Vector3 position)
    {
        return transform.InverseTransformPoint(position).normalized;
    }

    private float ConvertToAngle(Vector2 direction)
    {
        return Vector2.SignedAngle(Vector2.up, direction);
    }

    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor FindPrimaryInteractor()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor bestInteractor = null;
        float bestDot = -Mathf.Infinity;

        foreach (var interactor in interactorsSelecting)
        {
            Vector3 localPos = transform.InverseTransformPoint(interactor.transform.position).normalized;
            float dot = Vector3.Dot(localPos, Vector3.up); // how close to "top"

            if (dot > bestDot)
            {
                bestDot = dot;
                bestInteractor = interactor;
            }
        }

        return bestInteractor;
    }
}

