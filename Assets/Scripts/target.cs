using UnityEngine;

/// <summary>The gourd of wisdom. Reaching it wins the level.</summary>
public class target : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(GameConstants.Tags.Player)) return;

        // Too late if the timer has already run out.
        GameManager game = GameManager.Instance;
        if (game == null || !game.IsPlaying) return;

        game.TargetReached();
        Destroy(gameObject);
    }
}
