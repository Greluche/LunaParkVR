using UnityEngine;

public class RubberDuckArchery : MonoBehaviour
{
    public ParticleSystem animation;
    public GameObject duck;
    public GameObject gameManager;
    private ArcheryGameManager bow_script;
    public AudioSource source;
    public int score;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnCollisionEnter(Collision collision)
    {
        animation.Play();
        Destroy(duck, 1);
        gameManager.GetComponent<ArcheryGameManager>().OnDuckHit(score);
        source.Play();
        Destroy(collision.gameObject, 0.5f);
    }
}
