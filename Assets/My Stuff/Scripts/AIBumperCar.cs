using UnityEngine;

public class AIBumperCar : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float turnSpeed = 60f;
    public float changeDirectionInterval = 2f;

    [Header("Arena Bounds")]
    public Transform arenaCenter;
    public float arenaWidth = 20f;
    public float arenaHeight = 20f;

    private float directionTimer = 0f;
    private Vector3 targetDirection;

    void Start()
    {
        PickNewDirection();
    }

    void Update()
    {
        directionTimer -= Time.deltaTime;

        if (directionTimer <= 0f)
        {
            PickNewDirection();
        }

        // Smooth turn toward target direction
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDirection, turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(newDir);

        // Move forward
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);

        // Check if we're going out of bounds
        if (!IsInsideArena(transform.position))
        {
            // Turn back toward center
            targetDirection = (arenaCenter.position - transform.position).normalized;
        }
    }

    void PickNewDirection()
    {
        Vector2 rand2D = Random.insideUnitCircle.normalized;
        targetDirection = new Vector3(rand2D.x, 0, rand2D.y);
        directionTimer = changeDirectionInterval;
    }

    bool IsInsideArena(Vector3 pos)
    {
        Vector3 local = pos - arenaCenter.position;
        float halfW = arenaWidth / 2f;
        float halfH = arenaHeight / 2f;

        return Mathf.Abs(local.x) <= halfW && Mathf.Abs(local.z) <= halfH;
    }
}