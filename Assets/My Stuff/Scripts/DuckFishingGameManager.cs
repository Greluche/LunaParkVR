using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DuckFishingGameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI tutorialTextRod;
    public TextMeshProUGUI tutorialTextDuck;
    public TextMeshProUGUI countdownText;

    [Header("Settings")]
    public float delayBeforeStart = 3f; // sync with countdown duration

    [Header("Post-win Scene Transition")]
    public string hubSceneName = "HubScene";  // Name of your hub scene
    public float returnToHubDelay = 3f;
    
    public Transform hubSpawnPoint;
    public bool gameStarted = false;

    private float gameCountdown = 30f; // #TODO change time
    //private int totalEnemies;
    private int ducksJailed;
    private float initialCountdown = 3f;

    void Start()
    {
        ducksJailed = 0;

        if (winText != null)
            winText.gameObject.SetActive(false);

        if (tutorialTextDuck != null)
            tutorialTextDuck.gameObject.SetActive(false);
    }

    public IEnumerator CountdownRoutine()
    {
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
            //gameStarted = false;
            timerText.color = new Color(1, 0, 0, 1);
            winText.text = "Done! You captured " + ducksJailed + " ducks";
            winText.gameObject.SetActive(true);
            StartCoroutine(ReturnToHubAfterDelay());
        }
    }

    public void OnDuckJailed()
    {
        ducksJailed++;
        counterText.text = "Ducks captured: " + ducksJailed;
    }
    
    private IEnumerator ReturnToHubAfterDelay()
    {
        yield return new WaitForSeconds(returnToHubDelay);

        // Set spawn point for the hub
        SpawnPointManager.hubSpawnPosition = hubSpawnPoint.position;
        SpawnPointManager.hubSpawnRotation = hubSpawnPoint.rotation;

        SceneManager.LoadScene(hubSceneName);
    }

}