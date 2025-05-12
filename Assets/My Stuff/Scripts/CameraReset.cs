using UnityEngine;

public class CameraReset : MonoBehaviour
{
    [Header("References")]
    public Transform xrRig;       // The XR Rig root object
    public Transform xrCamera;    // The camera inside the XR Rig (usually called "Main Camera")
    public Transform seatTransform; // The target position (player's head should end up here)

    void Start()
    {
        RecenterRig();
    }

    void RecenterRig()
    {
        if (xrRig == null || xrCamera == null || seatTransform == null)
        {
            Debug.LogWarning("XRRig recentering failed: missing references.");
            return;
        }

        // Get headset offset in local rig space
        Vector3 headsetLocalOffset = xrCamera.localPosition;

        // Move XR rig so that the headset aligns with the seat
        xrRig.position = seatTransform.position - headsetLocalOffset;

        // Optionally match rotation (facing same direction as seat)
        Vector3 seatForwardFlat = seatTransform.forward;
        seatForwardFlat.y = 0;
        if (seatForwardFlat != Vector3.zero)
        {
            xrRig.rotation = Quaternion.LookRotation(seatForwardFlat);
        }
    }
}
