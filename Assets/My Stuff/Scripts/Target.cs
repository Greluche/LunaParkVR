using UnityEngine;

public class Target : MonoBehaviour
{
    public int hits;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hits = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision){
        
        if(collision.gameObject.name == "Arrow(Clone)") {
            hits +=1;
        }

    }
}
