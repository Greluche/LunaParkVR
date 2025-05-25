using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// This script should be added alongside a standard XR Interactable component
public class ClawButton : MonoBehaviour
{
    [Header("References")]
    public ClawScript clawController;
    public Transform buttonVisual;
    
    [Header("Settings")]
    public float pressDistance = 0.05f;
    public float returnSpeed = 5f;
    public float autoReleaseTime = 0.2f; // Time after which the button auto-releases
    
    [Header("Debug")]
    public KeyCode testKey = KeyCode.Space; // Add keyboard shortcut for testing
    public bool debugMode = true;
    
    private Vector3 initialButtonPos;
    private bool isPressed = false;
    private MonoBehaviour xrInteractable;
    private float pressTime = 0f;
    
    void Awake()
    {
        // Find any XR interactable component
        xrInteractable = GetComponent<MonoBehaviour>();
        
        if (buttonVisual != null)
        {
            initialButtonPos = buttonVisual.localPosition;
        }
        
        // Debug check to ensure claw controller is assigned
        if (clawController == null)
        {
            Debug.LogError("ClawButton: No ClawScript assigned! Button won't work properly.");
            
            // try to find ClawScript 
            clawController = FindObjectOfType<ClawScript>();
            if (clawController != null)
            {
                Debug.Log("ClawButton: Found ClawScript in scene and assigned it automatically.");
            }
        }
        
        // Configure for poke-only in editor
        #if UNITY_EDITOR
        ConfigureForPokeOnly();
        #endif
    }
    
    // Configure the interactable to only respond to poke interactions
    private void ConfigureForPokeOnly()
    {
        // Try to disable any grab interactable if it exists
        var grabComponents = GetComponents<MonoBehaviour>();
        foreach (var component in grabComponents)
        {
            string typeName = component.GetType().Name;
            if (typeName.Contains("GrabInteractable"))
            {
                component.enabled = false;
                Debug.Log($"Disabled {typeName} on button");
            }
        }
        
        // Make sure there's a poke filter
        bool hasPokeFilter = false;
        var filterComponents = GetComponents<MonoBehaviour>();
        foreach (var component in filterComponents)
        {
            if (component.GetType().Name.Contains("PokeFilter"))
            {
                hasPokeFilter = true;
                break;
            }
        }
        
        if (!hasPokeFilter)
        {
            Debug.LogWarning("No PokeFilter found on button. Please add an XR Poke Filter component in the inspector.");
        }
        
        Debug.Log("Button configured for poke-only interaction");
    }
    
