using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves Ananse. Two ways to play, at the same time:
/// <list type="bullet">
/// <item><b>Auto-walk:</b> as soon as the player's block turns open a clear path to the gourd,
/// Ananse walks it by himself, so a player who only turns blocks can finish a level.</item>
/// <item><b>Steering:</b> the on-screen joystick (mobile) or WASD / arrow keys (PC) move him
/// directly and always take over from auto-walk; useful for levels where he must go partway first.</item>
/// </list>
/// Movement is analog (a small push walks slowly), eases in and out, and the walk animation's pace
/// follows his speed. He always moves through his CharacterController, so walls always block him.
/// </summary>
/// <remarks>The file is named PlayerMovement.cs for historical reasons; the class is attached in every level.</remarks>
public class PlayerController : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private const float InputDeadZone = 0.1f;

    // Below this fraction of full speed Ananse counts as standing still.
    private const float StillSpeedFraction = 0.05f;

    // How often to check for a clear path while idle (the walk map also triggers a check after every turn).
    private const float PathCheckSeconds = 0.25f;

    // Give up the current auto-walk if Ananse barely moves for this long (e.g. a block turned into his way).
    private const float StuckSeconds = 0.4f;

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

    [Header("Auto-walk")]
    [Tooltip("When the player's turns open a clear path to the gourd, Ananse walks it by himself.")]
    [SerializeField] private bool autoWalkWhenPathClear = true;

    [Tooltip("How close Ananse must get to each point on his route before heading for the next.")]
    [SerializeField] private float arriveDistance = 0.04f;

    /// <summary>True while Ananse is moving this physics step.</summary>
    public bool isWalking;

    private float turnSmoothVelocity;
    private float currentSpeed;
    private Vector3 moveDirection;

    private Transform gourd;
    private readonly List<Vector3> route = new List<Vector3>();
    private int routeIndex;
    private bool followingRoute;
    private float pathCheckTimer;
    private float stuckTimer;

    private void Start()
    {
        if (cam == null) cam = Camera.main;

        // The joystick lives under the "For Mobile" canvas, which is hidden on PC.
        if (fixedJoystick == null) fixedJoystick = FindAnyObjectByType<Joystick>(FindObjectsInactive.Include);

        target gourdTrigger = FindAnyObjectByType<target>();
        if (gourdTrigger != null) gourd = gourdTrigger.transform;
    }

    private void OnEnable()
    {
        WalkGrid.Rebuilt += CheckSoon;
    }

    private void OnDisable()
    {
        WalkGrid.Rebuilt -= CheckSoon;

        // Stop at once when control is taken away (pause, win, lose, dialogue).
        StopRoute();
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
            // Steering always wins over auto-walk.
            StopRoute();

            // Analog: map the push from just past the dead zone (gentlest) to full.
            float strength = Mathf.InverseLerp(InputDeadZone, 1f, push);
            targetSpeed = speed * Mathf.Lerp(minPushSpeedFraction, 1f, strength);

            // Direction relative to where the camera is looking.
            float inputAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            FaceAndHead(Quaternion.Euler(0f, inputAngle, 0f) * Vector3.forward);
        }
        else if (autoWalkWhenPathClear)
        {
            pathCheckTimer -= Time.deltaTime;
            if (pathCheckTimer <= 0f) CheckPathToGourd();

            if (followingRoute && TryGetRouteDirection(out Vector3 routeDirection))
            {
                targetSpeed = speed;
                FaceAndHead(routeDirection);
            }
        }

        // Ease towards the target speed; when released he glides to a quick stop the way he was going.
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

        Vector3 before = transform.position;
        float step = currentSpeed * Time.deltaTime;
        controller.Move(moveDirection * step);

        if (followingRoute) CheckStuck(before, step);

        // Match the walk cycle to his pace so his feet don't slide at low speed.
        if (playerAnimator != null)
        {
            playerAnimator.speed = Mathf.Lerp(slowestAnimationPace, 1f, Mathf.InverseLerp(speed * minPushSpeedFraction, speed, currentSpeed));
        }
    }

    private void FaceAndHead(Vector3 direction)
    {
        moveDirection = direction;

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void CheckSoon()
    {
        pathCheckTimer = 0f;
    }

    /// <summary>
    /// Starts (or refreshes) the walk to the gourd if a clear path exists, and stops it if a turning
    /// block has closed the way. Ananse never sets off along a path that doesn't reach the gourd.
    /// </summary>
    private void CheckPathToGourd()
    {
        pathCheckTimer = PathCheckSeconds;
        if (gourd == null || WalkGrid.Instance == null) return;

        bool found = WalkGrid.Instance.TryFindPath(transform.position, gourd.position, route, out bool reachesGourd);
        LogPathCheck(found, reachesGourd);

        if (found && reachesGourd)
        {
            followingRoute = true;
            routeIndex = 0;
        }
        else
        {
            StopRoute();
        }
    }

#if UNITY_EDITOR
    private string lastPathLog;
#endif

    /// <summary>Editor-only: reports when the auto-walk check changes its mind, to help diagnose levels.</summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogPathCheck(bool found, bool reachesGourd)
    {
#if UNITY_EDITOR
        string message;
        if (found && reachesGourd)
        {
            message = $"Auto-walk: clear path to the gourd ({route.Count} turns), walking.";
        }
        else if (found)
        {
            Vector3 gap = route[route.Count - 1] - gourd.position;
            gap.y = 0f;
            message = $"Auto-walk: no clear path yet; the furthest Ananse can get is {gap.magnitude:F2} m from the gourd.";
        }
        else
        {
            message = "Auto-walk: no clear path yet; Ananse is already as close to the gourd as he can get " +
                      "(or has no walkable spot; check the WalkGrid line and the Scene view overlay).";
        }

        if (message == lastPathLog) return;
        lastPathLog = message;
        Debug.Log(message);
#endif
    }

    /// <summary>Direction to the next point on the route; false when there is nowhere left to go.</summary>
    private bool TryGetRouteDirection(out Vector3 direction)
    {
        direction = Vector3.zero;
        float step = speed * Time.deltaTime;

        while (routeIndex < route.Count)
        {
            Vector3 toPoint = route[routeIndex] - transform.position;
            toPoint.y = 0f;

            if (toPoint.magnitude > Mathf.Max(arriveDistance, step))
            {
                direction = toPoint.normalized;
                return true;
            }

            routeIndex++;
        }

        StopRoute();
        return false;
    }

    private void CheckStuck(Vector3 before, float step)
    {
        Vector3 moved = transform.position - before;
        moved.y = 0f;

        stuckTimer = moved.magnitude < step * 0.25f ? stuckTimer + Time.deltaTime : 0f;
        if (stuckTimer > StuckSeconds) StopRoute();
    }

    private void StopRoute()
    {
        followingRoute = false;
        route.Clear();
        routeIndex = 0;
        stuckTimer = 0f;
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
