using UnityEngine;
public class CircularMotion : MonoBehaviour
{
    public float radius = 2f;
    public float speed = 1f;
    private float angle = 0f;
    private Vector3 center;

    void Start()
    {
        center = transform.position;
    }

    void Update()
    {
        // Update the angle over time
        angle += speed * Time.deltaTime;

        // Compute new position on the circle
        float x = center.x + Mathf.Cos(angle) * radius;
        float z = center.z + Mathf.Sin(angle) * radius;
        Vector3 newPos = new Vector3(x, transform.position.y, z);
        transform.position = newPos;

        // Compute direction of movement (tangent to the circle)
        Vector3 direction = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
        transform.rotation = Quaternion.LookRotation(direction);
    }
}