using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DuckFishingRod : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private DuckFishingGameManager gameManager;
    private GameObject button;
    private GameObject rod;

    void Start()
    {
        gameManager = FindObjectOfType<DuckFishingGameManager>();
        button = GameObject.Find("ButtonTutorial");
        rod = GameObject.Find("TutorialFishingRod");
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!gameManager.gameStarted)
        {
            gameManager.tutorialTextRod.gameObject.SetActive(false);
            Destroy(button);
            Destroy(rod);
            gameManager.GameStart();
        }
    }
}
