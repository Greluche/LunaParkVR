using UnityEngine;

public class ButtonPush : MonoBehaviour
{
    public BackroomsManager manager;

    public void ButtonPushed()
    {
        manager.MarkPuzzleComplete(1);
    }
}
