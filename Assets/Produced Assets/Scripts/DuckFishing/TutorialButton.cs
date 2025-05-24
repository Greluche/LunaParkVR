using UnityEngine;
using System.Collections;

public class TutorialButton : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            GetComponent<Renderer>().enabled = false;
            yield return new WaitForSeconds(0.2f);

            GetComponent<Renderer>().enabled = true;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
