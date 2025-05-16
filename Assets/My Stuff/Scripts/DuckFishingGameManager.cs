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

    [Header("Settings")]
    public float delayBeforeStart = 3f; // sync with countdown duration

    [Header("Post-win Scene Transition")]
    public string hubSceneName = "HubScene";  // Name of your hub scene
    public float returnToHubDelay = 3f;
    
    public Transform hubSpawnPoint;
    private float remainingTime = 180f; // 3 minutes
    //private int totalEnemies;
    private int ducksJailed;
    private bool gameStarted = false;

    void Start()
    {
        // Find all AI bumper cars at the start
        //AIBumperCar[] allEnemies = FindObjectsOfType<AIBumperCar>();
        //totalEnemies = allEnemies.Length;
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

        remainingTime -= Time.deltaTime;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    public void OnDuckJailed()
    {
        ducksJailed++;
        counterText.text = "Ducks captured: " + ducksJailed;

        if (remainingTime == 0)
        {
            gameStarted = false;
            winText.text = "Done! You captured " + ducksJailed + " ducks";
            winText.gameObject.SetActive(true);
            StartCoroutine(ReturnToHubAfterDelay());
        }
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