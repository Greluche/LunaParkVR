using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Dialogue : MonoBehaviour
{
    [Header("XR Input")]
    public InputActionProperty nextLineButton;

    public string sceneToLoad;
    
    [Header("UI Elements")]
    public GameObject dialogueBox;
    public TextMeshProUGUI textComponent;
    public Button yesButton;
    public Button noButton;
    
    [Header("Dialogue Lines")]
    public string[] lines;

    private int index;

    [Header("Distance Cancel")]
    [SerializeField] private Transform playerHead;
    [SerializeField] private Transform npcTransform;
    [SerializeField] private float dialogueCancelDistance = 3f;
    void Start()
    {
        dialogueBox.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        // Optional: hook up button behavior here or via inspector
        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(OnNo);
        
        nextLineButton.action.Enable();
    }
    
    void Update()
    {
        if (dialogueBox.activeSelf && nextLineButton.action.WasPressedThisFrame())
        {
            NextLine();
        }

        if (dialogueBox.activeSelf && playerHead != null && npcTransform != null)
        {
            float distance = Vector3.Distance(playerHead.position, npcTransform.position);
            if (distance > dialogueCancelDistance)
            {
                Debug.Log("Player walked away — cancelling dialogue.");
                EndDialogue();
            }
        }
    }

    public void StartDialogue()
    {
        DialogueManager.IsDialogueActive = true;
        index = 0;
        dialogueBox.SetActive(true);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        ShowLine();
    }

    void ShowLine()
    {
        if (index < lines.Length)
        {
            textComponent.text = lines[index];
        }
        else
        {
            ShowChoiceButtons();
        }
    }

    public void NextLine()
    {
        index++;
        ShowLine();
    }

    void ShowChoiceButtons()
    {
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
    }

    void OnYes()
    {
        // TODO: You handle scene teleport here
        SceneManager.LoadScene(sceneToLoad);
        Debug.Log("Player chose YES");
        EndDialogue();
    }

    void OnNo()
    {
        Debug.Log("Player chose NO");
        EndDialogue();
    }

    void EndDialogue()
    {
        DialogueManager.IsDialogueActive = false;
        dialogueBox.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
    }
}
