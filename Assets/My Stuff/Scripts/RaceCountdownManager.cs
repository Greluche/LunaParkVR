using UnityEngine;

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // or TMPro if using TextMeshPro

public class RaceCountdownManager : MonoBehaviour
{
    public float countdownTime = 3f;
    public TextMeshProUGUI countdownText; // Or TextMeshProUGUI if using TMP

    public MonoBehaviour[] playerScriptsToDisable;
    public MonoBehaviour[] aiScriptsToDisable;
    
    [Header("Countdown Audio")]
    public AudioSource countdownAudioSource;
    public AudioClip Go;
    
    private bool countdownStarted = false;
    void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        // Disable all control scripts
        SetScriptsEnabled(false);
        // Countdown display sync
        float timeLeft = countdownTime;
        
        
        while (timeLeft > 0f)
        {
            countdownText.text = Mathf.Ceil(timeLeft).ToString();
            yield return new WaitForSeconds(0.2f);
            if (countdownStarted == false)
            {
                countdownAudioSource.clip = Go; 
                countdownAudioSource.Play();
            }    
            yield return new WaitForSeconds(0.8f);
            timeLeft -= 1f;

            countdownStarted = true;
        }

        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);

        // Enable gameplay
        SetScriptsEnabled(true);
    }

    void SetScriptsEnabled(bool state)
    {
        foreach (var script in playerScriptsToDisable)
            script.enabled = state;

        foreach (var script in aiScriptsToDisable)
            script.enabled = state;
    }
}
