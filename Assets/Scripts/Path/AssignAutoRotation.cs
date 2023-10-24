using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignAutoRotation : MonoBehaviour
{
    public GameObject[] blocks;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            int rand = Random.Range(0, blocks.Length);
            if(i == rand)
            {
                blocks[i].AddComponent<AutoRotation>();
            }
        }
       

    
    }

    
}
