using System;
using UnityEngine;

public class EndingRoom : MonoBehaviour
{

    public GameObject wallBehind;
    public string returnScene = "Hub";
    public AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wallBehind.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("MainCamera"))
        {
            wallBehind.SetActive(true);
            if (audioSource != null)
                audioSource.Play();
            
        }
    }
}
