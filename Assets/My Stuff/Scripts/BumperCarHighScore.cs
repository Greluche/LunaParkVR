using TMPro;
using UnityEngine;

public class BumperCarHighScore : MonoBehaviour
{

    public TextMeshProUGUI bumperCarScoreText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float bestTime = PlayerPrefs.GetFloat("BumpercarHighscore", 0f);

        if (bestTime > 0f)
        {
            int minutes = Mathf.FloorToInt(bestTime / 60f);
            int seconds = Mathf.FloorToInt(bestTime % 60f);
            int dsec = Mathf.FloorToInt((bestTime * 10) % 10);

            bumperCarScoreText.text = $"High score: {minutes:00}:{seconds:00}:{dsec:00}";
        }
        else
        {
            bumperCarScoreText.text = "High score: --:--:--";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
