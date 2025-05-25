using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArcheryGameManager : MonoBehaviour
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
        
        counterText.text = "Score: " + ducksJailed;

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
        if (remainingTime >= 0)
        {
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
        else
        {
            timerText.text = $"Time: 00:00";
        }
        

        if (remainingTime <= 0)
        {
            gameStarted = false;
            if (ducksJailed >= HighScoreManager.ArcheryHighScore)
            {
                winText.text = "Done! You got  " + ducksJailed + " points ! New High Score !";
                HighScoreManager.ArcheryHighScore = ducksJailed;
            }
            else
            {
                winText.text = "Done! You got  " + ducksJailed + " points";
            }
            
            
            winText.gameObject.SetActive(true);
            StartCoroutine(ReturnToHubAfterDelay());
        }
    }

    public void OnRodGrabbed()
    {
        winText.gameObject.SetActive(false);
    }

    public void OnDuckHit(int score)
    {
        ducksJailed+=score;
        counterText.text = "Score: " + ducksJailed;
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