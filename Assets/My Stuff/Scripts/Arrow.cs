using UnityEngine;

using System.Collections.Generic;
public class Arrow : MonoBehaviour
{
    [SerializeField, Tooltip("Where to spawn the bullet")] 
    
    public GameObject Bow;
    public Rigidbody rb;
    private Quaternion rotation;
    public GrabBow bow_script;
    private Shoot_arrow s_a;
    public bool isFlying;
    public GameObject point;
    private GameObject p;
    private List<GameObject> trajectoryPoints;
    private int c = 0;
    public AudioSource source;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rotation = rb.rotation;
        bow_script = GetComponent<GrabBow>();
        s_a = Bow.GetComponent<Shoot_arrow>();
        trajectoryPoints = new List<GameObject>();
        

    }
    void Update(){
        if (isFlying)
        {

            p = UnityEngine.Object.Instantiate(point, transform);
            trajectoryPoints.Insert(c, p);
            c += 1;
            p.transform.parent = null;
            p.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        }
        else
        {
            for (int i = 0; i < trajectoryPoints.Count; i++)
            {
                Destroy(trajectoryPoints[i]);
            }
            c = 0;
            trajectoryPoints = new List<GameObject>();

        }

    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {   
        
        Debug.Log(collision.gameObject.name);


        if (collision.gameObject.name == "Bow")
        {

            rb.isKinematic = false;
        }
        else if (collision.gameObject.name == "midgrab")
        {
            if (!s_a.is_held)
            {
                rb.isKinematic = false;
            }
            else
            {
                rb.isKinematic = true;
            }
        }
        else if (collision.gameObject.name.Contains( "Origin") || collision.gameObject.name == "Main Camera")
        {
            rb.isKinematic = false;
        }
        else{
            rb.rotation = rotation;
            rb.position = new Vector3(collision.gameObject.transform.position.x, rb.position.y, rb.position.z);
            rb.isKinematic = true;
            source.Play();
            isFlying = false;
        }
           
         //rb.transform.position = collision.transform.position;
    }
    
}
