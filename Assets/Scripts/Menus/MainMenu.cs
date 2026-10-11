using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Main menu buttons.</summary>
public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        // In case we arrived here from a paused level.
        Time.timeScale = 1;
    }

    public void GoTo(string sceneToGo)
    {
        SceneManager.LoadScene(sceneToGo);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
