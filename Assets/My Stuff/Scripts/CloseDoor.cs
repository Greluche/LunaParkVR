using System;
using UnityEngine;

public class CloseDoor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AudioSource audioSource;
    public Animator animator;
    public string closeTrigger;
    public string openTrigger;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            if (audioSource != null)
                audioSource.Play();

            if (animator != null)
                animator.SetTrigger(openTrigger);
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("MainCamera"))
        {
            if (audioSource != null)
                audioSource.Play();

            if (animator != null)
                animator.SetTrigger(closeTrigger);

        }
    }
}
