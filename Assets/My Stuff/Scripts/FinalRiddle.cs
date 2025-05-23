using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DuckGodIntro : MonoBehaviour
{
    public AudioSource audioSource;
    public Animator animator;
    public string triggerName = "Scream";
    public bool oneTimeOnly = true;
    public RiddleDialogue dialogueScript;
    
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && oneTimeOnly) return;

        if (other.CompareTag("MainCamera"))
        {
            hasTriggered = true;

            if (audioSource != null)
                audioSource.Play();

            if (animator != null)
                animator.SetTrigger(triggerName);
            
        }

        StartCoroutine(TriggerDialogueAfterDelay());
    }
    private IEnumerator TriggerDialogueAfterDelay()
    {   
        Debug.Log("TriggerDialogueAfterDelay");
        yield return new WaitForSeconds(10f);
        
        if (dialogueScript != null)
            dialogueScript.StartDialogue();
    }
}