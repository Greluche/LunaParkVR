using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DuckBowlJudge : MonoBehaviour
{
    public string correctDuckColor = "Red";
    public BackroomsManager manager;
    public TextMeshProUGUI scrollText;
    public ParticleSystem fireEffect;
    public AudioSource screamAudio;
    public AudioSource fireAudio;
    public XRSocketInteractor socket;
    public GameObject duckBowl;
    public AudioSource successAudio;
    public ParticleSystem successEffect;
    private void Start()
    {
        scrollText.text = "The light will guide you towards the correct duck to be placed in this bowl";
    }
    private void Awake()
    {
        socket.selectEntered.AddListener(OnDuckInserted);
    }

    private void OnDestroy()
    {
        socket.selectEntered.RemoveListener(OnDuckInserted);
    }

    private void OnDuckInserted(SelectEnterEventArgs args)
    {
        var duck = args.interactableObject.transform.GetComponent<DuckIdentity>();
        if (duck == null) return;
        
        if (duck.duckColor == correctDuckColor)
        {
            Debug.Log("✅ Correct duck!");
            scrollText.text = "Red as its blood, the duck god thanks you. You may go back to the entrance.";
            manager.MarkPuzzleComplete(2);
            // ✅ Play success sound
            if (successAudio != null)
                successAudio.Play();

            // ✅ Spawn visual effect (sparkles, etc.)
            if (successEffect != null)
            {
                ParticleSystem fx = Instantiate(successEffect, duckBowl.transform.position, Quaternion.LookRotation(Vector3.up));
                fx.transform.localScale = Vector3.one * 2.0f;
                fx.Play();
                Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
            }
        }
        else
        {
            Debug.Log("🔥 Wrong duck!");
            StartCoroutine(BurnTheDuck(duck.gameObject));
        }
    }

    private IEnumerator BurnTheDuck(GameObject duck)
    {
        Vector3 centerPosition = duckBowl.transform.position;
        yield return new WaitForSeconds(2f);
        scrollText.text = "WRONG, HE NOW DIES IN AGONY";
        ParticleSystem fire = Instantiate(fireEffect, centerPosition, Quaternion.identity);
        fire.transform.rotation = Quaternion.LookRotation(Vector3.up);
        fire.transform.localScale = Vector3.one * 2.0f;
        fire.Play();
        
        // Play scream
        screamAudio.Play();
        fireAudio.Play();
        // Optional: Delay before destroying
        yield return new WaitForSeconds(4f);

        Destroy(duck);
        screamAudio.Stop();
        yield return new WaitForSeconds(2f);
        Destroy(fire.gameObject, fire.main.duration + fire.main.startLifetime.constantMax);
        yield return new WaitForSeconds(1f);
        fireAudio.Stop();
        scrollText.text =  "The light will guide you towards the correct duck to be placed in this bowl";
        
    }
}