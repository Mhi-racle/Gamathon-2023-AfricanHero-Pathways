using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip click;
    public AudioSource audioSource;

    public void ClickSound()
    {
        audioSource.PlayOneShot(click);
    }
}
