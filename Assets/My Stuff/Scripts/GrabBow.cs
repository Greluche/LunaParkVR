using UnityEngine;

public class GrabBow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isArrowGrabbed;
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    public void onGrab()
    {
        isArrowGrabbed = true;
        //quiver.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().activated = false;
        //quiver.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().enabled = true;


    }
    public void onDrop(){
        isArrowGrabbed = false;
    }
}
