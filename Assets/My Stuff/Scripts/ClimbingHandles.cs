using UnityEngine;

public class ClimbingHandles : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void isIn(){
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbInteractable>().enabled = true;
        
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().interactionLayers = 0;
        //GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
    }
}
