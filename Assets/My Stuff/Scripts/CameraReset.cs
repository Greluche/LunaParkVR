using UnityEngine;
using System.Collections;
using Unity.XR.CoreUtils;
public class CameraReset : MonoBehaviour
{
    [Header("References")]
    public Transform seatTransform; // The target position (player's head should end up here)

    void Start()
    {
        StartCoroutine(DelayedRecenter());
    }

    private IEnumerator DelayedRecenter()
    {
        yield return null; // wait one frame for XR tracking to initialize

        RecenterRig();
    }

    void RecenterRig()
    {
        XROrigin xrOrigin = GetComponent<XROrigin>();
        xrOrigin.MoveCameraToWorldLocation(seatTransform.position);
        xrOrigin.MatchOriginUpCameraForward(seatTransform.up, seatTransform.forward);
    }
}
