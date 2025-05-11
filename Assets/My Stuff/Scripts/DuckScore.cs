using UnityEngine;
using UnityEngine.UI;

public class DuckScore : MonoBehaviour
{
    public static int score;
    public Text scoreText;
    
    private Camera mainCamera;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        score = 0;
        UpdateScore();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0);
        UpdateScore();
    }

    void UpdateScore()
    {
        scoreText.text = "Score: " + score.ToString();
    }
}
