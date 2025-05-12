using UnityEngine;
using UnityEngine.XR;

public class FacePlayerUI : MonoBehaviour
{
    private Transform xrHead;

    void Start()
    {
        xrHead = Camera.main?.transform;

        if (xrHead == null)
        {
            Debug.LogWarning("FacePlayer: Couldn't find XR headset camera (Camera.main).");
        }
    }

    void Update()
    {
        if (xrHead == null) return;

        // Get direction to the player
        Vector3 direction = xrHead.position - transform.position;

        // Zero out vertical component to keep the canvas upright
        direction.y = 0;

        // Avoid zero-length vectors
        if (direction.sqrMagnitude < 0.001f) return;

        // Rotate to face player (on horizontal plane only)
        transform.rotation = Quaternion.LookRotation(direction);

        // Flip 180 degrees to show front of canvas
        transform.Rotate(0, 180f, 0);
    }
}