using UnityEngine;

public class ButtonRoom : MonoBehaviour
{
    public GameObject endingText;
    public GameObject lightRoom;
    public bool oneTimeOnly = true;
    private bool hasTriggered = false;
    public AudioSource audioSource;    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        endingText.SetActive(false);
        lightRoom.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && oneTimeOnly) return;
    
        if (other.CompareTag("MainCamera"))
        {
            hasTriggered = true;
    
            if (audioSource != null)
                audioSource.Play();
            
            endingText.SetActive(true);
            lightRoom.SetActive(true);
    
        }
    }

}
