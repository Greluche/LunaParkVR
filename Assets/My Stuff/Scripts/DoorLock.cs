using UnityEngine;

public class DoorLock : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isLocked;
    public GameObject handle;
    void Start()
    {
        isLocked = true;

    }

    // Update is called once per frame
    void Update()
    {
        if(isLocked){
            handle.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = false;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        }else{
            handle.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;
         
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

        }
    }
    public void lockUnlock(){
        isLocked = !isLocked;
        Debug.Log(isLocked.ToString());
    }
}
