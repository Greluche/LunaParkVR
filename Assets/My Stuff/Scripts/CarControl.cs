using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float turnSpeed = 100f;

    [Header("Steering Input")]
    [Range(-1f, 1f)]
    public float steeringValue = 0f;

    private void Update()
    {
        // Move the car forward constantly
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Turn the car based on steering input
        transform.Rotate(Vector3.up, (steeringValue*2 -1) * turnSpeed * Time.deltaTime);
    }

    // You can call this from the steering wheel script
    public void SetSteeringValue(float value)
    {
        steeringValue = Mathf.Clamp(value, -1f, 1f);
    }
}