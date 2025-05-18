using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FloatingDuck : MonoBehaviour
{
    public float angularSpeed = .3f;
    public float radius = 2f;
    public float shift = 0f;
    public Transform duckSpawnPoint;
    public AudioClip duckScream;
    public GameObject Hook;
    private DuckFishingGameManager gameManager;
    private XRController xr;
    private AudioSource quack;
    public GameObject childDuck;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        quack.Play(0);
        xr = (XRController)GameObject.FindObjectOfType(typeof(XRController));
    }

    // Update is called once per frame
    void Update()
    {
        /*
        // trajectory
        float x = Mathf.Cos((Time.time + shift) * angularSpeed) * radius;
        float z = Mathf.Sin((Time.time + shift) * angularSpeed) * radius;
        // illusion of floating
        float y = Mathf.PingPong(0.01f * (Time.time + shift), .05f);

        transform.position = new Vector3(x, y, z);
        transform.LookAt(duckSpawnPoint);
        */
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Jail"))
        {
            Debug.Log($"{gameObject.name} was captured!");
            if (gameManager != null)
            {
                gameManager.OnDuckJailed();
            }
            Instantiate(gameObject, Vector3.zero, Quaternion.Euler(0, 90, 0));

        }
    }
    */
    private void OnCollisionEnter(Collision collision)
    {   

        Debug.Log("ldasd");
        if (collision.gameObject.CompareTag("Hook"))
        {
            ///this.GetComponent<Rigidbody>().useGravity = true;
            transform.parent = Hook.transform;
            
            AudioSource.PlayClipAtPoint(duckScream, transform.position);
            angularSpeed = 0;
            //xr.SendHapticImpulse(0.7f, 2f);
        }
    }
}
