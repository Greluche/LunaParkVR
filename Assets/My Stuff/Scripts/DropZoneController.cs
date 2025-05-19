using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZoneController : MonoBehaviour
{
    [Header("Settings")]
    public float attractionForce = 2.0f; // Force to pull toys toward center
    public float stabilizationForce = 1.0f; // Force to stabilize toys
    public float activationDelay = 0.5f; // Delay before starting to attract toys
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    private List<Rigidbody> toysInZone = new List<Rigidbody>();
    private Dictionary<Rigidbody, float> entryTimes = new Dictionary<Rigidbody, float>();
    
    void OnTriggerEnter(Collider other)
    {
        // Check if it's a toy
        if (other.CompareTag("Toy"))
        {
            Rigidbody toyRb = other.GetComponent<Rigidbody>();
            if (toyRb != null && !toysInZone.Contains(toyRb))
            {
                toysInZone.Add(toyRb);
                entryTimes[toyRb] = Time.time;
                
                if (showDebugInfo)
                {
                    Debug.Log($"Toy entered drop zone: {other.gameObject.name}");
                }
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Check if it's a toy
        if (other.CompareTag("Toy"))
        {
            Rigidbody toyRb = other.GetComponent<Rigidbody>();
            if (toyRb != null)
            {
                toysInZone.Remove(toyRb);
                entryTimes.Remove(toyRb);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Toy left drop zone: {other.gameObject.name}");
                }
            }
        }
    }
    
    void FixedUpdate()
    {
        // Process each toy in the zone
        foreach (Rigidbody toyRb in toysInZone)
        {
            if (toyRb == null) continue;
            
            // Check if the toy has been in the zone long enough
            if (Time.time - entryTimes[toyRb] < activationDelay) continue;
            
            // Calculate direction to center
            Vector3 directionToCenter = transform.position - toyRb.position;
            directionToCenter.y = 0; // Only attract horizontally
            
            // Apply attraction force toward center if toy is not at center
            if (directionToCenter.magnitude > 0.05f)
            {
                toyRb.AddForce(directionToCenter.normalized * attractionForce, ForceMode.Force);
            }
            
            // Apply stabilization force to reduce velocity and rotation
            if (toyRb.linearVelocity.magnitude > 0.1f)
            {
                toyRb.AddForce(-toyRb.linearVelocity * stabilizationForce, ForceMode.Force);
            }
            
            // Dampen rotation
            toyRb.angularVelocity *= 0.95f;
        }
    }
    
    // Clean up any null references (toys that were destroyed)
    void CleanupLists()
    {
        toysInZone.RemoveAll(item => item == null);
        
        // Create a list of keys to remove
        List<Rigidbody> keysToRemove = new List<Rigidbody>();
        foreach (var entry in entryTimes)
        {
            if (entry.Key == null)
            {
                keysToRemove.Add(entry.Key);
            }
        }
        
        // Remove each key
        foreach (var key in keysToRemove)
        {
            entryTimes.Remove(key);
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw a semi-transparent green cube to visualize the drop zone
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        
        // Use the collider bounds if available, otherwise use transform
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
        }
        else
        {
            Gizmos.DrawCube(transform.position, transform.localScale);
        }
    }
} 