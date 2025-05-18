using UnityEngine;
using System;
public class Arrow : MonoBehaviour
{
    [SerializeField, Tooltip("Where to spawn the bullet")] 
    
    public GameObject Bow;
    public Rigidbody rb;
    private Quaternion rotation;
    public GrabBow bow_script;
    private Shoot_arrow s_a;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rotation = rb.rotation;
        bow_script = GetComponent<GrabBow>();
        s_a = Bow.GetComponent<Shoot_arrow>();

    }
    void Update(){
        

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
        else
        {
            rb.rotation = rotation;
            rb.position = new  Vector3(collision.gameObject.transform.position.x,rb.position.y,rb.position.z);
            rb.isKinematic = true; 
        }
           
         //rb.transform.position = collision.transform.position;
    }
    
}
