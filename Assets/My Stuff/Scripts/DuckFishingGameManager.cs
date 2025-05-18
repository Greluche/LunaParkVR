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
    public TextMeshProUGUI tutorialText;

    [Header("Settings")]
    public float delayBeforeStart = 3f; // sync with countdown duration

    [Header("Post-win Scene Transition")]
    public string hubSceneName = "HubScene";  // Name of your hub scene
    public float returnToHubDelay = 3f;
    
    public Transform hubSpawnPoint;
    private float countdownTime = 30f; // #TODO change time
    //private int totalEnemies;
    private int ducksJailed;
    private bool gameStarted = false;

    void Start()
    {
        ducksJailed = 0;

        if (winText != null)
            winText.gameObject.SetActive(false);
        
        counterText.text = "Ducks captured: " + ducksJailed;

        StartCoroutine(WaitForStart());
    }

    IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(delayBeforeStart);
        gameStarted = true;
    }

    void Update()
    {
        if (!gameStarted) return;

        if (countdownTime >= 0f)
        {
            int minutes = Mathf.FloorToInt(countdownTime / 60f);
            int seconds = Mathf.FloorToInt(countdownTime % 60f);
            int dsec = Mathf.FloorToInt((countdownTime * 10) % 10);
            timerText.text = $"Time: {minutes:00}:{seconds:00}:{dsec:00}";
            countdownTime -= Time.deltaTime;
        }
        else
        {
            gameStarted = false;
            timerText.color = new Color(1, 0, 0, 1);
            winText.text = "Done! You captured " + ducksJailed + " ducks";
            winText.gameObject.SetActive(true);
            //StartCoroutine(ReturnToHubAfterDelay());
        }
    }

    public void OnRodGrabbed()
    {
        winText.gameObject.SetActive(false);
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