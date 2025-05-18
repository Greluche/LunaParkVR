using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("References")] public InputActionProperty interactButton;
    [SerializeField] private Transform xrCameraTransform;

    void Start()
    {
        interactButton.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (DialogueManager.IsDialogueActive) return; // 🚫 Skip if dialogue is open

        if (interactButton.action.IsPressed())
        {
            float interactRange = 2f;
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent(out NPCInteraction npcInteractable))
                {
                    npcInteractable.Interact();
                }
            }
        }
    }

    public bool IsNearInteractable()
    {
        if (xrCameraTransform == null || xrCameraTransform.position == Vector3.zero)
        {
            // Avoid proximity check if headset hasn't updated its position yet
            return false;
        }

        float interactRange = 2f;
        Vector3 checkOrigin = xrCameraTransform.position;

        Collider[] colliderArray = Physics.OverlapSphere(checkOrigin, interactRange);

        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out NPCInteraction npcInteractable))
            {
                return true;
            }
        }

        return false;
    }
}