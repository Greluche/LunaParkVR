using UnityEngine;

public class RubberDuckArcheryTutorial : MonoBehaviour
{
    public ParticleSystem animation;
    public GameObject duck;
    public bool isHit = false;
    private ArcheryGameManager bow_script;
    public AudioSource source;
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
        Destroy(duck, 0.5f);
        isHit = true;
        source.Play();
    }
}
