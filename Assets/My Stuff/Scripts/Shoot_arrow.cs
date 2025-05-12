using UnityEngine;

using UnityEngine.XR;
using System;
using System.Collections.Generic;

using UnityEngine.XR.Interaction.Toolkit;

using TMPro;

public class Shoot_arrow : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField, Tooltip("Where to spawn the bullet")] 
    private Transform muzzle;
    [SerializeField, Tooltip("The bullet to spawn")] 
    private GameObject bulletPrefab;
    [SerializeField, Tooltip("The force at which the bullet will be shot")] 
    private float force = 10f;
    public GameObject midpoint;
    public  Transform endpoint_1, endpoint_2;
    public GameObject _string;
    public Transform Arrow_position;
    public GameObject arrow;
    public GameObject r_a;
    public GrabBow bow_script;
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    public bool is_held;
    private LineRenderer lineRenderer;
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable midpointGrab;
    public void Start(){
        //access input devices, from : https://docs.unity3d.com/Manual/xr_input.html#AccessingInputDevices
        lineRenderer = _string.GetComponent<LineRenderer>();
        bow_script = arrow.GetComponent<GrabBow>();
        midpointGrab=  midpoint.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

    }
    public void Update(){
        if(midpointGrab.enabled == false){
               midpoint.transform.localPosition=   (endpoint_1.localPosition-endpoint_2.localPosition)/2 + endpoint_2.localPosition;

        }
        
        if(bow_script.isArrowGrabbed && !is_held  ){
             r_a =UnityEngine.Object.Instantiate(arrow,midpoint.transform.position ,transform.rotation, transform);
            r_a.transform.localScale= new Vector3(1.5f,1.5f ,1.5f );
            r_a.transform.rotation = transform.rotation * Quaternion.Euler(90,90,0);
            r_a.transform.localPosition = lineRenderer.GetPosition(1);
             rb =   r_a.GetComponent<Rigidbody>();
            
            
           bow_script.isArrowGrabbed = false;
            is_held = true;
            var k = r_a.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            k.enabled = false;
        }else if(is_held){
            r_a.transform.localPosition = lineRenderer.GetPosition(1);
             var  middlepoint = (endpoint_1.position-endpoint_2.position)/2 + endpoint_2.position;
            var handpos  =  transform.TransformPoint( lineRenderer.GetPosition(1));
            if(middlepoint!= handpos){
               
                  r_a.transform.rotation =Quaternion.LookRotation(   transform.forward  ,   (handpos-middlepoint).normalized) * Quaternion.Euler(-90,0,0);
               
            }

        }
    }

    /// <summary>
    /// Method <c>Shoot</c> shoots the bullet prefab at a 
    /// certain force, all specified by this <c>Pistol</c>.
    /// </summary>
    public void Shoot()
    {
        bow_script = arrow.GetComponent<GrabBow>(); 
        if(is_held){
                //var bullet = Instantiate(bulletPrefab, transform.position ,transform.rotation * Quaternion.Euler(0,270,90));
                 rb = r_a.GetComponent<Rigidbody>();
                
                rb.isKinematic = false;
                
                r_a.layer = 1;
                r_a.transform.parent = null;
                rb.isKinematic = false;
                var  middlepoint = (endpoint_1.position-endpoint_2.position)/2 + endpoint_2.position;
                var handpos  =  transform.TransformPoint( lineRenderer.GetPosition(1));
                rb?.AddForce((-handpos+middlepoint )*5*(force), ForceMode.Impulse);
                Debug.Log((handpos-middlepoint ).ToString());
                Debug.Log( (Quaternion.Euler(0,-90,0 ) * transform.forward).ToString());
                
                rb.useGravity = true;

                is_held = false;
                
               
                
        }
        
        
            

       
    
        
    }
    public void Destroy_Arrow(){

            if(is_held){
               
               rb.isKinematic= true;
               r_a.GetComponent<BoxCollider>().enabled = false;
            }
            midpointGrab.enabled = false;
        }
    public void onGrab(){
        midpointGrab.enabled = true;
        if(r_a!=null){

             r_a.GetComponent<BoxCollider>().enabled = true;
        }
    }
}
