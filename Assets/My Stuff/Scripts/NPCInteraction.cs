using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class NPCInteraction : MonoBehaviour
{
    public Transform player; // Drag the VR Camera or XR Rig here in the inspector
    public float interactionDistance = 2.0f; // Max distance to interact
    public string sceneToLoad;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        interactable.enabled = distance <= interactionDistance;
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEnter);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEnter);
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log("NPC has been interacted with!");
        SceneManager.LoadScene(sceneToLoad);
    }
}