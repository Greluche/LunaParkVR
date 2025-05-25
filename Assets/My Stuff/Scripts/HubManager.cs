using UnityEngine;
using UnityEngine.Serialization;

public class HubManager : MonoBehaviour
{
    [Header("Game Completion Keys")]
    [Tooltip("List of PlayerPrefs keys that indicate each game has been completed.")]
    [SerializeField] private string[] gameCompletionKeys;
    
    [Header("Access Control")]
    [Tooltip("Object to activate once all games are completed (e.g. haunted house entrance).")]
    [SerializeField] private GameObject hauntedHouseNpc;
    [SerializeField] private GameObject doorCollider;

    private void Start()
    {
        CheckProgress();
    }

    public void CheckProgress()
    {
        foreach (string key in gameCompletionKeys)
        {
            if (PlayerPrefs.GetInt(key, 0) == 0)
            {
                Debug.Log($"Not all games completed. Missing: {key}");
                doorCollider.SetActive(false);
                return;
            }
        }

        Debug.Log("🎉 All games completed — haunted house unlocked!");
        hauntedHouseNpc.SetActive(false);
        doorCollider.SetActive(true);
    }
}