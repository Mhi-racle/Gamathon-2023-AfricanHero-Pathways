using UnityEngine;

/// <summary>
/// Moves Ananse relative to the camera. Reads the on-screen joystick on mobile and
/// WASD / arrow keys on PC, and turns him smoothly to face the way he walks.
/// </summary>
/// <remarks>The file is named PlayerMovement.cs for historical reasons; the class is attached in every level.</remarks>
public class PlayerController : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private const float InputDeadZone = 0.1f;

    public CharacterController controller;
    public Camera cam;
    public float speed = 6f;

    // Replaces the old "turnSmoothSpeed", which the scenes had set to 20: as a smoothing *time*
    // that meant 20 seconds to turn, so Ananse never faced the way he walked.
    [Tooltip("Seconds Ananse takes to turn towards his walking direction. Smaller is snappier.")]
    [SerializeField] private float turnSmoothTime = 0.08f;
    public Animator playerAnimator;
    public FixedJoystick fixedJoystick;

    /// <summary>True while Ananse is moving this physics step.</summary>
    public bool isWalking;

    private float turnSmoothVelocity;

    private void Start()
    {
        if (cam == null) cam = Camera.main;

        // The joystick lives under the "For Mobile" canvas, which is hidden on PC.
        if (fixedJoystick == null) fixedJoystick = FindAnyObjectByType<FixedJoystick>(FindObjectsInactive.Include);
    }

    private void OnDisable()
    {
        // Stop the walk cycle when control is taken away (pause, win, lose, dialogue).
        isWalking = false;
        if (playerAnimator != null) playerAnimator.SetBool(IsWalkingHash, false);
    }

    private void Update()
    {
        // Ananse stays on the maze floor.
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    private void FixedUpdate()
    {
        Vector2 input = ReadMoveInput();
        Vector3 direction = new Vector3(input.x, 0f, input.y);

        isWalking = direction.magnitude >= InputDeadZone;
        if (playerAnimator != null) playerAnimator.SetBool(IsWalkingHash, isWalking);
        if (!isWalking) return;

        direction.Normalize();

        // Angle of the input relative to where the camera is looking.
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        controller.Move(moveDirection * speed * Time.deltaTime);
    }

    /// <summary>Joystick and keyboard combined; whichever is pushed further wins.</summary>
    private Vector2 ReadMoveInput()
    {
        Vector2 keyboard = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (fixedJoystick == null || !fixedJoystick.isActiveAndEnabled) return keyboard;

        Vector2 stick = new Vector2(fixedJoystick.Horizontal, fixedJoystick.Vertical);
        return stick.sqrMagnitude > keyboard.sqrMagnitude ? stick : keyboard;
    }
}
