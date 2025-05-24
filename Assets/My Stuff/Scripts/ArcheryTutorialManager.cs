using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class ArcheryTutorialManager : MonoBehaviour
{
    
    public GameObject bow;
    public BlinkControl bumper;
    public BlinkControl1 bumper2;

    public BlinkingPrimary primary;
    public GameObject arrow;

    public TextMeshProUGUI tutoText;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    private bool step1Complete = false;
    private bool step2Complete = false;
    private bool step3Complete = false;

    private bool isFading = false;
    public BowIsGrabbed bow_script;
    public GrabBow grab_bow;
    public GrabBow grab_bow2;
    public RubberDuckArcheryTutorial rdat;

    void Start()
    {
        bow_script = bow.GetComponent<BowIsGrabbed>();
        tutoText.text = "Hello, welcome to the Archery Tutorial. Try to grab the bow with your left hand using the highlighted button under your middle finger";
        bumper.StartBlinking();
        primary.StopBlinking();
        bumper2.StopBlinking1();
    }

    void Update()
    {

        if (!step1Complete && bow_script.isBowGrabbed)
        {
            step1Complete = true;
            bumper.StopBlinking();
            primary.StartBlinking();
            StartCoroutine(Step2());
        }

        if (step1Complete && !step2Complete && grab_bow.isArrowGrabbed)
        {
            step2Complete = true;
            primary.StopBlinking();
            bumper2.StartBlinking1();
            StartCoroutine(Step3());
        }
        if (step1Complete && step2Complete && !step3Complete && grab_bow2.isArrowGrabbed)
        {
            step3Complete = true;

            bumper2.StopBlinking1();
            StartCoroutine(Step4());
        }
        if (step1Complete && step2Complete && step3Complete && rdat.isHit)
        {
            
            StartCoroutine(Step5());
        }
        
    }

    IEnumerator Step2()
    {
        yield return StartCoroutine(WellDone("Well done!"));
        tutoText.text = "Now, use the A button on your right controller to click on the arrows";
        primary.StartBlinking();
    }

    IEnumerator Step5()
    {
        yield return StartCoroutine(WellDone("Well done!"));
        yield return StartCoroutine(GetReady());
        TutorialProgress.MarkTutorialComplete("Played_ArcheryTutorial");
        SceneManager.LoadScene("Archery");
    }
    IEnumerator Step3()
    {   
        yield return StartCoroutine(WellDone("Well done!"));
        tutoText.text = "Now, use the inside trigger on you right hand to grab the arrow ";
        
    }
    
    IEnumerator Step4()
    {   
        yield return StartCoroutine(WellDone("Well done!"));
        tutoText.text = "Keep the trigger down and pull ! When you release the trigger, the arrow will shoot ! Try and hit the rubber duck !";
        
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
