// using UnityEngine;
// using TMPro;
// using System.Collections;
// using UnityEngine.Events; // Add this for UnityEvents
// using UnityEngine.XR.Interaction.Toolkit; // Add this for SelectEnterEventArgs
// using UnityEngine.UI; // Add this for Button

// public class ClawGameUI : MonoBehaviour
// {
//     [Header("UI References")]
//     public GameObject instructionsCanvas;  // Reference to the entire Canvas
//     public TextMeshProUGUI instructionsText; // Reference to the TextMeshProUGUI on the Canvas
//     public TextMeshPro winText;
//     public TextMeshPro attemptsText;
//     public TextMeshPro loseText;

//     [Header("Play Again UI")]
//     [Tooltip("Canvas containing the play again prompt")]
//     public GameObject playAgainCanvas;

//     [Tooltip("Text component for the play again question")]
//     public TextMeshProUGUI playAgainText;

//     [Tooltip("Button to click if player wants to play again")]
//     public Button yesButton;

//     [Tooltip("Button to click if player doesn't want to play again")]
//     public Button noButton;

//     [Header("Game Settings")]
//     public GameObject[] goldenTeddies; // Reference your 2 golden teddies
//     public int maxAttempts = 5;
//     [Tooltip("Number of attempts that triggers the warning sound")]
//     public int warningAttemptThreshold = 3;

//     [Header("Toy Reset")]
//     [Tooltip("All toys with ToyController component will be reset when game ends")]
//     public bool resetToysOnGameOver = true;

//     [Tooltip("Delay before resetting toys when game ends (seconds)")]
//     [Range(0f, 5f)]
//     public float toyResetDelay = 2f;

//     [Tooltip("Container that holds all the toys (optional)")]
//     public Transform toyContainer;

//     [Header("UI Display Settings")]
//     [Tooltip("How long to display the win message before fading out (seconds)")]
//     [Range(1f, 10f)]
//     public float winMessageDisplayTime = 10f;

//     [Tooltip("How long to display the game over message before fading out (seconds)")]
//     [Range(1f, 10f)]
//     public float gameOverDisplayTime = 8f;

//     [Tooltip("Font asset to use for the win message")]
//     public TMP_FontAsset winMessageFont;

//     [Header("References")]
//     public ClawScript clawController;
//     public JoystickControl joystickController;

//     [Header("Audio")]
//     [Tooltip("Sound played when player reaches the warning attempt threshold")]
//     public AudioClip warningSound;
//     [Tooltip("Sound played when player wins")]
//     public AudioClip winSound;
//     [Tooltip("Sound played when player loses")]
//     public AudioClip loseSound;
//     [Tooltip("Sound played when showing the play again prompt")]
//     public AudioClip promptSound;

//     private int attemptCount = 0;
//     private bool gameWon = false;
//     private bool initialized = false;
//     private bool hasShownAttempts = false;
//     private AudioSource audioSource;

//     // Add debounce variables to prevent double counting
//     private bool isProcessingAttempt = false;
//     private float attemptDebounceTime = 1.0f; // Minimum time between counting attempts
//     private float lastAttemptTime = 0f;

//     void Start()
//     {
//         // Setup UI
//         if (winText != null)
//             winText.gameObject.SetActive(false);

//         if (loseText != null)
//             loseText.gameObject.SetActive(false);

//         if (attemptsText != null)
//         {
//             attemptsText.text = $"Attempts: 0/{maxAttempts}";
//             // Hide attempts counter initially
//             attemptsText.gameObject.SetActive(false);
//         }

//         // Apply custom font to win message if specified
//         if (winText != null && winMessageFont != null)
//         {
//             winText.font = winMessageFont;
//         }

//         // Hide play again UI initially
//         if (playAgainCanvas != null)
//         {
//             playAgainCanvas.SetActive(false);
//         }

//         // Set up button listeners
//         SetupPlayAgainButtons();

//         // Show instructions
//         ShowInstructions();

//         // Initialize connection to claw
//         InitializeClawConnection();

//         // Initialize joystick connection
//         InitializeJoystickConnection();

