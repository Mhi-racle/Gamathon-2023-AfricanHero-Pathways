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
    public GameObject MainCanvas;

    bool isPaused = false;

    public AudioClip congratulatorySound;
    public AudioSource audioSource;

    public GameObject player;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        MainCanvas.SetActive(true);
        PauseMenu.SetActive(false);
        WinMenu.SetActive(false);
        FailMenu.SetActive(false);

        Time.timeScale = 1;


    }

    // Update is called once per frame
    void Update()
    {
        if (DialogManager.isActive)
        {
            MainCanvas.SetActive(false);
            gameObject.GetComponent<RotateBlock>().enabled = false;
            player.GetComponent<PlayerController>().enabled = false;
        }
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
        SceneManager.LoadScene("Main Menu");
    }
    public void NextLevel (int nextLevel)
    {
        PlayerPrefs.SetInt("levelsUnlocked", nextLevel);
        SceneManager.LoadScene("Level " + nextLevel);
    }
    public void ResumeGame()
    {
        MainCanvas.SetActive(true);
        isPaused = false;
        PauseMenu.SetActive(false);
        PauseMenu.GetComponent<Animator>().SetBool("paused", false);
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        MainCanvas.SetActive(false);
        isPaused = true;
        PauseMenu.SetActive(true);
        PauseMenu.GetComponent<Animator>().SetBool("paused", true);
        Time.timeScale = 0;
    }
    public void TimeUp()
    {
        Debug.Log("Time is Up");
        MainCanvas.SetActive(false);
        FailMenu.SetActive(true);
        FailMenu.GetComponent<Animator>().SetTrigger("gameOver");

        gameObject.GetComponent<Timer>().enabled = false;
        gameObject.GetComponent<RotateBlock>().enabled = false;

    }

    public void TargetReached()
    {
        audioSource.PlayOneShot(congratulatorySound);
        WinMenu.SetActive(true);
        MainCanvas.SetActive(false);
        WinMenu.GetComponent<Animator>().SetTrigger("gameOver");
        Debug.Log("Target reached");
        gameObject.GetComponent<Timer>().enabled = false;
        gameObject.GetComponent<RotateBlock>().enabled = false;
    }
}
