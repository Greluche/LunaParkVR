using UnityEngine;

public class HubPlayerSpawner : MonoBehaviour
{
    public Transform xrRig;
    public Transform xrCamera;

    void Start()
    {
        Vector3 offset = xrCamera.localPosition;
        xrRig.position = SpawnPointManager.hubSpawnPosition - offset;

        Vector3 forward = SpawnPointManager.hubSpawnRotation * Vector3.forward;
        forward.y = 0;
        xrRig.rotation = Quaternion.LookRotation(forward);
    }
}