//         // Setup audio source
//         SetupAudioSource();
//     }

//     void SetupPlayAgainButtons()
//     {
//         if (yesButton != null)
//             yesButton.onClick.AddListener(OnPlayAgainYesClicked);
//         if (noButton != null)
//             noButton.onClick.AddListener(OnPlayAgainNoClicked);
//     }

//     // Method so the player can play again 
//     void OnPlayAgainYesClicked()
//     {
//         // Hide the play again canvas
//         if (playAgainCanvas != null)
//             playAgainCanvas.SetActive(false);

//         // Reset toys
//         if (resetToysOnGameOver)
//             StartCoroutine(ResetAllToysAfterDelay(0.5f));

//         // Reset game state
//         ResetGameState();
//     }

//     // Method if the player doesn't want to play again, the toys he won will stay on the table

//     void OnPlayAgainNoClicked()
//     {
//         // Hide the play again canvas
//         if (playAgainCanvas != null)
//             playAgainCanvas.SetActive(false);

//         // Hide all UI elements
//         if (attemptsText != null)
//             attemptsText.gameObject.SetActive(false);
//         if (instructionsCanvas != null)
//             instructionsCanvas.SetActive(false);
//     }

//     void SetupAudioSource()
//     {
//         audioSource = GetComponent<AudioSource>();
//         if (audioSource == null && (warningSound != null || winSound != null || loseSound != null))
//         {
//             audioSource = gameObject.AddComponent<AudioSource>();
//             audioSource.playOnAwake = false;
//         }
//     }

//     void InitializeClawConnection()
//     {
//         if (clawController == null)
//         {
//             // Try to find it automatically
//             clawController = FindObjectOfType<ClawScript>();
//             if (clawController == null) return;
//         }

//         // Subscribe to event
//         SubscribeToClawEvents();
//         initialized = true;
//     }

//     void InitializeJoystickConnection()
//     {
//         if (joystickController == null)
//         {
//             // Try to find it automatically
//             joystickController = FindObjectOfType<JoystickControl>();
//             if (joystickController == null) return;
//         }

//         // Subscribe to joystick events
//         joystickController.selectEntered.AddListener(OnJoystickGrabbed);
//     }

//     // Method matching the XRGrabInteractable.selectEntered event for the joystick
//     void OnJoystickGrabbed(SelectEnterEventArgs args)
//     {
//         // Show attempts counter when joystick is first grabbed
//         if (!hasShownAttempts && attemptsText != null)
//         {
//             attemptsText.gameObject.SetActive(true);
//             hasShownAttempts = true;

//             // Update the attempts text to show 0 attempts
//             attemptsText.text = $"Attempts: 0/{maxAttempts}";

//             // Fade out instructions when showing attempts
//             if (instructionsCanvas != null && instructionsCanvas.activeSelf)
//                 instructionsCanvas.SetActive(false);
//         }
//     }

//     void SubscribeToClawEvents()
//     {
//         UnsubscribeFromClawEvents(); // Before subscribing we make sure nothing is here before, clearing step
//         if (clawController != null)
//             clawController.OnDropCompleted.AddListener(CheckForGoldenTeddy);
//     }

//     void UnsubscribeFromClawEvents()
//     {
//         if (clawController != null)
//             clawController.OnDropCompleted.RemoveAllListeners();
//         if (joystickController != null)
//             joystickController.selectEntered.RemoveListener(OnJoystickGrabbed);
//     }

//     void OnEnable()
//     {
//         if (initialized)
//             SubscribeToClawEvents();
//     }

//     void OnDisable()
//     {
//         UnsubscribeFromClawEvents();
//     }

//     void ShowInstructions()
//     {
//         if (instructionsCanvas != null && instructionsText != null)
//         {
//             instructionsText.text = "The claw game is simple. There are 2 golden teddys.\nIf you manage to get one in less than 5 tries, you win this game!";
//             instructionsCanvas.SetActive(true);
//         }
//     }

