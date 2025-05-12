using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;

    public void Interact()
    {
        if (dialogue != null)
        {
            dialogue.StartDialogue();
        }
    }
}