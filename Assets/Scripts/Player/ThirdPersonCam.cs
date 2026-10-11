using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("Transforms")]

    public Transform orientation;
    public Transform player;
    public Transform playerObj;

    [Header("Rigidbody")]
    public CharacterController characterController;

    [Header("floats")]
    public float rotationSpeed;
    public FixedJoystick fixedJoystick;

    private void Start()
    {
        fixedJoystick = FindAnyObjectByType<FixedJoystick>();
    }
    private void Update()
    {
        // The Main Menu has this script but no joystick; without one there is nothing to steer.
        if (fixedJoystick == null || orientation == null || player == null || playerObj == null) return;

        //rotate orientation
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir;

        //rotate player object
        float horizontalInput = fixedJoystick.Horizontal;
        float verticalInput = fixedJoystick.Vertical;

        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(inputDir != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }


}
