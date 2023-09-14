using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { set; get; }
    public GameObject PauseMenu;
    public GameObject WinMenu;
    public GameObject FailMenu;

    bool isPaused = false;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        PauseMenu.SetActive(false);
        WinMenu.SetActive(false);
        FailMenu.SetActive(false);

        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void GoHome()
    {
        SceneManager.LoadScene("Home");
    }
    public void NextLevel (string nextLevel)
    {
        SceneManager.LoadScene(nextLevel);
    }
    public void ResumeGame()
    {
        isPaused = false;
        PauseMenu.SetActive(false);
        PauseMenu.GetComponent<Animator>().SetBool("paused", false);
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        isPaused = true;
        PauseMenu.SetActive(true);
        PauseMenu.GetComponent<Animator>().SetBool("paused", true);
        Time.timeScale = 0;
    }
    public void TimeUp()
    {
        Debug.Log("Time is Up");
        Time.timeScale = 0;
        FailMenu.SetActive(true);
        FailMenu.GetComponent<Animator>().SetTrigger("gameOver");
    }

    public void TargetReached()
    {
        WinMenu.SetActive(true);
      
        WinMenu.GetComponent<Animator>().SetTrigger("gameOver");
        Debug.Log("Target reached");

    }
}
