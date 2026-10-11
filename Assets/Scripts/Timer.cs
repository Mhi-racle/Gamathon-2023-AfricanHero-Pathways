using UnityEngine;
using TMPro;

/// <summary>
/// Level countdown. Only runs while the game is <see cref="GameState.Playing"/>, so the intro
/// dialogue and the pause menu don't eat into the player's time.
/// </summary>
public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    [Tooltip("Seconds the player has to reach the gourd.")]
    public int timeLeft = 50;

    private float remaining;
    private int shownSeconds = -1;
    private bool expired;

    private void Start()
    {
        remaining = timeLeft;
    }

    private void Update()
    {
        if (expired || GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        if (timerText != null && !timerText.gameObject.activeSelf) timerText.gameObject.SetActive(true);

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            expired = true;
            if (timerText != null) timerText.text = "Time is up";
            GameManager.Instance.TimeUp();
            return;
        }

        // Only rebuild the label when the visible number changes, to avoid a string allocation per frame.
        int seconds = Mathf.CeilToInt(remaining);
        if (seconds == shownSeconds || timerText == null) return;

        shownSeconds = seconds;
        timerText.text = seconds + "s";
    }
}
