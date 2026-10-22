using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Shows the proverb held by this level's gourd. Lives on the win panel, which is switched on
/// when the gourd is collected. Ananse's remark (from <see cref="FunnyRemarksManager"/>) is
/// read first; the proverb then fades in beneath it as the reward.
/// </summary>
public class WisdomReveal : MonoBehaviour
{
    [Tooltip("Shows the proverb in Twi.")]
    [SerializeField] private TextMeshProUGUI proverbText;

    [Tooltip("Shows the English translation and meaning.")]
    [SerializeField] private TextMeshProUGUI meaningText;

    [Tooltip("Optional. Fades everything under it together (e.g. a 'Wisdom gathered' heading and divider). " +
             "If empty, only the two texts above fade.")]
    [SerializeField] private CanvasGroup fadeGroup;

    [Tooltip("Seconds after the win panel opens before the proverb starts to appear.")]
    [SerializeField] private float revealDelay = 1.2f;

    [SerializeField] private float fadeDuration = 0.6f;

    private void OnEnable()
    {
        Proverb proverb = WisdomLibrary.ForLevel(LevelProgress.CurrentLevel());
        bool hasProverb = proverb != null;

        if (fadeGroup != null) fadeGroup.gameObject.SetActive(hasProverb);
        if (proverbText != null) proverbText.gameObject.SetActive(hasProverb);
        if (meaningText != null) meaningText.gameObject.SetActive(hasProverb);
        if (!hasProverb) return;

        if (proverbText != null) proverbText.text = $"“{proverb.Twi}”";
        if (meaningText != null) meaningText.text = FormatMeaning(proverb);

        SetAlpha(0f);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(revealDelay);

        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            SetAlpha(t / fadeDuration);
            yield return null;
        }

        SetAlpha(1f);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeGroup != null)
        {
            fadeGroup.alpha = alpha;
            return;
        }

        if (proverbText != null) proverbText.alpha = alpha;
        if (meaningText != null) meaningText.alpha = alpha;
    }

    /// <summary>Literal translation in italics, then the lesson. Shared with the collection screen.</summary>
    public static string FormatMeaning(Proverb proverb)
    {
        return $"<i>{proverb.Literal}</i>\n{proverb.Meaning}";
    }
}
