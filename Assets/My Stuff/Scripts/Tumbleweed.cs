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
        GetComponent<Rigidbody>()?.AddForce(new Vector3(0, 0.005f, 0.04f), ForceMode.Impulse);
    }
}
