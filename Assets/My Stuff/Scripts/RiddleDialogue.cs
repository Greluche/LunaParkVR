using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RiddleDialogue : MonoBehaviour
{
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public Button[] answerButtons; // Set in inspector
    public string[] dialogueLines;
    public AudioSource duckGodVoice;
    public Animator characterAnimator; // Drag in the riddle character's animator
    public string wrongAnswerAnimationTrigger = "Wrong";
    public string rightAnswerAnimationTrigger = "Right"; // Animator trigger name
    public string wrongResponseLine = "INCORRECT"; // The new line
    public float delayBeforeReEnabling = 2f;
    public AudioSource successAudio;
    public ParticleSystem successEffect;
    public BackroomsManager manager; 
    
    [Header("XR Input")]
    public InputActionProperty nextLineButton;

    private int index = 0;
    private bool isDialogueActive = false;

    public void Start()
    {
        dialogueBox.SetActive(false);
        foreach (var btn in answerButtons)
            btn.gameObject.SetActive(false);
        nextLineButton.action.Enable();
        
    }
    
    public void StartDialogue()
    {
        dialogueBox.SetActive(true);
        index = 0;
        isDialogueActive = true;
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        duckGodVoice.Play();
        if (index < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[index];
            index++;
        }
        else
        {
            isDialogueActive = false;
            dialogueText.text = "";
            ShowRiddleChoices();
        }
    }

    private void ShowRiddleChoices()
    {
        foreach (var btn in answerButtons)
        {
            btn.interactable = true;
            btn.gameObject.SetActive(true);
        }

        //Canvas.ForceUpdateCanvases();
    }
    public void AnswerSelected(int choiceIndex)
    {
        Debug.Log("Player chose: " + choiceIndex);
            
        bool isCorrect = (choiceIndex == 3); // define this elsewhere

        if (isCorrect)
        {
            Debug.Log("Correct answer!");
            StartCoroutine(HandleCorrectAnswer());
        }
        else
        {
            StartCoroutine(HandleWrongAnswer());
        }
    }

    void Update()
    {
        if (isDialogueActive && nextLineButton.action.WasPressedThisFrame())
        {
            ShowNextLine();
        }
    }
    
    private IEnumerator HandleWrongAnswer()
    {
        // Hide buttons
        foreach (var btn in answerButtons)
            btn.gameObject.SetActive(false);

        // Show wrong response line
        dialogueText.text = wrongResponseLine;
        duckGodVoice.Play();
        // Play the animation
        if (characterAnimator != null)
            characterAnimator.SetTrigger(wrongAnswerAnimationTrigger);

        // Wait for animation to finish or for a fixed delay
        yield return new WaitForSeconds(delayBeforeReEnabling);

        dialogueText.text = "";
        // Show buttons again
        foreach (var btn in answerButtons)
            btn.gameObject.SetActive(true);
    }
    
    private IEnumerator HandleCorrectAnswer()
    {
        manager.MarkPuzzleComplete(3);
        duckGodVoice.Play();
        successAudio.Play();
        ParticleSystem fx = Instantiate(successEffect, dialogueBox.transform.position, Quaternion.LookRotation(Vector3.up));
        fx.transform.localScale = Vector3.one * 2.0f;
        fx.Play();
        Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        dialogueText.text = "That is correct! Well done."; // Or whatever message you want
        
        // Optionally hide buttons
        foreach (var btn in answerButtons)
            btn.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f); // Adjust the delay as needed
        
        if (characterAnimator != null)
            characterAnimator.SetTrigger(rightAnswerAnimationTrigger);
        
        dialogueBox.SetActive(false);
    }
}