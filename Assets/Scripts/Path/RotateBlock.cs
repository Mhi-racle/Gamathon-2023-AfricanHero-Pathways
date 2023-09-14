using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBlock : MonoBehaviour
{
    public float rotationDuration = 1;
    public float startTimeForR = 0;
    public AudioSource speaker;
    public AudioClip swoosh;
    Quaternion targetRotation;
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
                   
                   
                        //float rotationTime = (Time.time - startTimeForR) / rotationDuration;
                        //if (rotationTime < 1)
                        //{
                        //    // Use Quaternion.Lerp to smoothly rotate the player
                        //    selection.rotation = Quaternion.Lerp(selection.rotation, targetRotation, rotationTime);

                        //}
                      
                    
                }
            }
        }
        
    }
}
