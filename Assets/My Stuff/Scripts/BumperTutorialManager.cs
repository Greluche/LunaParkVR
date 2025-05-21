using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class BumperTutorialManager : MonoBehaviour
{
    public SteeringWheel wheel;
    public BlinkControl bumper;
    public BlinkingPrimary primary;
    public CarControl forward;

    public TextMeshProUGUI tutoText;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    private bool step1Complete = false;
    private bool step2Complete = false;
    private bool isFading = false;

    void Start()
    {
        tutoText.text = "Hello, welcome to the Bumping Cars Tutorial. Try to grab the wheel in front of you using the highlighted buttons under your middle fingers";
        bumper.StartBlinking();
        primary.StopBlinking();
    }

    void Update()
    {
        if (!step1Complete && wheel.wheelGrabbed)
        {
            step1Complete = true;
            bumper.StopBlinking();
            StartCoroutine(Step2());
        }

        if (step1Complete && !step2Complete && forward.accelerateButton.action.IsPressed())
        {
            step2Complete = true;
            primary.StopBlinking();
            StartCoroutine(Step3());
        }
    }

    IEnumerator Step2()
    {
        yield return StartCoroutine(WellDone("Well done!"));
        tutoText.text = "Now, use the A button on your right controller to move forward";
        primary.StartBlinking();
    }

    IEnumerator Step3()
    {
        yield return StartCoroutine(WellDone("Well done!"));
        yield return StartCoroutine(GetReady());
        TutorialProgress.MarkTutorialComplete("Played_BumperTutorial");
        SceneManager.LoadScene("BumpingCars");
    }

    IEnumerator WellDone(string message)
    {
        tutoText.text = message;
        yield return new WaitForSeconds(2f);
    }

    IEnumerator GetReady()
    {
        isFading = true;
        tutoText.text = "You are now ready to rumble, your goal will be to destroy all the other bumper cars. Good luck.";
        yield return new WaitForSeconds(5f);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvasGroup.alpha = alpha;
            elapsed += Time.deltaTime;
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
        isFading = false;
    }
}
