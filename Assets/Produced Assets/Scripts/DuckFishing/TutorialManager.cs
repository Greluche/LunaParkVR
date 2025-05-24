using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("UI REFERENCES")]
    public TextMeshProUGUI tutorialTextRod;
    public TextMeshProUGUI tutorialTextDuck;
    public TextMeshProUGUI goToGameText;

    private string gameSceneName = "DuckFishing";
    private float goToGameDelay = 3f;

    void Start()
    {
        if (tutorialTextDuck != null)
            tutorialTextDuck.gameObject.SetActive(false);

        if (goToGameText != null)
            goToGameText.gameObject.SetActive(false);
    }

    void Update()
    {
    }

    public void OnDuckJailed()
    {
        goToGameText.gameObject.SetActive(true);
        StartCoroutine(GoToGame());
    }

    private IEnumerator GoToGame()
    {
        yield return new WaitForSeconds(goToGameDelay);

        SceneManager.LoadScene(gameSceneName);
    }
}