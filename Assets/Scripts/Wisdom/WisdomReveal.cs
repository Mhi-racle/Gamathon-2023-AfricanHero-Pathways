using TMPro;
using UnityEngine;

/// <summary>
/// Shows the proverb held by this level's gourd. Lives on the win panel, which is switched on
/// when the gourd is collected, so the text is filled in each time the panel appears.
/// </summary>
public class WisdomReveal : MonoBehaviour
{
    [Tooltip("Shows the proverb in Twi.")]
    [SerializeField] private TextMeshProUGUI proverbText;

    [Tooltip("Shows the English translation and meaning.")]
    [SerializeField] private TextMeshProUGUI meaningText;

    private void OnEnable()
    {
        Proverb proverb = WisdomLibrary.ForLevel(LevelProgress.CurrentLevel());
        bool hasProverb = proverb != null;

        if (proverbText != null)
        {
            proverbText.gameObject.SetActive(hasProverb);
            if (hasProverb) proverbText.text = $"“{proverb.Twi}”";
        }

        if (meaningText != null)
        {
            meaningText.gameObject.SetActive(hasProverb);
            if (hasProverb) meaningText.text = FormatMeaning(proverb);
        }
    }

    /// <summary>Literal translation in italics, then the lesson. Shared with the collection screen.</summary>
    public static string FormatMeaning(Proverb proverb)
    {
        return $"<i>{proverb.Literal}</i>\n{proverb.Meaning}";
    }
}
