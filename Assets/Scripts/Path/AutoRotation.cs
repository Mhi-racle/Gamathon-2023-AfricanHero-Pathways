using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script requires AudioSource
[RequireComponent(typeof(AudioSource))]

public class AutoRotation : MonoBehaviour
{
    

   private float delay = 1;
    AudioSource speaker;
    
    // Start is called before the first frame update
    void Start()
    {
        speaker = GetComponent<AudioSource>();
        speaker.clip = RotateBlock.Instance.swoosh;
        InvokeRepeating("AutoRotate", 1, delay);

        gameObject.tag = "Untagged";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AutoRotate()
    {
        speaker.Play();
        transform.Rotate(0, 90, 0, Space.World);

    }
}