//     // We check if the player has grabbed a golden teddy to display the win or lose message
//     public void CheckForGoldenTeddy(GameObject grabbedToy)
//     {
//         // Skip if game is won
//         if (gameWon) return;

//         // Debounce check
//         if (isProcessingAttempt || Time.time - lastAttemptTime < attemptDebounceTime) return;

//         isProcessingAttempt = true;
//         lastAttemptTime = Time.time;

//         if (hasShownAttempts)
//         {
//             attemptCount++;
//             UpdateAttemptCounter();
//             if (attemptCount == warningAttemptThreshold) PlayWarningSound();
//         }

//         if (grabbedToy != null)
//         {
//             foreach (var goldenTeddy in goldenTeddies)
//             {
//                 if (goldenTeddy != null && (grabbedToy == goldenTeddy || grabbedToy.name == goldenTeddy.name))
//                 {
//                     ShowWinMessage();
//                     gameWon = true;
//                     return;
//                 }
//             }
//         }

//         if (attemptCount >= maxAttempts && !gameWon) // If the player has exceeded the max number of attempts its game over
//             ShowGameOverMessage();

//         StartCoroutine(ResetProcessingFlag());
//     }

//     private IEnumerator ResetProcessingFlag()
//     {
//         yield return new WaitForSeconds(attemptDebounceTime);
//         isProcessingAttempt = false;
//     }

//     // Increment the number of attemtps to match the claw drop
//     void UpdateAttemptCounter()
//     {
//         if (attemptsText != null)
//         {
//             attemptsText.text = $"Attempts: {attemptCount}/{maxAttempts}";
//             if (!attemptsText.gameObject.activeSelf)
//                 attemptsText.gameObject.SetActive(true);
//             StartCoroutine(PulseText(attemptsText, 1f, 1.2f, 0.5f));
//         }
//     }

//     // Play the warning/time sound when the player reaches 3/5 attempts we can decide at which attempt the sound will play in the inspector
//     void PlayWarningSound()
//     {
//         if (audioSource != null && warningSound != null)
//         {
//             audioSource.PlayOneShot(warningSound);
//             if (attemptsText != null)
//                 StartCoroutine(FlashText(attemptsText, Color.red, 3));
//         }
//     }

//     // If the player manage to get the golden teddy within the max attempts we display the winmessage + play the win sound
//     void ShowWinMessage()
//     {
//         if (winText != null)
//         {
//             StopAllCoroutines();
//             winText.transform.localScale = Vector3.one;
//             var c = winText.color;
//             winText.color = new Color(c.r, c.g, c.b, 1f);
//             winText.gameObject.SetActive(true);
//             StartCoroutine(SinglePulseText(winText, 0.8f, 1.5f, 0.7f));
//             StartCoroutine(FadeOutTextAfterDelay(winText, winMessageDisplayTime));
//         }
//         if (audioSource != null && winSound != null)
//             audioSource.PlayOneShot(winSound);
//         if (attemptsText != null)
//             StartCoroutine(FadeOutTextAfterDelay(attemptsText, 3f));
//         StartCoroutine(ShowPlayAgainPromptAfterDelay(winMessageDisplayTime + 1f));
//     }

//     // Show game over message if the player didn't get the golden teddy within the max attempts + play lose sound
//     void ShowGameOverMessage()
//     {
//         if (loseText != null)
//         {
//             loseText.gameObject.SetActive(true);
//             loseText.text = "Game Over! You've used all attempts.";
//             loseText.color = Color.red;
//             StartCoroutine(SinglePulseText(loseText, 0.9f, 1.3f, 0.6f));
//             StartCoroutine(FadeOutTextAfterDelay(loseText, gameOverDisplayTime));
//         }
//         if (audioSource != null && loseSound != null)
//             audioSource.PlayOneShot(loseSound);
//         StartCoroutine(ShowPlayAgainPromptAfterDelay(gameOverDisplayTime + 1f));
//     }

