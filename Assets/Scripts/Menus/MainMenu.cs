using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Main menu buttons.</summary>
public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        // In case we arrived here from a paused level.
        Time.timeScale = 1;

#if UNITY_WEBGL
        // A browser tab can't be closed by the game, so a Quit button would do nothing.
        HideQuitButtons();
#endif
    }

    public void GoTo(string sceneToGo)
    {
        SceneManager.LoadScene(sceneToGo);
    }

    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>Hides every button wired to <see cref="Quit"/>, without needing an Inspector reference.</summary>
    private void HideQuitButtons()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentMethodName(i) == nameof(Quit))
                {
                    button.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }
}
