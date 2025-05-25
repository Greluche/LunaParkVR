using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DuckFishingRod : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private DuckFishingGameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<DuckFishingGameManager>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (gameManager != null && !gameManager.tutorialCompleted)
            gameManager.OnRodGrabbed();
    }
}
