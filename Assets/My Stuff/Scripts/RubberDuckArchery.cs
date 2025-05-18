using UnityEngine;

public class RubberDuckArchery : MonoBehaviour
{
    public ParticleSystem animation;
    public GameObject duck;
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
    }
}