//     // Ask the player if he wants to play again
//     IEnumerator ShowPlayAgainPromptAfterDelay(float delay)
//     {
//         yield return new WaitForSeconds(delay);
//         if (playAgainCanvas != null)
//         {
//             playAgainCanvas.SetActive(true);
//             if (playAgainText != null)
//                 playAgainText.text = gameWon ? "Congratulations! Play again?" : "Try again?";
//             if (audioSource != null && promptSound != null)
//                 audioSource.PlayOneShot(promptSound);
//         }
//     }

//     // Rset the game state to allow the player to play again
//     void ResetGameState()
//     {
//         attemptCount = 0;
//         gameWon = false;
//         hasShownAttempts = true;
//         if (attemptsText != null)
//         {
//             attemptsText.text = $"Attempts: 0/{maxAttempts}";
//             attemptsText.color = Color.white;
//             attemptsText.gameObject.SetActive(true);
//             StartCoroutine(SinglePulseText(attemptsText, 0.9f, 1.2f, 0.5f));
//         }
//         if (winText != null) winText.gameObject.SetActive(false);
//         if (loseText != null) loseText.gameObject.SetActive(false);
//         if (instructionsCanvas != null) instructionsCanvas.SetActive(false);
//         SubscribeToClawEvents();
//     }

//     // Calls toycontroller script so the toys go back to inside the machine if the player wants to play again
//     private IEnumerator ResetAllToysAfterDelay(float delay)
//     {
//         yield return new WaitForSeconds(delay);
//         var toys = toyContainer ? toyContainer.GetComponentsInChildren<ToyController>(true) : FindObjectsOfType<ToyController>();
//         foreach (var toy in toys)
//         {
//             toy.ResetToInitialPosition();
//             yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
//         }
//     }

//     // Method to make the text fade after a delay
//     IEnumerator FadeOutTextAfterDelay(TextMeshPro text, float delay)
//     {
//         if (text == null) yield break;
//         yield return new WaitForSeconds(delay);
//         var start = text.color;
//         float elapsed = 0f, duration = 2f;
//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             text.color = new Color(start.r, start.g, start.b, Mathf.Lerp(start.a, 0f, elapsed / duration));
//             yield return null;
//         }
//         text.gameObject.SetActive(false);
//     }

//     // Method to make the text pulse once

//     IEnumerator SinglePulseText(TMP_Text text, float min, float max, float dur)
//     {
//         if (text == null) yield break;
//         var orig = text.transform.localScale;
//         float t = 0f;
//         while (t < dur)
//         {
//             t += Time.deltaTime;
//             float s = Mathf.Lerp(min, max, t / dur);
//             text.transform.localScale = orig * s;
//             yield return null;
//         }
//         yield return new WaitForSeconds(0.2f);
//         t = 0f;
//         while (t < dur)
//         {
//             t += Time.deltaTime;
//             float s = Mathf.Lerp(max, min, t / dur);
//             text.transform.localScale = orig * s;
//             yield return null;
//         }
//         text.transform.localScale = orig;
//     }

//     // Method to make the text pulse 

//     IEnumerator PulseText(TMP_Text text, float min, float max, float dur)
//     {
//         if (text == null) yield break;
//         var orig = text.transform.localScale;
//         while (text.gameObject.activeSelf)
//         {
//             float t = 0f;
//             while (t < dur)
//             {
//                 t += Time.deltaTime;
//                 float s = Mathf.Lerp(min, max, t / dur);
//                 text.transform.localScale = orig * s;
//                 yield return null;
//             }
//             t = 0f;
//             while (t < dur)
//             {
//                 t += Time.deltaTime;
//                 float s = Mathf.Lerp(max, min, t / dur);
//                 text.transform.localScale = orig * s;
//                 yield return null;
//             }
//         }
//     }

//     // Method to flash the attempt text when the player doesnt have many left
//     IEnumerator FlashText(TMP_Text text, Color col, int count)
//     {
//         if (text == null) yield break;
//         var orig = text.color;
//         for (int i = 0; i < count; i++)
//         {
//             text.color = col;
//             yield return new WaitForSeconds(0.2f);
//             text.color = orig;
//             yield return new WaitForSeconds(0.2f);
//         }
//     }

// }


