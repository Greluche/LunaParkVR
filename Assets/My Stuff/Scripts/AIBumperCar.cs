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

    private BumperCarGameManager gameManager;
    private float directionTimer = 0f;
    private Vector3 targetDirection;

    void Start()
    {
        gameManager = FindObjectOfType<BumperCarGameManager>();
        PickRandomDirection();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        directionTimer -= dt;
        if (directionTimer <= 0f)
        {
            UpdateDirection();
        }

        // Rotate toward target
        Vector3 flatForward = transform.forward;
        Vector3 flatTarget = targetDirection;
        flatForward.y = 0;
        flatTarget.y = 0;

        Vector3 newDir = Vector3.RotateTowards(flatForward, flatTarget, turnSpeed * Mathf.Deg2Rad * dt, 0f);
        transform.rotation = Quaternion.LookRotation(newDir);

        // Move forward
        transform.Translate(Vector3.forward * moveSpeed * dt, Space.Self);
    }

    void UpdateDirection()
    {
        if (IsNearWall())
        {
            targetDirection = GetSmartWallAvoidanceDirection();
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) < fleeDistance)
        {
            Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 fleeDir = -toPlayer;

            // Figure out left vs right
            Vector3 left = Quaternion.Euler(0, -90f, 0) * transform.forward;
            Vector3 right = Quaternion.Euler(0, 90f, 0) * transform.forward;

            // Pick the side more perpendicular to the player vector
            float leftDot = Vector3.Dot(left, toPlayer);
            float rightDot = Vector3.Dot(right, toPlayer);

            Vector3 strafeDir = (leftDot < rightDot) ? left : right;

            // Blend between direct flee and safe strafe
            fleeDir = Vector3.Lerp(fleeDir, strafeDir, 0.7f).normalized;

            targetDirection = fleeDir;
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

        if (Physics.Raycast(transform.position, transform.forward, out hit, wallDetectionDistance))
        {
            Vector3 incoming = transform.forward;
            Vector3 reflect = Vector3.Reflect(incoming, hit.normal);
            reflect.y = 0;
            return reflect.normalized;
        }

        Vector3 local = transform.position - arenaCenter.position;
        Vector3 correction = Vector3.zero;

        float halfW = arenaWidth / 2f;
        float halfH = arenaHeight / 2f;

        if (Mathf.Abs(local.x) > (halfW - wallDetectionDistance))
            correction.x = -Mathf.Sign(local.x);

        if (Mathf.Abs(local.z) > (halfH - wallDetectionDistance))
            correction.z = -Mathf.Sign(local.z);

        return correction != Vector3.zero ? correction.normalized : transform.forward;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name} was destroyed by the player!");
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            if (gameManager != null)
            {
                gameManager.OnAICarDestroyed();
            }

            Destroy(gameObject);
        }
    }
}