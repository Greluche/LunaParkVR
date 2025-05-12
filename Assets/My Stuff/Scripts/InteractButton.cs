using UnityEngine;
using System.Collections;

public class InteractButton : MonoBehaviour
{
    [SerializeField] private GameObject containerGameObject;
    [SerializeField] private PlayerInteract playerInteract;
    
    void Start()
    {
        Debug.Log("InteractButton script Start() called");
    }
    private void Update()
    {
        
        if (DialogueManager.IsDialogueActive)
        {
            // Dialogue is ongoing → hide the interact prompt
            Hide();
            return;
        }

        if (playerInteract.IsNearInteractable())
        {
            Debug.Log("Player is near NPC — show UI");
            Show();
        }
        else
        {
            Hide();
        }
    }
    
    private void Show()
    {
        containerGameObject.SetActive(true);
    }

    private void Hide()
    {
        containerGameObject.SetActive(false);
    }
}
