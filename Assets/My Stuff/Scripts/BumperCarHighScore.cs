using TMPro;
using UnityEngine;

public class BumperCarHighScore : MonoBehaviour
{

    public TextMeshProUGUI bumperCarScoreText;

    private float bumperCarNewScore = 1000f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (HighScoreManager.BumpercarHighscore > 0f)
        {
            Debug.Log("Manager score: " + HighScoreManager.BumpercarHighscore);
            if (HighScoreManager.BumpercarHighscore < bumperCarNewScore)
            {
                bumperCarNewScore = HighScoreManager.BumpercarHighscore;
                int minutes = Mathf.FloorToInt(bumperCarNewScore / 60f);
                int seconds = Mathf.FloorToInt(bumperCarNewScore % 60f);
                int dsec = Mathf.FloorToInt((bumperCarNewScore * 10) % 10);
                bumperCarScoreText.text = $"High score: {minutes:00}:{seconds:00}:{dsec:00}";
            }
        }
        else
        {
            bumperCarScoreText.text = "High score: None";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
