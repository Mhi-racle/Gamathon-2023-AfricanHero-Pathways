using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Camera cam;
    public float speed = 6f;
    public float turnSmoothSpeed = 0.1f;
    float turnSmoothVelocity;
    private Rigidbody rb;

    public Animator playerAnimator;
    //public fixedJoystick fixedJoystick;

    public FixedJoystick fixedJoystick;
    public bool isWalking;
    
    Vector3 lookPos;



    // Start is called before the first frame update
    void Start()
    {
      
        rb = GetComponent<Rigidbody>();
        fixedJoystick = FindAnyObjectByType<FixedJoystick>();

    }


    // Update is called once per frame
    void Update()
    {

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }
    void FixedUpdate()
    {
        float verticalInput;
        float horizontalInput;
       

         verticalInput = fixedJoystick.Vertical;
         horizontalInput = fixedJoystick.Horizontal;

     

        //#region Standalone Inputs
        //verticalInput = Input.GetAxis("Vertical");
        //horizontalInput = Input.GetAxis("Horizontal");
        //#endregion

        Vector3 direction = new Vector3(horizontalInput, 0f, verticalInput).normalized;
       

        //if the direction is greater than zero
        if (direction.magnitude >= 0.1f)
        {
            //if player is moving, then play the rolling animation and also set isRolling to true
            //isRolling notifies the shooting script so that it doesnt shoot when the player is rolling
            playerAnimator.SetBool("isWalking", true);
            isWalking = true;

      
            //calculates the target angle to be rotated to 
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //moves the player in the direction of the camera
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
            // rb.AddForce(moveDirection * speed);



        }
        else
        {
            playerAnimator.SetBool("isWalking", false);
        }


        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            lookPos = hit.point;
          
        }

         
        Vector3 lookDir = lookPos - transform.position;

        lookPos.y = 2;
        
        lookDir.y = 0;
        transform.LookAt(transform.position + lookDir, Vector3.up);
    }


   
}
