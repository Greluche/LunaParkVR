using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

public class BackroomsManager : MonoBehaviour
{
    [Header("Puzzle Status")]
    public bool puzzle1Complete = false;
    public bool puzzle2Complete = false;
    public bool puzzle3Complete = false;

    [Header("Climbing Holds")]
    public ClimbInteractable hold1;
    public ClimbInteractable hold2;
    public ClimbInteractable hold3;
    public GameObject teleportVolume;

    [Header("Puzzle Access")] 
    public GameObject introUI;
    public GameObject firstLight;
    public GameObject secondLight;
    public GameObject thirdLight;
    public GameObject secondBlock;
    public GameObject thirdBlock;
    public GameObject climbUI;

    [Header("Climbing Hold Visuals")]
    public Renderer hold1Renderer;
    public Renderer hold2Renderer;
    public Renderer hold3Renderer;

    public Color activatedColor1 = Color.cyan;
    public Color activatedColor2 = Color.red;
    public Color activatedColor3 = Color.green;
    
    private bool hasActivatedClimbing = false;

    private void Start()
    {
        teleportVolume.SetActive(false);
        hold1.enabled = false;
        hold2.enabled = false;
        hold3.enabled = false;
        secondLight.SetActive(false);
        thirdLight.SetActive(false);
        climbUI.SetActive(false);
    }
    private void Update()
    {
        if (!hasActivatedClimbing)
        {
            if (puzzle1Complete)
            {
                introUI.SetActive(false);
                hold1.enabled = true;
                firstLight.SetActive(false);
                secondLight.SetActive(true);
                secondBlock.SetActive(false);
                hold1Renderer.material.color = activatedColor1;
            }

            if (puzzle2Complete)
            {
                hold2.enabled = true;
                thirdLight.SetActive(true);
                secondLight.SetActive(false);
                thirdBlock.SetActive(false);
                hold2Renderer.material.color = activatedColor2;
            }

            if (puzzle3Complete)
            {
                hold3.enabled = true;
                thirdLight.SetActive(false);
                hold3Renderer.material.color = activatedColor3;
                teleportVolume.SetActive(true);
                climbUI.SetActive(true);
            }

            if (puzzle1Complete && puzzle2Complete && puzzle3Complete)
            {
                hasActivatedClimbing = true;
                Debug.Log("🧠 All puzzles complete! Climb your way out...");
                // Optional: trigger music, light change, escape VFX
            }
        }
    }

    // Optional method to set puzzle completion from outside
    public void MarkPuzzleComplete(int puzzleIndex)
    {
        switch (puzzleIndex)
        {
            case 1: puzzle1Complete = true; break;
            case 2: puzzle2Complete = true; break;
            case 3: puzzle3Complete = true; break;
        }
    }
}