using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform xrRig;     // The XR Rig root
    public Transform xrCamera;  // The Main Camera inside the rig

    void Start()
    {
        Vector3 cameraOffset = xrCamera.localPosition;
        xrRig.position = SpawnPointManager.hubSpawnPosition - cameraOffset;

        Vector3 flatForward = SpawnPointManager.hubSpawnRotation * Vector3.forward;
        flatForward.y = 0;

        if (flatForward != Vector3.zero)
        {
            xrRig.rotation = Quaternion.LookRotation(flatForward);
        }
    }
}