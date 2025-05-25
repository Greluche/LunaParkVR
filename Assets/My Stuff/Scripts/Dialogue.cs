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
    public Button repeatTutorialButton;

    [Header("Dialogue Lines")]
    public string[] lines;

    private int index;

    [Header("Distance Cancel")]
    [SerializeField] private Transform playerHead;
    [SerializeField] private Transform npcTransform;
    [SerializeField] private float dialogueCancelDistance = 3f;

    [Header("NPC Voice Reactions")]
    [SerializeField] private AudioClip[] voiceClips;
    [SerializeField] private AudioSource voiceSource;

    [Header("Tutorial Scene Settings")]
    [SerializeField] private bool useTutorialScene = false;
    [SerializeField] private string tutorialSceneName;
    [SerializeField] private string tutorialPlayerPrefKey;
    
    void Start()
    {
        dialogueBox.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        repeatTutorialButton.gameObject.SetActive(false);
        repeatTutorialButton.onClick.AddListener(OnRepeatTutorial);

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
        repeatTutorialButton.gameObject.SetActive(false);
        ShowLine();
        PlayVoiceReaction();
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

        if (useTutorialScene && PlayerPrefs.GetInt(tutorialPlayerPrefKey, 0) == 1)
        {
            repeatTutorialButton.gameObject.SetActive(true);
        }
        else
        {
            repeatTutorialButton.gameObject.SetActive(false);
        }
    }

    void OnYes()
    {
        Debug.Log("Player chose YES");

        if (useTutorialScene)
        {
            // Check if tutorial has already been completed
            if (PlayerPrefs.GetInt(tutorialPlayerPrefKey, 0) == 0)
            {
                Debug.Log("Loading tutorial scene: " + tutorialSceneName);
                SceneManager.LoadScene(tutorialSceneName);
                return;
            }
        }

        Debug.Log("Loading main game scene: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
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
        repeatTutorialButton.gameObject.SetActive(false);
    }
    
    void PlayVoiceReaction()
    {
        if (voiceSource == null || voiceClips.Length == 0) return;

        AudioClip clip = voiceClips[Random.Range(0, voiceClips.Length)];
        voiceSource.PlayOneShot(clip);
    }
    
    void OnRepeatTutorial()
    {
        Debug.Log("Player chose to repeat the tutorial");

        PlayerPrefs.SetInt(tutorialPlayerPrefKey, 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(tutorialSceneName);
    }
}