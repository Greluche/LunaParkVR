using UnityEngine;

public class FacePlayerNPC : MonoBehaviour
{
    [SerializeField] private float lookAtDistance = 2f;
    [SerializeField] private float rotationSpeed = 2f; // Rotation speed (higher = faster)
    private Transform xrHead;
    private Quaternion originalRotation;
    private bool playerInRange = false;
    
    void Start()
    {
        xrHead = Camera.main?.transform;
        originalRotation = transform.rotation;

        if (xrHead == null)
        {
            Debug.LogWarning("FacePlayer: Could not find XR headset camera (Camera.main). Make sure your XR camera is tagged as MainCamera.");
        }
    }

    void Update()
    {
        if (xrHead == null) return;

        float distance = Vector3.Distance(transform.position, xrHead.position);
        playerInRange = distance <= lookAtDistance;

        Quaternion targetRotation;

        if (playerInRange)
        {
            Vector3 direction = xrHead.position - transform.position;
            direction.y = 0f; // keep upright

            if (direction.sqrMagnitude > 0.001f)
            {
                targetRotation = Quaternion.LookRotation(direction);
            }
            else
            {
                targetRotation = transform.rotation;
            }
        }
        else
        {
            targetRotation = originalRotation;
        }

        // Smooth rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}