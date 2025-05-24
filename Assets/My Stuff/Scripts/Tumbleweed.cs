using UnityEngine;

public class Tumbleweed : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Rigidbody>()?.AddForce(new Vector3(Random.Range(-0.1f,0.1f), 0.06f + Random.Range(0.005f,0.02f), 0.065f + Random.Range(0.005f,0.03f)), ForceMode.Impulse);
    }
}
