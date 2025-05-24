using UnityEngine;

public class BlinkControl1 : MonoBehaviour
{
    [SerializeField] private BlinkingBumper1[] buttons;

    public void StartBlinking1()
    {
        foreach (var button in buttons)
        {
            button.StartBlinking1();
        }
    }

    public void StopBlinking1()
    {
        foreach (var button in buttons)
        {
            button.StopBlinking1();
        }
    }
}