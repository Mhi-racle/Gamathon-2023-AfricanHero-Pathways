using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class BGMusic : MonoBehaviour
{
    public static BGMusic instance;

    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject);
        }

       
    }
    void Start()
    {
        

        if (PlayerPrefs.GetString("music") == "true")
        {
            BGMusic.instance.GetComponent<AudioSource>().Play();
        }
        else
        {
            BGMusic.instance.GetComponent<AudioSource>().Pause();
        }
       
    }

    private void Update()
    {
       if(!BGMusic.instance.GetComponent<AudioSource>().isPlaying)
        {
            BGMusic.instance.GetComponent<AudioSource>().Play();
        }
    }



}
