using UnityEngine;

using UnityEngine;

public class BlinkingPrimary : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private MeshRenderer buttonRenderer;
    [SerializeField] private Color blinkColor = Color.green;
    [SerializeField] private float blinkInterval = 0.5f;

    private Material buttonMaterial;
    private Color originalColor;
    private float timer;
    private bool isBlinking = true;

    void Start()
    {
        if (buttonRenderer == null)
        {
            Debug.LogError("No MeshRenderer assigned to BlinkingButton.");
            enabled = false;
            return;
        }

        buttonMaterial = buttonRenderer.material; // instance copy
        originalColor = buttonMaterial.color;
        timer = blinkInterval;
    }

    void Update()
    {
        if (!isBlinking) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ToggleColor();
            timer = blinkInterval;
        }
    }

    private void ToggleColor()
    {
        if (buttonMaterial.color == originalColor)
        {
            buttonMaterial.color = blinkColor;
        }
        else
        {
            buttonMaterial.color = originalColor;
        }
    }

    public void StopBlinking()
    {
        isBlinking = false;
        buttonMaterial.color = originalColor;
    }

    public void StartBlinking()
    {
        isBlinking = true;
        timer = 0f;
    }
}