using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialRod : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private TutorialManager gameManager;
    private GameObject button;

    void Start()
    {
        gameManager = FindObjectOfType<TutorialManager>();
        button = GameObject.Find("ButtonTutorial");
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        gameManager.tutorialTextRod.gameObject.SetActive(false);
        Destroy(button);
    }
}
