using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { set; get; }
    public GameObject PauseMenu;

    bool isPaused = false;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        PauseMenu.SetActive(true);
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

    public void ResumeGame()
    {
        isPaused = false;
        PauseMenu.GetComponent<Animator>().SetBool("paused", false);
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        isPaused = true;
        PauseMenu.GetComponent<Animator>().SetBool("paused", true);
        Time.timeScale = 0;
    }
    public void TimeUp()
    {
        Debug.Log("Time is Up");
    }
}
