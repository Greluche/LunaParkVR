using UnityEngine;

public class BowIsGrabbed : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isBowGrabbed;
    void Start()
    {
        isBowGrabbed = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void onGrab(){
        isBowGrabbed = true;
       

    }
    public void onDrop(){
        isBowGrabbed = false;
    }
}
