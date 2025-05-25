using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ClawGameUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionsCanvas;
    public TextMeshPro attemptsText;
    public TextMeshPro winText;
    public TextMeshPro loseText;
    public GameObject playAgainCanvas;

    [Header("Play Again")]
    public Button yesButton;
    public Button noButton;

    [Header("Game Settings")]
    public GameObject[] goldenTeddies;
    public int maxAttempts = 5;
    public ClawScript clawController;
    public Joystick joystickController;

    [Header("Audio")]
    public AudioClip warningSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    private int attemptCount = 0;
    private bool gameWon = false;
    private AudioSource audioSource;
    private bool attemptProcessing = false;


    void Start()
    {
        // UI initial state
        instructionsCanvas?.SetActive(true);
        attemptsText?.gameObject.SetActive(false);
        winText?.gameObject.SetActive(false);
        loseText?.gameObject.SetActive(false);
        playAgainCanvas?.SetActive(false);

        // Button listeners
        yesButton?.onClick.AddListener(RestartGame);
        noButton?.onClick.AddListener(QuitGame);

        // Joystick events
        if (joystickController != null)
        {
            joystickController.onGrab.AddListener(OnJoystickGrabbed);
        }

        // Claw event (called when a drop attempt finishes)
        if (clawController != null)
        {
            clawController.OnDropCompleted.RemoveAllListeners();
            clawController.OnDropCompleted.AddListener(CheckForGoldenTeddy);
        }

        audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
        audioSource = gameObject.AddComponent<AudioSource>();

    }

// Once the joystick is used we hide the instructions, and show the attempts
    void OnJoystickGrabbed()
    {
        // Hide instructions, show attempts
        instructionsCanvas?.SetActive(false);
        attemptsText?.gameObject.SetActive(true);
        attemptsText.text = $"Attempts: {attemptCount}/{maxAttempts}";
    }


// method to check if the goldenTeddy is catched plays warning sound when 2 attempts left an show win or lose message
    public void CheckForGoldenTeddy(GameObject grabbedToy)
    {
        Debug.Log("CheckForGoldenTeddy called, attemptCount = " + attemptCount);
        if (attemptProcessing) return;
        attemptProcessing = true;
        StartCoroutine(ResetAttemptProcessing());

        if (gameWon || attemptCount >= maxAttempts) return;

        attemptCount++;
        attemptsText.text = $"Attempts: {attemptCount}/{maxAttempts}";

        // Play warning sound at 3 attempts
        if (attemptCount == 3 && warningSound != null)
            audioSource.PlayOneShot(warningSound, 0.3f);

        bool gotGolden = false;
        if (grabbedToy != null)
        {
            foreach (var teddy in goldenTeddies)
            {
                if (grabbedToy == teddy || grabbedToy.name == teddy.name)
                {
                    gotGolden = true;
                    break;
                }
            }
        }

        if (gotGolden)
        {
            ShowWin();
        }
        else if (attemptCount >= maxAttempts)
        {
            ShowLose();
        }
    }

// method to show win message when the golden teddy catched within the 5 attempts + sounds
    void ShowWin()
    {
        gameWon = true;
        winText?.gameObject.SetActive(true);
        loseText?.gameObject.SetActive(false);
        if (winSound != null) audioSource.PlayOneShot(winSound);
        StartCoroutine(ShowWinThenPlayAgain());
    }

// method to show lose message when the golden teddy isnt catched within the 5 attempts + sounds

    void ShowLose()
    {
        winText?.gameObject.SetActive(false);
        loseText?.gameObject.SetActive(true);
        if (loseSound != null) audioSource.PlayOneShot(loseSound);
        StartCoroutine(ShowLoseThenPlayAgain());
    }

// 2 methods to delay and avoid overlapping of the playagain canvas and the win/lose text
    IEnumerator ShowWinThenPlayAgain()
    {
        yield return new WaitForSeconds(3f); // Show win for 3 seconds
        winText?.gameObject.SetActive(false);
        playAgainCanvas?.SetActive(true);
    }

    IEnumerator ShowLoseThenPlayAgain()
    {
        yield return new WaitForSeconds(3f); // Show lose for 3 seconds
        loseText?.gameObject.SetActive(false);
        playAgainCanvas?.SetActive(true);
    }

// to restart the game if the player answer yes to playagain canvas
    void RestartGame()
    {
        attemptCount = 0;
        gameWon = false;
        attemptsText.text = $"Attempts: 0/{maxAttempts}";
        attemptsText?.gameObject.SetActive(true);
        winText?.gameObject.SetActive(false);
        loseText?.gameObject.SetActive(false);
        playAgainCanvas?.SetActive(false);
        instructionsCanvas?.SetActive(true);
    }

    private IEnumerator ResetAttemptProcessing()
    {
        yield return new WaitForSeconds(0.5f);
        attemptProcessing = false;
    }

    void QuitGame()
    {
        playAgainCanvas?.SetActive(false);
    }
}
