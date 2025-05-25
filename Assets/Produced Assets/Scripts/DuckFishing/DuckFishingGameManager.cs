using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DuckFishingGameManager : MonoBehaviour
{
    [Header("UI REFERENCES")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI tutorialTextRod;
    public TextMeshProUGUI tutorialTextDuck;
    public TextMeshProUGUI tutorialDone;
    public Button tutorialSkip;

    [Header("SETTINGS")]
    public bool tutorialCompleted = false;
    private bool gameStarted = false;

    [Header("POST-WIN SCENE TRANSITION")]
    public string hubSceneName = "Hub";
    public float returnToHubDelay = 3f;
    public Transform hubSpawnPoint;

    private float gameCountdown = 30f; // #TODO change time
    private int ducksJailed = 0;
    private float initialCountdown = 3f;
    private GameObject button;
    private GameObject rod;

    void Start()
    {
        tutorialSkip.onClick.AddListener(SkipTutorial);

        button = GameObject.Find("ButtonTutorial");
        rod = GameObject.Find("TutorialFishingRod");

        if (winText != null)
            winText.gameObject.SetActive(false);

        if (tutorialTextDuck != null)
            tutorialTextDuck.gameObject.SetActive(false);

        if (tutorialDone != null)
            tutorialDone.gameObject.SetActive(false);
    }

    void SkipTutorial()
    {
        if (!tutorialCompleted)
        {
            tutorialCompleted = true;
            GameStart();
        }
    }

    // tutorial pt.1: grab rod
    public void OnRodGrabbed()
    {

        tutorialTextRod.gameObject.SetActive(false);
        Destroy(button);
        Destroy(rod);
    }

    // tutorial pt.2: grab duck
    public void OnDuckGrabbed()
    {
        if (!tutorialCompleted)
            tutorialTextDuck.gameObject.SetActive(true);
    }

    // tutorial pt.3: jail duck
    public void OnDuckJailed()
    {
        ducksJailed++;
        counterText.text = "Ducks captured: " + ducksJailed;

        if (!tutorialCompleted)
        {
            tutorialCompleted = true;
            GameStart();
        }
    }

    // tutorial done, start game
    private void GameStart()
    {
        tutorialTextRod.gameObject.SetActive(false);
        tutorialTextDuck.gameObject.SetActive(false);
        tutorialSkip.gameObject.SetActive(false);

        tutorialDone.gameObject.SetActive(true);

        StartCoroutine(CountdownRoutine());
    }

    // countdown before game
    private IEnumerator CountdownRoutine()
    {
        yield return new WaitForSeconds(3f);
        tutorialDone.gameObject.SetActive(false);

        countdownText.gameObject.SetActive(true);

        while (initialCountdown > 0f)
        {
            countdownText.text = Mathf.Ceil(initialCountdown).ToString();
            yield return new WaitForSeconds(1f);
            initialCountdown -= 1f;
        }

        // Show "GO!"
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);

        gameStarted = true;
    }

    // update if game is started
    void Update()
    {
        if (!gameStarted) return;

        if (gameCountdown >= 0f)
        {
            counterText.text = "Ducks captured: " + ducksJailed;

            int minutes = Mathf.FloorToInt(gameCountdown / 60f);
            int seconds = Mathf.FloorToInt(gameCountdown % 60f);
            int dsec = Mathf.FloorToInt((gameCountdown * 10) % 10);
            timerText.text = $"Time: {minutes:00}:{seconds:00}:{dsec:00}";
            gameCountdown -= Time.deltaTime;
        }
        else
        {
            StartCoroutine(GameFinished());
        }
    }

    // game done
    private IEnumerator GameFinished()
    {
        yield return new WaitForSeconds(3f);

        gameStarted = false;
        timerText.color = new Color(1, 0, 0, 1);
        winText.text = "Done! You captured " + ducksJailed + " ducks";
        winText.gameObject.SetActive(true);
        StartCoroutine(ReturnToHubAfterDelay());
    }

    // return to hub after game is done
    private IEnumerator ReturnToHubAfterDelay()
    {
        yield return new WaitForSeconds(returnToHubDelay);

        // Set spawn point for the hub
        SpawnPointManager.hubSpawnPosition = hubSpawnPoint.position;
        SpawnPointManager.hubSpawnRotation = hubSpawnPoint.rotation;

        SceneManager.LoadScene(hubSceneName);
    }
}