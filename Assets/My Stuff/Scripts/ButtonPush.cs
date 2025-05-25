using UnityEngine;

public class ButtonPush : MonoBehaviour
{
    public BackroomsManager manager;
    public AudioSource audioSource;  
    public void ButtonPushed()
    {
        if (audioSource != null)
            audioSource.Play();
        manager.MarkPuzzleComplete(1);
    }
}
