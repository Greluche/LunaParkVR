using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;



public class BumperCarGameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI winText;

    [Header("Settings")]
    public float delayBeforeStart = 4f; // sync with countdown duration

    [Header("Post-win Scene Transition")]
    public string hubSceneName = "HubScene";  // Name of your hub scene
    public float returnToHubDelay = 3f;
    
    public Transform hubSpawnPoint;
    private float elapsedTime = 0f;
    private int totalEnemies;
    private int enemiesRemaining;
    private bool gameStarted = false;
    
    [Header("Haptics")]
    public HapticImpulsePlayer leftHaptics;
    public HapticImpulsePlayer rightHaptics;

    [Header("Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;
    
    void Start()
    {
        // Find all AI bumper cars at the start
        AIBumperCar[] allEnemies = FindObjectsOfType<AIBumperCar>();
        totalEnemies = allEnemies.Length;
        enemiesRemaining = totalEnemies;

        if (winText != null)
            winText.gameObject.SetActive(false);
        
        counterText.text = "Cars left: " + enemiesRemaining;

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

        elapsedTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int dsec = Mathf.FloorToInt((elapsedTime * 10) % 10);
        timerText.text = $"Time: {minutes:00}:{seconds:00}:{dsec:00}";
    }

    public void OnAICarDestroyed()
    {
        enemiesRemaining--;
        counterText.text = "Cars left: " + enemiesRemaining;
        if (leftHaptics != null)
        {
            Debug.Log("Left Works");
            leftHaptics.SendHapticImpulse(0.8f, 0.15f);
        }

        if (rightHaptics != null)
        {
            Debug.Log("Right Works");
            rightHaptics.SendHapticImpulse(0.8f, 0.15f);
        }

        if (enemiesRemaining <= 0)
        {
            gameStarted = false;
            HighScoreManager.BumpercarHighscore = elapsedTime;
            float savedHighScore = PlayerPrefs.GetFloat("BumpercarHighscore", 0f);
            if (savedHighScore == 0f || elapsedTime < savedHighScore)
            {
                PlayerPrefs.SetFloat("BumpercarHighscore", elapsedTime);
                PlayerPrefs.Save(); // 🔐 Writes it to disk
            }
            if (winText != null)
                winText.gameObject.SetActive(true);
            StartCoroutine(ReturnToHubAfterDelay());
        }
    }
    
    private IEnumerator ReturnToHubAfterDelay()
    {
        yield return new WaitForSeconds(returnToHubDelay);

        // Start fade out
        if (fadeCanvasGroup != null)
            yield return StartCoroutine(FadeOut());

        // Set spawn point for the hub
        SpawnPointManager.hubSpawnPosition = hubSpawnPoint.position;
        SpawnPointManager.hubSpawnRotation = hubSpawnPoint.rotation;

        SceneManager.LoadScene(hubSceneName);
    }
    
    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvasGroup.alpha = alpha;
            elapsed += Time.deltaTime;
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

}