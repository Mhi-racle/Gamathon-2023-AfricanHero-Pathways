using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject fireball;
    public Transform firingPoint;
    public Animator playerAnimator;

    public void Shoot()
    {
        playerAnimator.SetTrigger("shooting");
            GameObject ball = Instantiate(fireball, firingPoint.position, Quaternion.identity, transform.parent);
            ball.GetComponent<Rigidbody>().AddForce(transform.forward * 2000);
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Shoot();
        }
    }

}
