using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health;
    public Slider healthBar;
    public Animator playerAnimator;

  

    private void Update()
    {
        healthBar.value = health;

        if(health <= 0)
        {
            playerAnimator.SetTrigger("dead");

           
        }
    }
}
