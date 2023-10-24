using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBlock : MonoBehaviour
{
    public static RotateBlock Instance { set; get; }
    public float rotationDuration = 1;
    public float startTimeForR = 0;
    public AudioSource speaker;
    public AudioClip swoosh;
    Quaternion targetRotation;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
         targetRotation = Quaternion.Euler(0, 90f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var selection = hit.transform;
                if (selection.tag == "Block")
                {
                    speaker.PlayOneShot(swoosh);
                    selection.transform.Rotate(0, 90, 0, Space.World);
                   
       
                    
                }
            }
        }
        
    }
}
