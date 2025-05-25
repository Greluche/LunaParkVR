using UnityEngine;

using UnityEngine.XR;
using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit;

using TMPro;

public class Shoot_arrow : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private GameObject bulletPrefab;
    [SerializeField, Tooltip("The force at which the bullet will be shot")]
    private float force = 10f;
    public GameObject midpoint;
    public Transform endpoint_1, endpoint_2;
    public GameObject _string;
    public Transform Arrow_position;
    public GameObject arrow;
    
    public GameObject r_a;
    public GrabBow bow_script;
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    public bool is_held;
    public GameObject quiver;
    private LineRenderer lineRenderer;
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable midpointGrab;
    [Header("Haptics")]
    public HapticImpulsePlayer leftHaptics;
    public HapticImpulsePlayer rightHaptics;
    public Vector3 sh = Vector3.zero;
    public bool isFlying;

    public void Start()
    {

        lineRenderer = _string.GetComponent<LineRenderer>();
        bow_script = quiver.GetComponent<GrabBow>();
        midpointGrab = midpoint.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

    }
    public void Update()
    {
        bow_script = quiver.GetComponent<GrabBow>();
        if (midpointGrab.enabled == false)
        {
            midpoint.transform.localPosition = (endpoint_1.localPosition - endpoint_2.localPosition) / 2 + endpoint_2.localPosition;

        }

        if (bow_script.isArrowGrabbed && !is_held)
        {
            Debug.Log(bow_script.isArrowGrabbed.ToString());
            r_a = UnityEngine.Object.Instantiate(arrow, midpoint.transform.position, transform.rotation, transform);
            r_a.transform.localScale = new Vector3(3.5f, 3.5f, 3.5f);
            r_a.transform.rotation = transform.rotation * Quaternion.Euler(0, 90, 90);
            r_a.transform.localPosition = lineRenderer.GetPosition(1);
            rb = r_a.GetComponent<Rigidbody>();


            bow_script.isArrowGrabbed = false;
            is_held = true;
            var k = r_a.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            k.enabled = false;
        }
        else if (is_held)
        {
            r_a.transform.localPosition = new Vector3(lineRenderer.GetPosition(1).x, lineRenderer.GetPosition(1).y - 0.2f, lineRenderer.GetPosition(1).z);
            var middlepoint = (endpoint_1.position - endpoint_2.position) / 2 + endpoint_2.position;
            var handpos = transform.TransformPoint(lineRenderer.GetPosition(1));
            if (middlepoint != handpos)
            {

                r_a.transform.rotation = Quaternion.LookRotation(transform.forward, (handpos - middlepoint).normalized) * Quaternion.Euler(0, 90, 90);
                if (rightHaptics != null && ((handpos - middlepoint).magnitude > 0.1 || (-handpos + middlepoint).magnitude > 0.1))
                {
                    rightHaptics.SendHapticImpulse(Math.Max((handpos - middlepoint).magnitude, (-handpos + middlepoint).magnitude), 0.05f);

                }
                sh = (handpos - middlepoint);
                
                
            }
            
            

        }
    }

    
    public void Shoot()
    {
        bow_script = arrow.GetComponent<GrabBow>();
        if (is_held)
        {
            rb = r_a.GetComponent<Rigidbody>();

            rb.isKinematic = false;

            r_a.layer = 1;
            r_a.transform.parent = null;
            rb.isKinematic = false;
            var middlepoint = (endpoint_1.position - endpoint_2.position) / 2 + endpoint_2.position;
            var handpos = transform.TransformPoint(lineRenderer.GetPosition(1));
            rb?.AddForce((-handpos + middlepoint) * 5 * (force), ForceMode.Impulse);
            r_a.transform.localScale = new Vector3(0.45f, 0.17f, 0.13f);
            rb.useGravity = true;
            r_a.GetComponent<Arrow>().isFlying = true;
            is_held = false;



        }







    }
    public void Destroy_Arrow()
    {

        if (is_held)
        {

            rb.isKinematic = true;
            r_a.GetComponent<BoxCollider>().enabled = false;
        }
        midpointGrab.enabled = false;
    }
    public void onGrab()
    {
        midpointGrab.enabled = true;
        if (r_a != null)
        {

            r_a.GetComponent<BoxCollider>().enabled = true;
        }
    }
    private void SendHaptic(XRBaseController controller, float amplitude, float duration = 0.05f)
    {
        if (controller != null)
        {
            controller.SendHapticImpulse(amplitude, duration);
        }
    }
}
