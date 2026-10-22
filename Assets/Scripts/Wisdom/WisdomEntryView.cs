using TMPro;
using UnityEngine;

/// <summary>One row in the Gourds of Wisdom collection: a proverb, or a locked placeholder.</summary>
public class WisdomEntryView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Tooltip("Optional. Shown only while the proverb is still locked, e.g. a padlock or a closed gourd.")]
    [SerializeField] private GameObject lockedIcon;

    public void Show(int level, Proverb proverb, bool collected)
    {
        if (lockedIcon != null) lockedIcon.SetActive(!collected);

        if (collected)
        {
            titleText.text = $"{level}. “{proverb.Twi}”";
            bodyText.text = WisdomReveal.FormatMeaning(proverb);
        }
        else
        {
            titleText.text = $"{level}. ?";
            bodyText.text = $"Collect the gourd in Level {level} to reveal this wisdom.";
        }
    }
}
