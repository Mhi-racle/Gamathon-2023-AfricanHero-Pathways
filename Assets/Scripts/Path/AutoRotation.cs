using UnityEngine;

/// <summary>
/// A block that turns by itself at a steady rhythm, used in later levels as a timing challenge.
/// Auto-rotating blocks are untagged so the player can't tap them.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AutoRotation : MonoBehaviour
{
    [Tooltip("Seconds between turns.")]
    [SerializeField] private float interval = 1f;

    private AudioSource speaker;
    private BlockSpin spin;

    private void Start()
    {
        speaker = GetComponent<AudioSource>();
        spin = BlockSpin.For(gameObject);
        if (RotateBlock.Instance != null) speaker.clip = RotateBlock.Instance.swoosh;

        gameObject.tag = "Untagged";
        InvokeRepeating(nameof(AutoRotate), interval, interval);
    }

    private void AutoRotate()
    {
        GameManager game = GameManager.Instance;
        if (game == null || !game.IsPlaying) return;

        float duration = RotateBlock.Instance != null ? RotateBlock.Instance.TurnDuration : 0.2f;
        if (spin.TryTurn(duration)) speaker.Play();
    }
}
