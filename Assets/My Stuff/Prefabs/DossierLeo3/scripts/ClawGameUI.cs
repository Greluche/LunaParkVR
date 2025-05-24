using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events; // Add this for UnityEvents
using UnityEngine.XR.Interaction.Toolkit; // Add this for SelectEnterEventArgs
using UnityEngine.UI; // Add this for Button

public class ClawGameUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionsCanvas;  // Reference to the entire Canvas
    public TextMeshProUGUI instructionsText; // Reference to the TextMeshProUGUI on the Canvas
    public TextMeshPro winText;
    public TextMeshPro attemptsText;
    public TextMeshPro loseText;
    
    [Header("Play Again UI")]
    [Tooltip("Canvas containing the play again prompt")]
    public GameObject playAgainCanvas;
    
    [Tooltip("Text component for the play again question")]
    public TextMeshProUGUI playAgainText;
    
    [Tooltip("Button to click if player wants to play again")]
    public Button yesButton;
    
    [Tooltip("Button to click if player doesn't want to play again")]
    public Button noButton;
    
    [Header("Game Settings")]
    public GameObject[] goldenTeddies; // Reference your 2 golden teddies
    public int maxAttempts = 5;
    [Tooltip("Number of attempts that triggers the warning sound")]
    public int warningAttemptThreshold = 3;
    
    [Header("Toy Reset")]
    [Tooltip("All toys with ToyController component will be reset when game ends")]
    public bool resetToysOnGameOver = true;
    
    [Tooltip("Delay before resetting toys when game ends (seconds)")]
    [Range(0f, 5f)]
    public float toyResetDelay = 2f;
    
    [Tooltip("Container that holds all the toys (optional)")]
    public Transform toyContainer;
    
    [Header("UI Display Settings")]
    [Tooltip("How long to display the win message before fading out (seconds)")]
    [Range(1f, 10f)]
    public float winMessageDisplayTime = 10f;
    
    [Tooltip("How long to display the game over message before fading out (seconds)")]
    [Range(1f, 10f)]
    public float gameOverDisplayTime = 8f;
    
    [Tooltip("Font asset to use for the win message")]
    public TMP_FontAsset winMessageFont;
    
    [Header("References")]
    public ClawScript clawController;
    public XRJoystickController joystickController;
    
    [Header("Audio")]
    [Tooltip("Sound played when player reaches the warning attempt threshold")]
    public AudioClip warningSound;
    [Tooltip("Sound played when player wins")]
    public AudioClip winSound;
    [Tooltip("Sound played when player loses")]
    public AudioClip loseSound;
    [Tooltip("Sound played when showing the play again prompt")]
    public AudioClip promptSound;
    
    private int attemptCount = 0;
    private bool gameWon = false;
    private bool initialized = false;
    private bool hasShownAttempts = false;
    private AudioSource audioSource;
    
    // Add debounce variables to prevent double counting
    private bool isProcessingAttempt = false;
    private float attemptDebounceTime = 1.0f; // Minimum time between counting attempts
    private float lastAttemptTime = 0f;
    
    void Start()
    {
        // Setup UI
        if (winText != null)
            winText.gameObject.SetActive(false);
            
        if (loseText != null)
            loseText.gameObject.SetActive(false);
            
        if (attemptsText != null) {
            attemptsText.text = $"Attempts: 0/{maxAttempts}";
            // Hide attempts counter initially
            attemptsText.gameObject.SetActive(false);
        }
        
        // Apply custom font to win message if specified
        if (winText != null && winMessageFont != null)
        {
            winText.font = winMessageFont;
        }
        
        // Hide play again UI initially
        if (playAgainCanvas != null)
        {
            playAgainCanvas.SetActive(false);
        }
        
        // Set up button listeners
        SetupPlayAgainButtons();
        
        // Show instructions
        ShowInstructions();
        
        // Initialize connection to claw
        InitializeClawConnection();
        
        // Initialize joystick connection
        InitializeJoystickConnection();
        
        // Setup audio source
        SetupAudioSource();
    }
    
    void SetupPlayAgainButtons()
    {
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnPlayAgainYesClicked);
        }
        else
        {
            Debug.LogError("Yes button not assigned to ClawGameUI!");
        }
        
        if (noButton != null)
        {
            noButton.onClick.AddListener(OnPlayAgainNoClicked);
        }
        else
        {
            Debug.LogError("No button not assigned to ClawGameUI!");
        }
    }
    
    void OnPlayAgainYesClicked()
    {
        // Hide the play again canvas
        if (playAgainCanvas != null)
        {
            playAgainCanvas.SetActive(false);
        }
        
        // Reset toys
        if (resetToysOnGameOver)
        {
            StartCoroutine(ResetAllToysAfterDelay(0.5f));
        }
        
        // Reset game state
        ResetGameState();
        
        Debug.Log("Player chose to play again - game reset");
    }
    
    void OnPlayAgainNoClicked()
    {
        // Hide the play again canvas
        if (playAgainCanvas != null)
        {
            playAgainCanvas.SetActive(false);
        }
        
        // Hide all UI elements
        if (attemptsText != null)
        {
            attemptsText.gameObject.SetActive(false);
        }
        
        if (instructionsCanvas != null)
        {
            instructionsCanvas.SetActive(false);
        }
        
        Debug.Log("Player chose not to play again");
    }
    
    void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (warningSound != null || winSound != null || loseSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
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
    
    void InitializeJoystickConnection()
    {
        if (joystickController == null)
        {
            // Try to find it automatically
            joystickController = FindObjectOfType<XRJoystickController>();
            
            if (joystickController == null)
            {
                Debug.LogWarning("ClawGameUI: Could not find XRJoystickController. Attempts counter won't show on joystick grab.");
            }
            else
            {
                Debug.Log("ClawGameUI: Found XRJoystickController automatically.");
                // Subscribe to joystick events
                joystickController.selectEntered.AddListener(OnJoystickGrabbed);
            }
        }
        else
        {
            // Subscribe to joystick events
            joystickController.selectEntered.AddListener(OnJoystickGrabbed);
        }
    }
    
    // Method signature matching the XRGrabInteractable.selectEntered event
    void OnJoystickGrabbed(SelectEnterEventArgs args)
    {
        // Show attempts counter when joystick is first grabbed
        if (!hasShownAttempts && attemptsText != null)
        {
            attemptsText.gameObject.SetActive(true);
            hasShownAttempts = true;
            
            // Update the attempts text to show 0 attempts
            attemptsText.text = $"Attempts: 0/{maxAttempts}";
            
            // Fade out instructions when showing attempts (reduce delay to 0.5 seconds)
            if (instructionsCanvas != null && instructionsCanvas.activeSelf)
            {
                StartCoroutine(FadeOutCanvasAfterDelay(instructionsCanvas, 0.2f));
                Debug.Log("Fading out instructions canvas");
            }
            
            Debug.Log("Joystick grabbed: Showing attempts counter");
        }
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
                clawController.OnDropCompleted.RemoveAllListeners(); // Clear any existing listeners to prevent duplicates
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
                clawController.OnDropCompleted.RemoveAllListeners();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"ClawGameUI: Error unsubscribing from events: {e.Message}");
            }
        }
        
        if (joystickController != null)
        {
            joystickController.selectEntered.RemoveListener(OnJoystickGrabbed);
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
        if (instructionsCanvas != null && instructionsText != null)
        {
            instructionsText.text = "The claw game is simple. There are 2 golden teddys.\nIf you manage to get one in less than 5 tries, you win this game!";
            
            // Show the canvas instead of the text object
            instructionsCanvas.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ClawGameUI: instructionsCanvas or instructionsText is not assigned!");
        }
    }
    
    public void CheckForGoldenTeddy(GameObject grabbedToy)
    {
        Debug.Log($"ClawGameUI: CheckForGoldenTeddy called with toy: {(grabbedToy != null ? grabbedToy.name : "none")}");
        
        // Skip if game is won
        if (gameWon) 
        {
            Debug.Log("Already won, ignoring further grab attempts");
            return;
        }
        
        // Debounce check to prevent multiple counts in quick succession
        if (isProcessingAttempt || Time.time - lastAttemptTime < attemptDebounceTime)
        {
            Debug.Log("Attempt already being processed or debounce time not elapsed, ignoring this call");
            return;
        }
        
        // Set debounce flag
        isProcessingAttempt = true;
        lastAttemptTime = Time.time;
        
        // Only increment attempt counter if we've shown the attempts counter already
        // This prevents double counting the first attempt
        if (hasShownAttempts)
        {
            // Increment counter
            attemptCount++;
            
            // Log for debugging
            Debug.Log($"Attempt count incremented to: {attemptCount}");
            
            // Update UI
            UpdateAttemptCounter();
            
            // Check if we've reached the warning threshold
            if (attemptCount == warningAttemptThreshold)
            {
                PlayWarningSound();
            }
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
        
        // Reset debounce flag after a short delay to ensure we don't count multiple events
        StartCoroutine(ResetProcessingFlag());
    }
    
    private IEnumerator ResetProcessingFlag()
    {
        // Wait for debounce time to elapse
        yield return new WaitForSeconds(attemptDebounceTime);
        
        // Reset the flag
        isProcessingAttempt = false;
        Debug.Log("Attempt processing flag reset, ready for next attempt");
    }
    
    void UpdateAttemptCounter()
    {
        if (attemptsText != null)
        {
            attemptsText.text = $"Attempts: {attemptCount}/{maxAttempts}";
            
            // Make sure attempts counter is visible
            if (!attemptsText.gameObject.activeSelf)
            {
                attemptsText.gameObject.SetActive(true);
            }
            
            // Pulse the text to draw attention
            StartCoroutine(PulseText(attemptsText, 1.0f, 1.2f, 0.5f));
        }
    }
    
    void PlayWarningSound()
    {
        if (audioSource != null && warningSound != null)
        {
            audioSource.clip = warningSound;
            audioSource.Play();
            Debug.Log("Playing warning sound at attempt threshold");
            
            // Make the attempts counter flash red
            if (attemptsText != null)
            {
                StartCoroutine(FlashText(attemptsText, Color.red, 3));
            }
        }
    }
    
    void ShowWinMessage()
    {
        Debug.Log("ShowWinMessage called - activating win text");
        if (winText != null)
        {
            // Reset any previous animations
            StopAllCoroutines();
            
            // Reset scale and position
            winText.transform.localScale = Vector3.one;
            
            // Reset color and alpha
            Color textColor = winText.color;
            winText.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
            
            // Show the text
            winText.gameObject.SetActive(true);
            
            // Play a single pulse animation, not continuous
            StartCoroutine(SinglePulseText(winText, 0.8f, 1.5f, 0.7f));
            
            // Fade out after delay
            StartCoroutine(FadeOutTextAfterDelay(winText, winMessageDisplayTime));
            
            Debug.Log($"Win text activated and will disappear after {winMessageDisplayTime} seconds");
        }
        else
        {
            Debug.LogError("Cannot show win message - winText is null!");
        }
        
        // Play win sound
        if (audioSource != null && winSound != null)
        {
            audioSource.clip = winSound;
            audioSource.Play();
        }
        
        // Hide attempts counter after a short delay
        if (attemptsText != null)
        {
            StartCoroutine(FadeOutTextAfterDelay(attemptsText, 3f));
        }
        
        // Show play again prompt after win message fades
        StartCoroutine(ShowPlayAgainPromptAfterDelay(winMessageDisplayTime + 1f));
    }
    
    void ShowGameOverMessage()
    {
        // Reset any previous animations
        StopAllCoroutines();
        
        if (instructionsText != null)
        {
            instructionsText.text = "Game Over! You've used all 5 attempts.";
            instructionsText.color = Color.red;
            instructionsCanvas.SetActive(true);
            
            // Fade out after delay
            StartCoroutine(FadeOutCanvasAfterDelay(instructionsCanvas, gameOverDisplayTime));
        }
        
        // Show the lose message
        if (loseText != null)
        {
            loseText.gameObject.SetActive(true);
            loseText.text = "Sorry you lost. More luck next time!";
            loseText.color = Color.red;
            
            // Single pulse then fade out
            StartCoroutine(SinglePulseText(loseText, 0.9f, 1.3f, 0.6f));
            StartCoroutine(FadeOutTextAfterDelay(loseText, gameOverDisplayTime));
        }
        else
        {
            Debug.LogWarning("LoseText is not assigned!");
        }
        
        // Hide attempts counter after a short delay
        if (attemptsText != null)
        {
            StartCoroutine(FadeOutTextAfterDelay(attemptsText, 3f));
        }
        
        // Play lose sound
        if (audioSource != null && loseSound != null)
        {
            audioSource.clip = loseSound;
            audioSource.Play();
        }
        
        // Show play again prompt after game over message fades
        StartCoroutine(ShowPlayAgainPromptAfterDelay(gameOverDisplayTime + 1f));
    }
    
    IEnumerator ShowPlayAgainPromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Show the play again UI
        if (playAgainCanvas != null)
        {
            playAgainCanvas.SetActive(true);
            
            // Set the question text
            if (playAgainText != null)
            {
                playAgainText.text = gameWon ? 
                    "Congratulations! Would you like to play again?" : 
                    "Would you like to try again?";
            }
            
            // Play prompt sound
            if (audioSource != null && promptSound != null)
            {
                audioSource.clip = promptSound;
                audioSource.Play();
            }
            
            Debug.Log("Showing play again prompt");
        }
        else
        {
            Debug.LogError("Play again canvas not assigned!");
            
            // If we don't have a play again UI, just reset the game automatically
            if (resetToysOnGameOver)
            {
                StartCoroutine(ResetAllToysAfterDelay(0.5f));
            }
            ResetGameState();
        }
    }
    
    void ResetGameState()
    {
        // Reset game state for a new attempt
        attemptCount = 0;
        gameWon = false;
        isProcessingAttempt = false;
        lastAttemptTime = 0f;
        
        // Keep hasShownAttempts true to skip instructions on replay
        
        // Reset and show attempts counter
        if (attemptsText != null)
        {
            // Reset counter text
            attemptsText.text = $"Attempts: 0/{maxAttempts}";
            
            // Reset color
            attemptsText.color = Color.white;
            
            // Make sure it's visible again
            attemptsText.gameObject.SetActive(true);
            
            // Reset alpha
            Color textColor = attemptsText.color;
            attemptsText.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
            
            // Pulse to draw attention
            StartCoroutine(SinglePulseText(attemptsText, 0.9f, 1.2f, 0.5f));
        }
        
        // Make sure win/lose messages are hidden
        if (winText != null)
        {
            winText.gameObject.SetActive(false);
        }
        
        if (loseText != null)
        {
            loseText.gameObject.SetActive(false);
        }
        
        // Hide instructions canvas if it was showing
        if (instructionsCanvas != null && instructionsCanvas.activeSelf)
        {
            instructionsCanvas.SetActive(false);
        }
        
        // Resubscribe to events to make sure they're working
        UnsubscribeFromClawEvents();
        SubscribeToClawEvents();
        
        Debug.Log("Game state completely reset - player can retry with attempts counter reset to 0");
    }
    
    /// <summary>
    /// Resets all toys with ToyController component to their initial positions
    /// </summary>
    private IEnumerator ResetAllToysAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);
        
        Debug.Log("Resetting all toys to their initial positions");
        
        // Find all toys with ToyController component
        ToyController[] allToys;
        
        if (toyContainer != null)
        {
            // If we have a container, only look for toys within it
            allToys = toyContainer.GetComponentsInChildren<ToyController>(true);
        }
        else
        {
            // Otherwise find all toys in the scene
            allToys = FindObjectsOfType<ToyController>();
        }
        
        // Reset each toy with a small delay between each to prevent physics issues
        int resetCount = 0;
        foreach (ToyController toy in allToys)
        {
            if (toy != null)
            {
                // Reset the toy to its initial position
                toy.ResetToInitialPosition();
                resetCount++;
                
                // Small delay between resets to prevent physics glitches
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }
        }
        
        Debug.Log($"Reset {resetCount} toys to their initial positions");
    }
    
    IEnumerator FadeOutTextAfterDelay(TextMeshPro text, float delay)
    {
        if (text == null) yield break;
        
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);
        
        // Get the initial color
        Color startColor = text.color;
        
        // Fade out over 2 seconds
        float elapsedTime = 0;
        float fadeDuration = 2.0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeDuration);
            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        // Ensure fully transparent at the end
        text.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        // Disable the game object
        text.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Performs a single pulse animation on text instead of continuous pulsing
    /// </summary>
    IEnumerator SinglePulseText(TextMeshPro text, float minScale = 0.8f, float maxScale = 1.2f, float duration = 0.5f)
    {
        if (text == null) yield break;
        
        // Store original scale
        Vector3 originalScale = text.transform.localScale;
        
        // Scale up
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(minScale, maxScale, elapsedTime / duration);
            text.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        // Hold at max scale briefly
        yield return new WaitForSeconds(0.2f);
        
        // Scale down
        elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(maxScale, minScale, elapsedTime / duration);
            text.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        // Return to original scale
        text.transform.localScale = originalScale;
    }
    
    IEnumerator PulseText(TextMeshPro text, float minScale = 0.8f, float maxScale = 1.2f, float duration = 0.5f)
    {
        if (text == null) yield break;
        
        // Store original scale
        Vector3 originalScale = text.transform.localScale;
        
        // Pulse forever
        while (text.gameObject.activeSelf)
        {
            // Scale up
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float scale = Mathf.Lerp(minScale, maxScale, elapsedTime / duration);
                text.transform.localScale = originalScale * scale;
                yield return null;
            }
            
            // Scale down
            elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float scale = Mathf.Lerp(maxScale, minScale, elapsedTime / duration);
                text.transform.localScale = originalScale * scale;
                yield return null;
            }
        }
    }
    
    IEnumerator FlashText(TextMeshPro text, Color flashColor, int flashCount)
    {
        if (text == null) yield break;
        
        // Store original color
        Color originalColor = text.color;
        
        for (int i = 0; i < flashCount; i++)
        {
            // Flash to new color
            text.color = flashColor;
            yield return new WaitForSeconds(0.2f);
            
            // Back to original
            text.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    IEnumerator FadeOutCanvasAfterDelay(GameObject canvas, float delay)
    {
        if (canvas == null) yield break;
        
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);
        
        // Get the CanvasGroup (add it if it doesn't exist)
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = canvas.AddComponent<CanvasGroup>();
        
        // Fade out over 0.5 seconds (much faster than before)
        float elapsedTime = 0;
        float fadeDuration = 0.5f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        
        // Ensure fully transparent at the end
        canvasGroup.alpha = 0f;
        
        // Disable the canvas
        canvas.SetActive(false);
    }
    
    IEnumerator FadeOutUITextAfterDelay(TextMeshProUGUI text, float delay)
    {
        if (text == null) yield break;
        
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);
        
        // Get the initial color
        Color startColor = text.color;
        
        // Fade out over 2 seconds
        float elapsedTime = 0;
        float fadeDuration = 2.0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeDuration);
            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        // Ensure fully transparent at the end
        text.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        // Disable the game object
        text.gameObject.SetActive(false);
    }
}
