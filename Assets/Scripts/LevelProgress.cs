using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Reads and writes level progression. Levels are scenes named "Level N";
/// progress is the highest unlocked level number and never goes backwards.
/// </summary>
public static class LevelProgress
{
    /// <summary>Highest level the player may open (at least 1).</summary>
    public static int HighestUnlocked => Mathf.Max(1, PlayerPrefs.GetInt(GameConstants.Prefs.LevelsUnlocked, 1));

    /// <summary>Unlocks <paramref name="level"/> unless a later level is already unlocked.</summary>
    public static void Unlock(int level)
    {
        if (level <= HighestUnlocked) return;

        PlayerPrefs.SetInt(GameConstants.Prefs.LevelsUnlocked, level);
        PlayerPrefs.Save();
    }

    public static string SceneNameFor(int level) => GameConstants.Scenes.LevelPrefix + level;

    /// <summary>True when "Level N" exists and is included in Build Settings.</summary>
    public static bool Exists(int level) => Application.CanStreamedLevelBeLoaded(SceneNameFor(level));

    /// <summary>Level number of the active scene, or 0 if it is not a level scene.</summary>
    public static int CurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.StartsWith(GameConstants.Scenes.LevelPrefix)) return 0;

        return int.TryParse(sceneName.Substring(GameConstants.Scenes.LevelPrefix.Length), out int level) ? level : 0;
    }
}