using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class ClawGameUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionsCanvas;
    public TextMeshProUGUI instructionsText;
    public TextMeshPro winText;
    public TextMeshPro attemptsText;
    public TextMeshPro loseText;

    [Header("Play Again UI")]
    public GameObject playAgainCanvas;
    public TextMeshProUGUI playAgainText;
    public Button yesButton;
    public Button noButton;

    [Header("Game Settings")]
    public GameObject[] goldenTeddies;
    public int maxAttempts = 5;
    public int warningAttemptThreshold = 3;

    [Header("Toy Reset")]
    public bool resetToysOnGameOver = true;
    [Range(0f, 5f)] public float toyResetDelay = 2f;
    public Transform toyContainer;

    [Header("UI Display Settings")]
    [Range(1f, 10f)] public float winMessageDisplayTime = 10f;
    [Range(1f, 10f)] public float gameOverDisplayTime = 8f;
    public TMP_FontAsset winMessageFont;

    [Header("References")]
    public ClawScript clawController;
    public JoystickControl joystickController;

    [Header("Audio")]
    public AudioClip warningSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip promptSound;

    private AudioSource audioSource;
    private int attemptCount = 0;
    private bool gameWon = false;
    private bool initialized = false;
    private bool hasShownAttempts = false;
    private bool isProcessingAttempt = false;
    private float attemptDebounceTime = 1f;
    private float lastAttemptTime = 0f;

    void Start()
    {
        // UI initial state
        winText?.gameObject.SetActive(false);
        loseText?.gameObject.SetActive(false);
        if (attemptsText != null)
        {
            attemptsText.text = $"Attempts: 0/{maxAttempts}";
            attemptsText.gameObject.SetActive(false);
        }
        if (playAgainCanvas != null)
            playAgainCanvas.SetActive(false);
        if (winText != null && winMessageFont != null)
            winText.font = winMessageFont;

        SetupPlayAgainButtons();
        ShowInstructions();
        SetupAudioSource();
        InitializeClawConnection();
        InitializeJoystickConnection();
    }

    void SetupPlayAgainButtons()
    {
        yesButton?.onClick.AddListener(OnPlayAgainYesClicked);
        noButton?.onClick.AddListener(OnPlayAgainNoClicked);
    }

    void OnPlayAgainYesClicked()
    {
        playAgainCanvas?.SetActive(false);
        if (resetToysOnGameOver)
            StartCoroutine(ResetAllToysAfterDelay(0.5f));
        ResetGameState();
    }

    void OnPlayAgainNoClicked()
    {
        playAgainCanvas?.SetActive(false);
        attemptsText?.gameObject.SetActive(false);
        instructionsCanvas?.SetActive(false);
    }

    void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void InitializeClawConnection()
    {
        if (clawController == null)
            clawController = FindObjectOfType<ClawScript>();
        if (clawController == null) return;
        SubscribeToClawEvents();
        initialized = true;
    }

    void InitializeJoystickConnection()
    {
        if (joystickController == null)
            joystickController = FindObjectOfType<JoystickControl>();
        if (joystickController == null) return;

        // Subscribe to custom events
        joystickController.onGrab.AddListener(OnJoystickGrabbed);
        joystickController.onRelease.AddListener(OnJoystickReleased);
    }

    void SubscribeToClawEvents()
    {
        clawController.OnDropCompleted.RemoveAllListeners();
        clawController.OnDropCompleted.AddListener(CheckForGoldenTeddy);
    }

    void OnEnable()
    {
        if (initialized)
            SubscribeToClawEvents();
    }

    void OnDisable()
    {
        clawController?.OnDropCompleted.RemoveAllListeners();
        if (joystickController != null)
        {
            joystickController.onGrab.RemoveListener(OnJoystickGrabbed);
            joystickController.onRelease.RemoveListener(OnJoystickReleased);
        }
    }

    void ShowInstructions()
    {
        if (instructionsCanvas != null && instructionsText != null)
        {
            instructionsText.text = "The claw game is simple : there are 2 golden teddys, catch them and win the game. ";
            instructionsCanvas.SetActive(true);
        }
    }

    void OnJoystickGrabbed()
    {
        if (!hasShownAttempts && attemptsText != null)
        {
            attemptsText.gameObject.SetActive(true);
            hasShownAttempts = true;
            attemptsText.text = $"Attempts: 0/{maxAttempts}";
            instructionsCanvas?.SetActive(false);
        }
    }

    void OnJoystickReleased()
    {
        // Optionally handle release
    }

    public void CheckForGoldenTeddy(GameObject grabbedToy)
    {
        if (gameWon) return;
        if (isProcessingAttempt || Time.time - lastAttemptTime < attemptDebounceTime) return;
        isProcessingAttempt = true;
        lastAttemptTime = Time.time;

        if (hasShownAttempts)
        {
            attemptCount++;
            UpdateAttemptCounter();
            if (attemptCount == warningAttemptThreshold)
                audioSource.PlayOneShot(warningSound);
        }

        bool gotGolden = false;
        if (grabbedToy != null)
        {
            foreach (var teddy in goldenTeddies)
                if (grabbedToy == teddy)
                    gotGolden = true;
        }

        if (gotGolden)
        {
            ShowWinMessage();
            gameWon = true;
        }
        else if (attemptCount >= maxAttempts)
        {
            ShowGameOverMessage();
        }

        StartCoroutine(ResetProcessingFlag());
    }

    IEnumerator ResetProcessingFlag()
    {
        yield return new WaitForSeconds(attemptDebounceTime);
        isProcessingAttempt = false;
    }

    void UpdateAttemptCounter()
    {
        attemptsText.text = $"Attempts: {attemptCount}/{maxAttempts}";
        StartCoroutine(PulseText(attemptsText, 1f, 1.2f, 0.5f));
    }

    void ShowWinMessage()
    {
        StopAllCoroutines();
        winText.color = new Color(winText.color.r, winText.color.g, winText.color.b, 1f);
        winText.gameObject.SetActive(true);
        audioSource.PlayOneShot(winSound);
        StartCoroutine(FadeOutTextAfterDelay(winText, winMessageDisplayTime));
        StartCoroutine(ShowPlayAgainPromptAfterDelay(winMessageDisplayTime + 1f));
    }

    void ShowGameOverMessage()
    {
        loseText.color = Color.red;
        loseText.gameObject.SetActive(true);
        audioSource.PlayOneShot(loseSound);
        StartCoroutine(FadeOutTextAfterDelay(loseText, gameOverDisplayTime));
        StartCoroutine(ShowPlayAgainPromptAfterDelay(gameOverDisplayTime + 1f));
    }

    IEnumerator ShowPlayAgainPromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        playAgainCanvas?.SetActive(true);
        playAgainText.text = gameWon ? "Congratulations! Play again?" : "Try again?";
        audioSource.PlayOneShot(promptSound);
    }

    void ResetGameState()
    {
        attemptCount = 0; gameWon = false; hasShownAttempts = true;
        attemptsText.text = $"Attempts: 0/{maxAttempts}";
        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
        instructionsCanvas?.SetActive(false);
        SubscribeToClawEvents();
    }

    IEnumerator ResetAllToysAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        var toys = toyContainer ? toyContainer.GetComponentsInChildren<ToyController>()
                                 : FindObjectsOfType<ToyController>();
        foreach (var toy in toys)
        {
            toy.ResetToInitialPosition();
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
    }

    IEnumerator FadeOutTextAfterDelay(TMP_Text text, float delay)
    {
        yield return new WaitForSeconds(delay);
        float elapsed = 0f, duration = 2f;
        Color start = text.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.color = new Color(start.r, start.g, start.b, Mathf.Lerp(start.a, 0, elapsed / duration));
            yield return null;
        }
        text.gameObject.SetActive(false);
    }

    IEnumerator PulseText(TMP_Text text, float min, float max, float dur)
    {
        Vector3 orig = text.transform.localScale;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(min, max, t / dur);
            text.transform.localScale = orig * s;
            yield return null;
        }
        text.transform.localScale = orig;
    }
}
