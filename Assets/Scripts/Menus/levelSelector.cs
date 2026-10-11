using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>A numbered button on the level selection screen.</summary>
public class levelSelector : MonoBehaviour
{
    public int level;
    public TextMeshProUGUI levelText;

    private void Start()
    {
        if (levelText != null) levelText.text = level.ToString();
    }

    public void OpenScene()
    {
        if (!LevelProgress.Exists(level))
        {
            Debug.LogWarning($"Level {level} is not in Build Settings.");
            return;
        }

        SceneManager.LoadScene(LevelProgress.SceneNameFor(level));
    }
}
