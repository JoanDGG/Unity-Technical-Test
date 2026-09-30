using System.Collections.Generic;
using UnityEngine;

public static class LevelProgressionManager
{
    private static int MaxLevels = 5;

    private const string HighestUnlockedLevelKey =
        "HighestUnlockedLevel";

    public static int HighestUnlockedLevel =>
        PlayerPrefs.GetInt(
            HighestUnlockedLevelKey,
            1
        );

    public static bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= HighestUnlockedLevel;
    }

    public static void CompleteLevel(int levelIndex)
    {
        int nextLevel = Mathf.Min(
            levelIndex + 1,
            MaxLevels
        );

        if (nextLevel > HighestUnlockedLevel)
        {
            PlayerPrefs.SetInt(
                HighestUnlockedLevelKey,
                nextLevel
            );

            PlayerPrefs.Save();
        }
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(
            HighestUnlockedLevelKey
        );

        PlayerPrefs.Save();
    }
}