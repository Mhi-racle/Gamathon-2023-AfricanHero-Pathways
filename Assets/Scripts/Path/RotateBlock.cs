using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Turns the maze block the player taps (touch) or clicks (mouse) by 90°.
/// Taps on UI (joystick, pause button, menus) are ignored so they never rotate a block behind them.
/// </summary>
public class RotateBlock : MonoBehaviour
{
    public static RotateBlock Instance { get; private set; }

    [Tooltip("Seconds a block takes to complete a quarter turn.")]
    [SerializeField] private float turnDuration = 0.2f;

    [Tooltip("Refuse to turn the block Ananse is standing on, so he can't get trapped inside a wall.")]
    [SerializeField] private bool protectPlayerBlock = true;

    [SerializeField] private float maxTapDistance = 500f;

    public AudioSource speaker;
    public AudioClip swoosh;

    /// <summary>Seconds a quarter turn takes; shared with <see cref="AutoRotation"/>.</summary>
    public float TurnDuration => turnDuration;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        GameManager game = GameManager.Instance;
        if (game == null || !game.IsPlaying || Time.frameCount <= game.StateChangedFrame) return;

        if (Input.touchCount > 0)
        {
            // Handle every new finger, so a block can be tapped while the other thumb holds the joystick.
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && !IsOverUI(touch.fingerId)) TryRotateAt(touch.position);
            }
        }
        else if (Input.GetMouseButtonDown(0) && !IsOverUI(-1))
        {
            TryRotateAt(Input.mousePosition);
        }
    }

    private void TryRotateAt(Vector2 screenPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPosition);
        // Ignore trigger volumes (like the gourd) so they don't block taps on the blocks underneath.
        if (!Physics.Raycast(ray, out RaycastHit hit, maxTapDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return;
        if (!hit.transform.CompareTag(GameConstants.Tags.Block)) return;

        BlockSpin spin = BlockSpin.For(hit.transform.gameObject);
        if (protectPlayerBlock && IsPlayerOn(spin))
        {
            spin.Nudge();
            return;
        }

        if (spin.TryTurn(turnDuration) && speaker != null && swoosh != null)
        {
            speaker.PlayOneShot(swoosh);
        }
    }

    /// <summary>True if Ananse is standing over <paramref name="spin"/>'s block.</summary>
    public static bool IsPlayerOn(BlockSpin spin)
    {
        GameManager game = GameManager.Instance;
        return game != null && game.player != null && spin.Covers(game.player.transform.position);
    }

    private static bool IsOverUI(int pointerId)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;

        return pointerId < 0 ? eventSystem.IsPointerOverGameObject() : eventSystem.IsPointerOverGameObject(pointerId);
    }
}
