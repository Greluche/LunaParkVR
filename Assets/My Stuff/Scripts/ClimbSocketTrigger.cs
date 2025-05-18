using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ClimbSocketTrigger : MonoBehaviour
{
    private XRSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks
        socket.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        var handle = args.interactableObject.transform.GetComponent<HandleToClimbable>();
        if (handle != null)
        {
            handle.ActivateClimbMode();
        }
    }
}