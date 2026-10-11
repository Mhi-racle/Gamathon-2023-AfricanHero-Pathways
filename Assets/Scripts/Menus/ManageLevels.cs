using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Level selection screen: unlocks the buttons for levels the player has reached and that
/// exist in the build. Buttons for levels that haven't been made yet stay locked.
/// </summary>
public class ManageLevels : MonoBehaviour
{
    public Button[] buttons;

    private void Start()
    {
        Time.timeScale = 1;
        int highestUnlocked = LevelProgress.HighestUnlocked;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            // Each button's levelSelector knows its level; fall back to its position in the list.
            int level = buttons[i].TryGetComponent(out levelSelector selector) ? selector.level : i + 1;
            buttons[i].interactable = level <= highestUnlocked && LevelProgress.Exists(level);
        }
    }

    private void Update()
    {
        // Escape on PC, Back on Android.
        if (Input.GetKeyDown(KeyCode.Escape)) goBack();
    }

    public void goBack()
    {
        SceneManager.LoadScene(GameConstants.Scenes.MainMenu);
    }
}
