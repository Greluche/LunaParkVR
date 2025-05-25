using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BlockingDialogue : MonoBehaviour
{
    [Header("XR Input")]
    public InputActionProperty interactButton; // A button
    public InputActionProperty dismissButton;  // Trigger or another dismiss

    [Header("UI Elements")]
    public GameObject dialogueBox;
    public TextMeshProUGUI textComponent;
    
    [Header("Message")]
    [TextArea]
    public string message = "Complete all the games before entering the haunted house.";

    [Header("Distance Cancel & Interaction")]
    [SerializeField] private Transform playerHead;
    [SerializeField] private Transform npcTransform;
    [SerializeField] private float dialogueCancelDistance = 3f;
    [SerializeField] private float interactRange = 2f;

    [Header("NPC Voice Reactions")]
    [SerializeField] private AudioClip[] voiceClips;
    [SerializeField] private AudioSource voiceSource;

    private bool dialogueActive = false;

    private void Start()
    {
        dialogueBox.SetActive(false);

        interactButton.action.Enable();
        dismissButton.action.Enable();
    }

    private void Update()
    {
        if (!dialogueActive)
        {
            if (interactButton.action.WasPressedThisFrame() && PlayerIsNear())
            {
                StartDialogue();
            }
            return;
        }

        if (dismissButton.action.WasPressedThisFrame())
        {
            CloseDialogue();
        }

        if (!PlayerIsNear())
        {
            Debug.Log("Player walked away — closing dialogue.");
            CloseDialogue();
        }
    }

    private bool PlayerIsNear()
    {
        if (playerHead == null || npcTransform == null) return false;

        return Vector3.Distance(playerHead.position, npcTransform.position) <= dialogueCancelDistance;
    }

    public void StartDialogue()
    {
        dialogueActive = true;
        dialogueBox.SetActive(true);
        textComponent.text = message;
        PlayVoiceReaction();
    }

    private void CloseDialogue()
    {
        dialogueActive = false;
        dialogueBox.SetActive(false);
    }

    private void PlayVoiceReaction()
    {
        if (voiceSource == null || voiceClips.Length == 0) return;

        AudioClip clip = voiceClips[Random.Range(0, voiceClips.Length)];
        voiceSource.PlayOneShot(clip);
    }
}