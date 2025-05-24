using UnityEngine;
using UnityEngine.InputSystem;

public class MachineBoundsHelper : MonoBehaviour
{
    [Header("References")]
    public ClawScript clawController;
    
    [Header("Visualization")]
    public bool showBounds = true;
    public Color boundsColor = new Color(1f, 0.5f, 0f, 0.3f);
    
    private Vector3 boundsMin;
    private Vector3 boundsMax;
    
    void Start()
    {
        // Find the claw controller if not assigned
        if (clawController == null)
        {
            clawController = FindFirstObjectByType<ClawScript>();
            if (clawController == null)
            {
                Debug.LogError("MachineBoundsHelper: No ClawScript found in the scene!");
                return;
            }
        }
        
        // Calculate bounds from the ClawScript settings
        CalculateBounds();
        
        // Log bounds for debugging
        Debug.Log($"Machine bounds initialized: Min={boundsMin}, Max={boundsMax}");
    }
    
    private void CalculateBounds()
    {
        if (clawController == null) return;
        
        // Get the machine transform
        Transform machineTransform = clawController.transform.parent;
        
        // Get bounds from ClawScript
        Vector2 boundsX = clawController.machineBoundsX;
        Vector2 boundsZ = clawController.machineBoundsZ;
        float machineHeight = clawController.machineHeight;
        
        if (machineTransform != null && clawController.useLocalCoordinates)
        {
            // Convert local bounds to world space
            Vector3 worldMinLocal = new Vector3(boundsX.x, -0.5f, boundsZ.x);
            Vector3 worldMaxLocal = new Vector3(boundsX.y, machineHeight, boundsZ.y);
            
            // Convert corners to world space
            Vector3 worldMin = machineTransform.TransformPoint(worldMinLocal);
            Vector3 worldMax = machineTransform.TransformPoint(worldMaxLocal);
            
            // Ensure min is actually min and max is actually max after transformation
            boundsMin = new Vector3(
                Mathf.Min(worldMin.x, worldMax.x),
                Mathf.Min(worldMin.y, worldMax.y),
                Mathf.Min(worldMin.z, worldMax.z)
            );
            
            boundsMax = new Vector3(
                Mathf.Max(worldMin.x, worldMax.x),
                Mathf.Max(worldMin.y, worldMax.y),
                Mathf.Max(worldMin.z, worldMax.z)
            );
        }
        else
        {
            // Simple world space bounds
            boundsMin = new Vector3(boundsX.x, -0.5f, boundsZ.x);
            boundsMax = new Vector3(boundsX.y, machineHeight, boundsZ.y);
        }
    }
    
    // Public methods to access bounds
    public Vector3 GetBoundsMin()
    {
        return boundsMin;
    }
    
    public Vector3 GetBoundsMax()
    {
        return boundsMax;
    }
    
    public Vector3 GetBoundsSize()
    {
        return boundsMax - boundsMin;
    }
    
    public Vector3 GetBoundsCenter()
    {
        return (boundsMin + boundsMax) * 0.5f;
    }
    
    public bool IsPointInBounds(Vector3 point)
    {
        return (point.x >= boundsMin.x && point.x <= boundsMax.x &&
                point.y >= boundsMin.y && point.y <= boundsMax.y &&
                point.z >= boundsMin.z && point.z <= boundsMax.z);
    }
    
    // Draw the bounds as a wireframe cube
    void OnDrawGizmos()
    {
        if (showBounds && clawController != null)
        {
            // Recalculate bounds for gizmo drawing
            CalculateBounds();
            
            // Store original gizmo color
            Color originalColor = Gizmos.color;
            
            // Set the color for our bounds
            Gizmos.color = boundsColor;
            
            // Draw a cube representing the bounds
            Vector3 center = GetBoundsCenter();
            Vector3 size = GetBoundsSize();
            Gizmos.DrawCube(center, size);
            
            // Draw wireframe in a slightly different color
            Gizmos.color = new Color(boundsColor.r, boundsColor.g, boundsColor.b, boundsColor.a + 0.2f);
            Gizmos.DrawWireCube(center, size);
            
            // Restore original gizmo color
            Gizmos.color = originalColor;
        }
    }
} 