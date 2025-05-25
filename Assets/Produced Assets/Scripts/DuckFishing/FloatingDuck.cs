using UnityEngine;
using UnityEngine.XR;

public class FloatingDuck : MonoBehaviour
{
    public ParticleSystem explosionAnimation;
    public int pointsWorth = 1;
    public float angularSpeed = .2f;
    public float radius = 2f;
    public float shift = 0f;
    public Transform duckSpawnPoint;
    public AudioClip duckScream;
    public GameObject Hook;
    public AudioSource explosionSound;

    private DuckFishingGameManager gameManager;
    private XRNode controllerNode;
    private InputDevice device;
    private bool captured = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //#TODO: detect which hand holds the fishing rod
        controllerNode = XRNode.RightHand;
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
        gameManager = FindObjectOfType<DuckFishingGameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!captured)
        {
            // trajectory
            float x = Mathf.Cos((Time.time + shift) * angularSpeed) * radius;
            float z = Mathf.Sin((Time.time + shift) * angularSpeed) * radius;
            // illusion of floating
            float y = Mathf.PingPong(0.1f * (Time.time + shift), .05f);

            transform.position = new Vector3(x, y, z);
            transform.LookAt(duckSpawnPoint);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Jail"))
        {
            if (explosionAnimation != null)
                explosionAnimation.Play();
            Destroy(gameObject, 1);
            if (explosionSound != null)
                explosionSound.Play();

            if (gameManager != null)
                gameManager.OnDuckJailed(pointsWorth);
        }
    }
   
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hook") && !captured)
        {
            if (gameManager != null)
                gameManager.OnDuckGrabbed();

            ///this.GetComponent<Rigidbody>().useGravity = true;
            transform.parent = Hook.transform;

            AudioSource.PlayClipAtPoint(duckScream, transform.position);
            device.SendHapticImpulse(0, 0.7f, 2f);

            captured = true;
        }
    }
}
