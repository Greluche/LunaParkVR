using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("List of audio clips to play randomly.")]
    [SerializeField] private AudioClip[] clips;

    [Tooltip("Minimum time between sounds (in seconds).")]
    [SerializeField] private float minDelay = 3f;

    [Tooltip("Maximum time between sounds (in seconds).")]
    [SerializeField] private float maxDelay = 10f;

    [Tooltip("Play sound on awake.")]
    [SerializeField] private bool playOnStart = true;

    private AudioSource audioSource;
    private float nextPlayTime;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (playOnStart)
        {
            ScheduleNextSound();
        }
    }

    private void Update()
    {
        if (clips.Length == 0 || !audioSource)
            return;

        if (Time.time >= nextPlayTime && !audioSource.isPlaying)
        {
            PlayRandomClip();
            ScheduleNextSound();
        }
    }

    private void PlayRandomClip()
    {
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }

    private void ScheduleNextSound()
    {
        nextPlayTime = Time.time + Random.Range(minDelay, maxDelay);
    }

    // Optional method to trigger sound logic externally
    public void TriggerManually()
    {
        PlayRandomClip();
        ScheduleNextSound();
    }
}
