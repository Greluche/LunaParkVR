using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadBackrooms : MonoBehaviour
{
    public Transform hubSpawn;
    
    [Header("Door Slam")] 
    public GameObject backWall;
    public AudioSource doorSlam;
    
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Target Scene")]
    [SerializeField] private string sceneToLoad = "Backrooms";
    

    private bool isFading = false;

    private void Start()
    {
        backWall.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isFading) return;

        if (other.CompareTag("Player") || other.CompareTag("MainCamera")) // adjust tag if needed
        {
            StartCoroutine(FadeAndLoad());
        }
    }

    private IEnumerator FadeAndLoad()
    {
        isFading = true;
        yield return new WaitForSeconds(1f);
        backWall.SetActive(true);
        doorSlam.Play();
        yield return new WaitForSeconds(1f);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvasGroup.alpha = alpha;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (hubSpawn != null)
        {
            Debug.Log("Loading back room");
            SpawnPointManager.hubSpawnPosition = hubSpawn.position;
            SpawnPointManager.hubSpawnRotation = hubSpawn.rotation;
        }
        
        fadeCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(0.5f); // optional delay
        SceneManager.LoadScene(sceneToLoad);
    }
}