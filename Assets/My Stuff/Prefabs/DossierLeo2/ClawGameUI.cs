using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events; // Add this for UnityEvents

public class ClawGameUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshPro instructionsText;
    public TextMeshPro winText;
    public TextMeshPro attemptsText;
    public TextMeshPro loseText;
    
    [Header("Game Settings")]
    public GameObject[] goldenTeddies; // Reference your 2 golden teddies
    public int maxAttempts = 5;
    
    [Header("References")]
    public ClawScript clawController;
    
    private int attemptCount = 0;
    private bool gameWon = false;
    private bool initialized = false;
    
    void Start()
    {
        // Setup UI
        if (winText != null)
            winText.gameObject.SetActive(false);
            
        if (loseText != null)
            loseText.gameObject.SetActive(false);
            
        if (attemptsText != null)
            attemptsText.text = $"Attempts: 0/{maxAttempts}";
        
        // Show instructions
        ShowInstructions();
        
        // Initialize connection to claw
        InitializeClawConnection();
    }
    
    void InitializeClawConnection()
    {
        if (clawController == null)
        {
            Debug.LogError("ClawGameUI: No ClawScript assigned! Please assign it in the Inspector.");
            // Try to find it automatically
            clawController = FindObjectOfType<ClawScript>();
            
            if (clawController == null)
            {
                Debug.LogError("ClawGameUI: Could not find ClawScript in scene. UI tracking won't work!");
                return;
            }
            else
            {
                Debug.Log("ClawGameUI: Found ClawScript automatically.");
            }
        }
        
        // Subscribe to event
        SubscribeToClawEvents();
        initialized = true;
    }
    
    void SubscribeToClawEvents()
    {
        // First unsubscribe to avoid duplicate callbacks
        UnsubscribeFromClawEvents();
        
        // Now subscribe
        if (clawController != null)
        {
            try
            {
                clawController.OnDropCompleted.AddListener(CheckForGoldenTeddy);
                Debug.Log("ClawGameUI: Successfully subscribed to OnDropCompleted event");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ClawGameUI: Error subscribing to OnDropCompleted event: {e.Message}");
            }
        }
    }
    
    void UnsubscribeFromClawEvents()
    {
        if (clawController != null)
        {
            try
            {
                clawController.OnDropCompleted.RemoveListener(CheckForGoldenTeddy);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"ClawGameUI: Error unsubscribing from events: {e.Message}");
            }
        }
    }
    
    void OnEnable()
    {
        if (initialized)
        {
            SubscribeToClawEvents();
        }
    }
    
    void OnDisable()
    {
        UnsubscribeFromClawEvents();
    }
    
    void ShowInstructions()
    {
        if (instructionsText != null)
        {
            instructionsText.text = "The claw game is simple. There are 2 golden teddys.\nIf you manage to get one in less than 5 tries, you win this game!";
            
            // Fade out instructions after delay
            StartCoroutine(FadeOutTextAfterDelay(instructionsText, 8.0f));
        }
        else
        {
            Debug.LogWarning("ClawGameUI: instructionsText is not assigned!");
        }
    }
    
    public void CheckForGoldenTeddy(GameObject grabbedToy)
    {
        Debug.Log($"ClawGameUI: CheckForGoldenTeddy called with toy: {(grabbedToy != null ? grabbedToy.name : "none")}");
        
        // Increment attempt counter
        attemptCount++;
        UpdateAttemptCounter();
        
        // If we already won, don't check again
        if (gameWon) 
        {
            Debug.Log("Already won, ignoring further grab attempts");
            return;
        }
        
        // Check if we got a golden teddy
        if (grabbedToy != null)
        {
            // Log the identities of all golden teddies for debugging
            Debug.Log($"Checking if grabbed toy matches any golden teddy. Grabbed toy ID: {grabbedToy.GetInstanceID()}");
            for (int i = 0; i < goldenTeddies.Length; i++)
            {
                GameObject goldenTeddy = goldenTeddies[i];
                if (goldenTeddy == null)
                {
                    Debug.LogWarning($"Golden teddy at index {i} is null!");
                    continue;
                }
                
                Debug.Log($"Golden teddy {i}: {goldenTeddy.name}, ID: {goldenTeddy.GetInstanceID()}");
                
                // Try both name matching and direct reference comparison
                bool nameMatch = goldenTeddy.name == grabbedToy.name;
                bool referenceMatch = grabbedToy == goldenTeddy;
                
                Debug.Log($"Comparison with {goldenTeddy.name}: Name match: {nameMatch}, Reference match: {referenceMatch}");
                
                // Try direct reference match first
                if (referenceMatch)
                {
                    // We got a golden teddy by reference!
                    Debug.Log("GOLDEN TEDDY GRABBED (reference match)! Showing win message!");
                    ShowWinMessage();
                    gameWon = true;
                    return;
                }
                // If reference doesn't match but names do, also count as a win
                else if (nameMatch)
                {
                    // We got a golden teddy by name!
                    Debug.Log("GOLDEN TEDDY GRABBED (name match)! Showing win message!");
                    ShowWinMessage();
                    gameWon = true;
                    return;
                }
            }
            
            Debug.Log("Grabbed toy did not match any golden teddy.");
        }
        else
        {
            Debug.Log("No toy was grabbed in this attempt");
        }
        
        // Check if game is over (out of attempts)
        if (attemptCount >= maxAttempts && !gameWon)
        {
            Debug.Log("Game over - maximum attempts reached without winning");
            ShowGameOverMessage();
        }
    }
    
    void UpdateAttemptCounter()
    {
        if (attemptsText != null)
        {
            attemptsText.text = $"Attempts: {attemptCount}/{maxAttempts}";
        }
    }
    
    void ShowWinMessage()
    {
        Debug.Log("ShowWinMessage called - activating win text");
        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            StartCoroutine(PulseText(winText));
            Debug.Log("Win text activated and pulse animation started");
        }
        else
        {
            Debug.LogError("Cannot show win message - winText is null!");
        }
    }
    
    void ShowGameOverMessage()
    {
        if (instructionsText != null)
        {
            instructionsText.gameObject.SetActive(true);
            instructionsText.text = "Game Over! You've used all 5 attempts.";
            instructionsText.color = Color.red;
        }
        
        // Show the lose message
        if (loseText != null)
        {
            loseText.gameObject.SetActive(true);
            loseText.text = "Sorry you lost. More luck next time!";
            loseText.color = Color.red;
            
            // Optional: Pulse the lose text similar to win text
            StartCoroutine(PulseText(loseText));
        }
        else
        {
            Debug.LogWarning("LoseText is not assigned!");
        }
    }
    
    IEnumerator FadeOutTextAfterDelay(TextMeshPro text, float delay)
    {
        if (text == null) yield break;
        
        yield return new WaitForSeconds(delay);
        
        float duration = 1.5f;
        float startTime = Time.time;
        Color startColor = text.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
        
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            text.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        text.color = endColor;
    }
    
    IEnumerator PulseText(TextMeshPro text)
    {
        if (text == null) yield break;
        
        float duration = 5.0f;
        float startTime = Time.time;
        Vector3 baseScale = text.transform.localScale;
        
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            float pulse = 1.0f + 0.2f * Mathf.Sin(t * 10f);
            text.transform.localScale = baseScale * pulse;
            
            // Also change color
            text.color = Color.Lerp(Color.yellow, Color.red, (Mathf.Sin(t * 8f) + 1) / 2);
            
            yield return null;
        }
        
        text.transform.localScale = baseScale;
        text.color = Color.yellow;
    }
}
