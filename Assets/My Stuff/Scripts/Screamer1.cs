using UnityEngine;

public class Screamer1 : MonoBehaviour
{
    public Animator animator;
    public bool oneTimeOnly = true;
    public string triggerName = "Screamer1";
    private bool hasTriggered = false;
    public AudioSource audioSource;

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
    }
}
