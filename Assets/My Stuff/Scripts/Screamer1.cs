using UnityEngine;

public class Screamer1 : MonoBehaviour
{
    public Animator animator;
    public string triggerName = "Screamer1";
    public AudioSource audioSource;
    private bool onlyOnce = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            if (onlyOnce == false)
            {
                onlyOnce = true;
                if (audioSource != null)
                    audioSource.Play();

                if (animator != null)
                    animator.SetTrigger(triggerName);
            }
        }
    }
}
