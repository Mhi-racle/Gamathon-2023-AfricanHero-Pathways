using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// The Gourds of Wisdom screen: lists every level's proverb, with the ones not yet earned
/// shown as locked. Rows are cloned from a single template, so the layout is designed once
/// in the Editor and the list refreshes every time the panel opens.
/// </summary>
public class WisdomCollectionPanel : MonoBehaviour
{
    [Tooltip("A row inside the scroll view's Content. It is hidden and used as the pattern for every row.")]
    [SerializeField] private WisdomEntryView entryTemplate;

    [Tooltip("Optional. Shows e.g. \"4 / 15 gourds of wisdom gathered\".")]
    [SerializeField] private TextMeshProUGUI progressText;

    private readonly List<WisdomEntryView> entries = new List<WisdomEntryView>();

    private void OnEnable()
    {
        if (entryTemplate == null) return;

        entryTemplate.gameObject.SetActive(false);
        Transform content = entryTemplate.transform.parent;

        for (int level = 1; level <= WisdomLibrary.Count; level++)
        {
            if (entries.Count < level)
            {
                entries.Add(Instantiate(entryTemplate, content));
            }

            WisdomEntryView entry = entries[level - 1];
            entry.gameObject.SetActive(true);
            entry.Show(level, WisdomLibrary.ForLevel(level), WisdomLibrary.IsCollected(level));
        }

        if (progressText != null)
        {
            progressText.text = $"{WisdomLibrary.CollectedCount()} / {WisdomLibrary.Count} gourds of wisdom gathered";
        }
    }
}
