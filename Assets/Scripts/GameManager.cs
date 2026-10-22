using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// The phases a level moves through. Only <see cref="Playing"/> accepts gameplay input.
/// </summary>
public enum GameState
{
    Intro,
    Playing,
    Paused,
    Won,
    Lost
}

/// <summary>
/// Owns the flow of a single level: intro dialogue, play, pause, win and lose.
/// Other scripts ask <see cref="IsPlaying"/> instead of toggling each other on and off.
/// </summary>
[RequireComponent(typeof(FunnyRemarksManager))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject PauseMenu;
    public GameObject WinMenu;
    public GameObject FailMenu;
    public GameObject MainCanvas;

    public AudioClip congratulatorySound;
    public AudioSource audioSource;

    public GameObject player;

    [Tooltip("Show the on-screen joystick and pause button in the Editor, for testing mobile controls on PC.")]
    [SerializeField] private bool forceMobileControls = false;

    private PlayerController playerController;
    private RotateBlock rotateBlock;

    public GameState State { get; private set; } = GameState.Playing;

    /// <summary>True while the player may move Ananse and rotate blocks.</summary>
    public bool IsPlaying => State == GameState.Playing;

    /// <summary>
    /// Frame on which the state last changed. Input scripts skip that frame so the tap that
    /// closes a dialogue or menu doesn't also rotate a block.
    /// </summary>
    public int StateChangedFrame { get; private set; }

    /// <summary>True on phones/tablets (including the Device Simulator) or when forced in the Inspector.</summary>
    public bool UseMobileControls => UnityEngine.Device.Application.isMobilePlatform || forceMobileControls;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1;

        // Some blocks were authored without colliders; patch them before anyone can walk or tap.
        MazeColliderFixer.AddMissingBlockColliders();

        rotateBlock = GetComponent<RotateBlock>();
        if (player != null) playerController = player.GetComponent<PlayerController>();

        // Panels are set up in Awake so an intro dialogue opened from another Start() isn't overridden.
        SetActiveSafe(MainCanvas, true);
        SetActiveSafe(PauseMenu, false);
        SetActiveSafe(WinMenu, false);
        SetActiveSafe(FailMenu, false);

        GameObject mobileControls = GameObject.Find(GameConstants.Objects.MobileControls);

        // Menu buttons can lose their link to this GameManager when the shared prefabs are edited.
        MenuButtonRepair.Repair(this, WinMenu, FailMenu, PauseMenu, MainCanvas, mobileControls);

        SetActiveSafe(mobileControls, UseMobileControls);

        // The camera follows Ananse through Cinemachine. ThirdPersonCam would turn Ananse to face
        // away from the camera whenever a joystick exists, overriding PlayerController's facing,
        // so every instance is switched off rather than relying on finding it by name.
        foreach (ThirdPersonCam thirdPersonCam in FindObjectsByType<ThirdPersonCam>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            thirdPersonCam.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        // Escape on PC, the Back button on Android.
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (State == GameState.Playing) PauseGame();
        else if (State == GameState.Paused) ResumeGame();
    }

    /// <summary>Called by <see cref="DialogManager"/> when the intro conversation opens.</summary>
    public void BeginDialogue()
    {
        if (State != GameState.Playing) return;

        SetState(GameState.Intro);
        SetActiveSafe(MainCanvas, false);
    }

    /// <summary>Called by <see cref="DialogManager"/> when the last line has been read.</summary>
    public void EndDialogue()
    {
        if (State != GameState.Intro) return;

        SetState(GameState.Playing);
        SetActiveSafe(MainCanvas, true);
    }

    public void RestartGame()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoHome()
    {
        LoadScene(GameConstants.Scenes.MainMenu);
    }

    /// <summary>
    /// Loads the level after the current one, or returns to level selection after the last level.
    /// The argument is kept so existing button wiring still works, but it is ignored: older scenes
    /// passed a wrong or zero value, which used to wipe the player's progress.
    /// </summary>
    public void NextLevel(int unusedLevelArgument)
    {
        int next = LevelProgress.CurrentLevel() + 1;
        LoadScene(LevelProgress.Exists(next) ? LevelProgress.SceneNameFor(next) : GameConstants.Scenes.LevelSelection);
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused) return;

        SetState(GameState.Playing);
        SetActiveSafe(MainCanvas, true);
        SetActiveSafe(PauseMenu, false);
        if (PauseMenu != null && PauseMenu.TryGetComponent(out Animator animator)) animator.SetBool("paused", false);
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        if (State != GameState.Playing) return;

        SetState(GameState.Paused);
        FunnyRemarksManager.Instance.ShowPauseText();
        SetActiveSafe(MainCanvas, false);
        SetActiveSafe(PauseMenu, true);
        if (PauseMenu != null && PauseMenu.TryGetComponent(out Animator animator)) animator.SetBool("paused", true);
        Time.timeScale = 0;
    }

    /// <summary>Called by <see cref="Timer"/> when the countdown reaches zero.</summary>
    public void TimeUp()
    {
        if (State != GameState.Playing) return;

        SetState(GameState.Lost);
        FunnyRemarksManager.Instance.ShowLoseText();
        ShowEndPanel(FailMenu);
    }

    /// <summary>
    /// Called when Ananse reaches the gourd. Input and the timer stop and the next level is
    /// unlocked straight away; the win panel opens after <paramref name="panelDelay"/> seconds
    /// so the gourd's celebration can play first.
    /// </summary>
    public void TargetReached(float panelDelay = 0f)
    {
        if (State != GameState.Playing) return;

        SetState(GameState.Won);
        LevelProgress.Unlock(LevelProgress.CurrentLevel() + 1);

        FunnyRemarksManager.Instance.ShowWinText();
        if (audioSource != null && congratulatorySound != null) audioSource.PlayOneShot(congratulatorySound);

        if (panelDelay > 0f) StartCoroutine(ShowEndPanelAfter(WinMenu, panelDelay));
        else ShowEndPanel(WinMenu);
    }

    private IEnumerator ShowEndPanelAfter(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowEndPanel(panel);
    }

    private void ShowEndPanel(GameObject panel)
    {
        SetActiveSafe(MainCanvas, false);
        SetActiveSafe(panel, true);
        if (panel != null && panel.TryGetComponent(out Animator animator)) animator.SetTrigger("gameOver");
    }

    private void SetState(GameState newState)
    {
        State = newState;
        StateChangedFrame = Time.frameCount;

        // Movement and block rotation are only live while playing.
        bool playing = newState == GameState.Playing;
        if (playerController != null) playerController.enabled = playing;
        if (rotateBlock != null) rotateBlock.enabled = playing;
    }

    private static void LoadScene(string sceneName)
    {
        // A scene left from the pause menu would otherwise start frozen.
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }

    private static void SetActiveSafe(GameObject target, bool active)
    {
        if (target != null) target.SetActive(active);
    }
}
