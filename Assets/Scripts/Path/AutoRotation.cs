using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotation : MonoBehaviour
{
    public float delay;
    AudioSource speaker;
    
    // Start is called before the first frame update
    void Start()
    {
        speaker = GetComponent<AudioSource>();
        InvokeRepeating("AutoRotate", 1, delay);
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
