using UnityEngine;

public class FloatingDuck : MonoBehaviour
{
    public float angularSpeed = .3f;
    public float radius = 2f;
    public float shift = 0f;
    public Transform center;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // trajectory
        float x = Mathf.Cos((Time.time + shift) * angularSpeed) * radius;
        float z = Mathf.Sin((Time.time + shift) * angularSpeed) * radius;
        // illusion of floating
        float y = Mathf.PingPong(Time.time + shift, .05f);
        
        transform.position = new Vector3(x, y, z);
        transform.LookAt(center);
    }
}
