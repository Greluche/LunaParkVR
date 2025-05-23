using UnityEngine;

public static class TutorialProgress
{
    public static void MarkTutorialComplete(string key)
    {
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }
}