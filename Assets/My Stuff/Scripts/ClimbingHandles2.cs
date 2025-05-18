using UnityEngine;

public class ClimbingHandles2 : MonoBehaviour
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

        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = false;
        //GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
    }
}
