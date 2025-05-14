using UnityEngine;

public class AIBumperCar : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float turnSpeed = 90f;
    public float directionChangeInterval = 2f;
    public GameObject explosionPrefab;

    [Header("Awareness")]
    public Transform playerTransform;
    public float fleeDistance = 8f;
    public float wallDetectionDistance = 2f;

    [Header("Arena Bounds")]
    public Transform arenaCenter;
    public float arenaWidth = 20f;
    public float arenaHeight = 20f;

    private Rigidbody rb;
    private float directionTimer = 0f;
    private Vector3 targetDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        PickRandomDirection();
    }

    void Update()
    {
        float dt = Time.fixedDeltaTime;

        directionTimer -= dt;
        if (directionTimer <= 0f)
        {
            UpdateDirection();
        }

        // Steering
        Vector3 flatForward = rb.rotation * Vector3.forward;
        Vector3 flatTarget = targetDirection;
        flatForward.y = 0;
        flatTarget.y = 0;

        Vector3 newDir = Vector3.RotateTowards(flatForward, flatTarget, turnSpeed * Mathf.Deg2Rad * dt, 0f);
        rb.MoveRotation(Quaternion.LookRotation(newDir));

        // Forward motion
        Vector3 move = rb.rotation * Vector3.forward * moveSpeed * dt;
        rb.MovePosition(rb.position + move);
    }

    void UpdateDirection()
    {
        if (IsNearWall())
        {
            targetDirection = GetSmartWallAvoidanceDirection();
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) < fleeDistance)
        {
            Vector3 fleeDir = (transform.position - playerTransform.position).normalized;
            targetDirection = new Vector3(fleeDir.x, 0, fleeDir.z);
        }
        else
        {
            PickRandomDirection();
        }

        directionTimer = directionChangeInterval;
    }
    
    Vector3 GetSmartWallAvoidanceDirection()
    {
        RaycastHit hit;

        // Cast a short ray forward to detect walls
        if (Physics.Raycast(transform.position, transform.forward, out hit, wallDetectionDistance))
        {
            // Reflect off the wall surface (bounce logic)
            Vector3 incoming = transform.forward;
            Vector3 reflect = Vector3.Reflect(incoming, hit.normal);
            reflect.y = 0;
            return reflect.normalized;
        }

        // If no forward wall, try to correct based on arena edges
        Vector3 local = transform.position - arenaCenter.position;
        Vector3 correction = Vector3.zero;

        float halfW = arenaWidth / 2f;
        float halfH = arenaHeight / 2f;

        if (Mathf.Abs(local.x) > (halfW - wallDetectionDistance))
        {
            correction.x = -Mathf.Sign(local.x);
        }

        if (Mathf.Abs(local.z) > (halfH - wallDetectionDistance))
        {
            correction.z = -Mathf.Sign(local.z);
        }

        if (correction != Vector3.zero)
        {
            correction = correction.normalized;
        }
        else
        {
            correction = transform.forward; // fallback: keep going
        }

        return correction;
    }

    bool IsNearWall()
    {
        float halfW = arenaWidth / 2f;
        float halfH = arenaHeight / 2f;
        Vector3 local = transform.position - arenaCenter.position;

        return Mathf.Abs(local.x) > (halfW - wallDetectionDistance) ||
               Mathf.Abs(local.z) > (halfH - wallDetectionDistance);
    }
    void PickRandomDirection()
    {
        Vector2 rand2D = Random.insideUnitCircle.normalized;
        targetDirection = new Vector3(rand2D.x, 0, rand2D.y);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Later: trigger explosion effect here

            Debug.Log($"{gameObject.name} was destroyed by the player!");
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }

}