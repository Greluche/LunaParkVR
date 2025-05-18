using UnityEngine;


public class HandleToClimbable : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbInteractable climbInteractable;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private void Awake()
    {
        climbInteractable.enabled = false;
    }

    public void ActivateClimbMode()
    {
        // Stop player from grabbing it again
        grabInteractable.enabled = false;

        // Optional: freeze in place
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Activate climbability
        climbInteractable.enabled = true;
    }
}
