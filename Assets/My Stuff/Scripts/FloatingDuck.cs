using UnityEngine;

public class FloatingDuck : MonoBehaviour
{
    public float angularSpeed = .3f;
    public float radius = 2f;
    public float test = 1f;
    
    private float angle = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Mathf.Cos(Time.time * angularSpeed) * radius;
        float y = transform.position.y;
        float z = Mathf.Sin(Time.time * angularSpeed) * radius;
        transform.position = new Vector3(x, y, z);
        
        angle += Time.deltaTime * test * angularSpeed;
        transform.Rotate(0, - Time.deltaTime * angularSpeed * test, 0, Space.Self);
        
        //vertical = Input.GetAxis("Vertical");
        //horizontal = Input.GetAxis("Horizontal");
        //Vector3 direction = Quaternion.Euler(0, angle, 0);
        //transform.position = center + direction * radius;
    }
}
