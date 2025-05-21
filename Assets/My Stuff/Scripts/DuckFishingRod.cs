using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DuckFishingRod : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private DuckFishingGameManager gameManager;
    private GameObject button;

    void Start()
    {
        gameManager = FindObjectOfType<DuckFishingGameManager>();
        button = GameObject.Find("ButtonTutorial");
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!gameManager.gameStarted)
        {
            gameManager.tutorialTextRod.gameObject.SetActive(false);
            Destroy(button);
            gameManager.GameStart();
        }
    }
}