    void OnEnable()
    {
        // Find the selectEntered event using reflection to avoid namespace issues
        if (xrInteractable != null)
        {
            var type = xrInteractable.GetType();
            
            // Try to find the events
            var selectEnteredField = type.GetField("selectEntered");
            var selectExitedField = type.GetField("selectExited");
            
            if (selectEnteredField != null && selectExitedField != null)
            {
                var selectEntered = selectEnteredField.GetValue(xrInteractable) as UnityEngine.Events.UnityEvent<SelectEnterEventArgs>;
                var selectExited = selectExitedField.GetValue(xrInteractable) as UnityEngine.Events.UnityEvent<SelectExitEventArgs>;
                
                if (selectEntered != null && selectExited != null)
                {
                    selectEntered.AddListener(OnSelectEntered);
                    selectExited.AddListener(OnSelectExited);
                    Debug.Log("ClawButton: Successfully registered XR Interactable events");
                }
                else
                {
                    Debug.LogError("ClawButton: Could not access select events!");
                }
            }
            else
            {
                Debug.LogError("ClawButton: Could not find select event fields!");
            }
        }
        
        // Alternative direct approach 
        var components = GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            string typeName = component.GetType().Name;
            if (typeName.Contains("Interactable"))
            {
                Debug.Log($"Found interactable component: {typeName}");
                
                // Try to add listeners using SendMessage pattern as a fallback
                gameObject.AddComponent<ButtonEventForwarder>();
            }
        }
    }
    
    void OnDisable()
    {
        // Unregister events using reflection
        if (xrInteractable != null)
        {
            var type = xrInteractable.GetType();
            
            var selectEnteredField = type.GetField("selectEntered");
            var selectExitedField = type.GetField("selectExited");
            
            if (selectEnteredField != null && selectExitedField != null)
            {
                var selectEntered = selectEnteredField.GetValue(xrInteractable) as UnityEngine.Events.UnityEvent<SelectEnterEventArgs>;
                var selectExited = selectExitedField.GetValue(xrInteractable) as UnityEngine.Events.UnityEvent<SelectExitEventArgs>;
                
                if (selectEntered != null && selectExited != null)
                {
                    selectEntered.RemoveListener(OnSelectEntered);
                    selectExited.RemoveListener(OnSelectExited);
                }
            }
        }
    }
    
    // This can be called directly from Unity events in the inspector
    public void OnButtonPressed()
    {
        Debug.Log("ClawButton: OnButtonPressed called from Unity event");
        PressButton();
        
        // Visual feedback - depress the button
        if (buttonVisual != null)
        {
            buttonVisual.localPosition = initialButtonPos + Vector3.down * pressDistance;
        }
    }
    
    // This can be called directly from Unity events in the inspector
    public void OnButtonReleased()
    {
        Debug.Log("ClawButton: OnButtonReleased called from Unity event");
        ReleaseButton();
    }
    
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Check if this is a poke interaction
        bool isPoke = args.interactorObject.transform.name.Contains("Poke") || 
                      args.interactorObject.GetType().Name.Contains("Poke");
        
        Debug.Log($"Button selected by {args.interactorObject.transform.name}, isPoke: {isPoke}");
        
        // When the button is "selected" (poked/pressed)
        PressButton();
        
        // Visual feedback - depress the button
        if (buttonVisual != null)
        {
            buttonVisual.localPosition = initialButtonPos + Vector3.down * pressDistance;
        }
        
        // Store press time for auto-release
        pressTime = Time.time;
    }
    
    private void OnSelectExited(SelectExitEventArgs args)
    {
        // When the button is released
        Debug.Log("Button released via SelectExited event");
        ReleaseButton();
    }
    
    // method calling the claw drop function
    private void PressButton()
    {
        if (isPressed) return; // Prevent multiple activations

        isPressed = true;

        // Trigger claw drop
        if (clawController != null)
        {
            clawController.Drop();

        }
        else
        {

            // Try to find it again
            clawController = FindObjectOfType<ClawScript>();
            if (clawController != null)
            {
                Debug.Log("Found ClawScript and calling Drop()");
                clawController.Drop();
            }
        }

        // Automatically release the button after a short delay
        Invoke("ReleaseButton", autoReleaseTime);
    }
    
    private void ReleaseButton()
    {
        isPressed = false;
    }
    
    void Update()
    {
        // Smoothly return button to initial position if not pressed
        if (!isPressed && buttonVisual != null)
        {
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, initialButtonPos, Time.deltaTime * returnSpeed);
        }
        
        // Auto-release if button has been pressed for too long
        // This helps when the controller moves back but doesn't trigger the exit event
        if (isPressed && Time.time - pressTime > autoReleaseTime)
        {
            ReleaseButton();
        }
        
        // Test button with keyboard shortcut (for debugging)
        if (Input.GetKeyDown(testKey))
        {
            PressButton();
            
            // Visual feedback
            if (buttonVisual != null)
            {
                buttonVisual.localPosition = initialButtonPos + Vector3.down * pressDistance;
            }
        }
    }
    
    // Draw gizmos to make the button more visible in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        Gizmos.DrawRay(transform.position, transform.forward * 0.1f);
    }
}

// Helper class mainly for debug issues
public class ButtonEventForwarder : MonoBehaviour
{
    private ClawButton parentButton;
    
    void Start()
    {
        parentButton = GetComponent<ClawButton>();
    }
    
    // called when the interactable is selected
    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("buttonr: OnSelectEntered called");
        if (parentButton != null)
        {
            parentButton.OnButtonPressed();
        }
    }
    
    // called when the button is not selected anymore
    public void OnSelectExited(SelectExitEventArgs args)
    {
        Debug.Log("button : OnSelectExited called");
        if (parentButton != null)
        {
            parentButton.OnButtonReleased();
        }
    }
} 