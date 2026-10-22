using UnityEngine;

/// <summary>
/// Moves Ananse relative to the camera. Reads the on-screen joystick on mobile and
/// WASD / arrow keys on PC. Movement is analog (a small push walks slowly, for careful steps in
/// tight corridors), eases in and out instead of starting and stopping dead, and the walk
/// animation's pace follows his speed. He turns smoothly to face the way he walks.
/// </summary>
/// <remarks>The file is named PlayerMovement.cs for historical reasons; the class is attached in every level.</remarks>
public class PlayerController : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private const float InputDeadZone = 0.1f;

    // Below this fraction of full speed Ananse counts as standing still.
    private const float StillSpeedFraction = 0.05f;

    public CharacterController controller;
    public Camera cam;
    public float speed = 6f;

    // Replaces the old "turnSmoothSpeed", which the scenes had set to 20: as a smoothing *time*
    // that meant 20 seconds to turn, so Ananse never faced the way he walked.
    [Tooltip("Seconds Ananse takes to turn towards his walking direction. Smaller is snappier.")]
    [SerializeField] private float turnSmoothTime = 0.08f;
    public Animator playerAnimator;

    [Tooltip("The on-screen joystick. Any Joystick Pack type works (Fixed, Floating, Dynamic, Variable).")]
    public Joystick fixedJoystick;

    [Header("Feel")]
    [Tooltip("Speed for the gentlest push, as a fraction of full speed. A full push always gives full speed.")]
    [Range(0f, 1f)]
    [SerializeField] private float minPushSpeedFraction = 0.35f;

    [Tooltip("Seconds to reach full speed from standing.")]
    [SerializeField] private float accelerationTime = 0.12f;

    [Tooltip("Seconds to stop from full speed after letting go.")]
    [SerializeField] private float decelerationTime = 0.08f;

    [Tooltip("Walk-animation pace at the slowest walking speed (1 = normal). Keeps his feet from sliding.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float slowestAnimationPace = 0.6f;

    /// <summary>True while Ananse is moving this physics step.</summary>
    public bool isWalking;

    private float turnSmoothVelocity;
    private float currentSpeed;
    private Vector3 moveDirection;

    private void Start()
    {
        if (cam == null) cam = Camera.main;

        // The joystick lives under the "For Mobile" canvas, which is hidden on PC.
        if (fixedJoystick == null) fixedJoystick = FindAnyObjectByType<Joystick>(FindObjectsInactive.Include);
    }

    private void OnDisable()
    {
        // Stop at once when control is taken away (pause, win, lose, dialogue).
        currentSpeed = 0f;
        SetWalking(false);
    }

    private void Update()
    {
        // Ananse stays on the maze floor.
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    private void FixedUpdate()
    {
        Vector2 input = Vector2.ClampMagnitude(ReadMoveInput(), 1f);
        float push = input.magnitude;
        float targetSpeed = 0f;

        if (push >= InputDeadZone)
        {
            // Analog: map the push from just past the dead zone (gentlest) to full.
            float strength = Mathf.InverseLerp(InputDeadZone, 1f, push);
            targetSpeed = speed * Mathf.Lerp(minPushSpeedFraction, 1f, strength);

            // Direction relative to where the camera is looking.
            float inputAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            moveDirection = Quaternion.Euler(0f, inputAngle, 0f) * Vector3.forward;

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, inputAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        // Ease towards the target speed; when the stick is released he glides to a quick stop
        // in the direction he was going.
        float rampTime = targetSpeed > currentSpeed ? accelerationTime : decelerationTime;
        float rate = speed / Mathf.Max(0.01f, rampTime);
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        bool moving = currentSpeed > speed * StillSpeedFraction;
        SetWalking(moving);
        if (!moving)
        {
            currentSpeed = targetSpeed > 0f ? currentSpeed : 0f;
            return;
        }

        controller.Move(moveDirection * (currentSpeed * Time.deltaTime));

        // Match the walk cycle to his pace so his feet don't slide at low speed.
        if (playerAnimator != null)
        {
            playerAnimator.speed = Mathf.Lerp(slowestAnimationPace, 1f, Mathf.InverseLerp(speed * minPushSpeedFraction, speed, currentSpeed));
        }
    }

    private void SetWalking(bool walking)
    {
        if (isWalking == walking) return;

        isWalking = walking;
        if (playerAnimator == null) return;

        playerAnimator.SetBool(IsWalkingHash, walking);

        // The idle animation always plays at normal pace.
        if (!walking) playerAnimator.speed = 1f;
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
