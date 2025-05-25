using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("References")] public InputActionProperty interactButton;
    [SerializeField] private Transform xrCameraTransform;

    void Awake()
    {
        Debug.Log("PlayerInteract.Awake() on: " + gameObject.name);
    }

    void Start()
    {
        Debug.Log("PlayerInteract.Start() on: " + gameObject.name);
        interactButton.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (DialogueManager.IsDialogueActive) return;

        if (xrCameraTransform == null || xrCameraTransform.position == Vector3.zero) return;

        if (interactButton.action.IsPressed())
        {
            float interactRange = 2f;
            Vector3 checkOrigin = xrCameraTransform.position; // HEAD instead of rig base
            Collider[] colliderArray = Physics.OverlapSphere(checkOrigin, interactRange);

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
        if (xrCameraTransform == null)
        {
            Debug.Log("[Interact] XR camera is null");
            return false;
        }

        if (xrCameraTransform.position == Vector3.zero)
        {
            Debug.Log("[Interact] XR camera position is zero");
            return false;
        }

        float interactRange = 2f;
        Vector3 checkOrigin = xrCameraTransform.position;

        Collider[] colliderArray = Physics.OverlapSphere(checkOrigin, interactRange);
        // Debug.Log($"[Interact] Found {colliderArray.Length} colliders near XR camera");

        foreach (Collider collider in colliderArray)
        {
            // Debug.Log($"[Interact] Collider found: {collider.name}");
            if (collider.TryGetComponent(out NPCInteraction npcInteractable))
            {
                Debug.Log($"[Interact] NPCInteraction found on: {collider.name}");
                return true;
            }
        }

        return false;
    }
}