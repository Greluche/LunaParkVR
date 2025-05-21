using UnityEngine;

public class BlinkControl : MonoBehaviour
{
    [SerializeField] private BlinkingBumper[] buttons;

    public void StartBlinking()
    {
        foreach (var button in buttons)
        {
            button.StartBlinking();
        }
    }

    public void StopBlinking()
    {
        foreach (var button in buttons)
        {
            button.StopBlinking();
        }
    }
}