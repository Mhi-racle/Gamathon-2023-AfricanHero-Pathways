using UnityEngine;
using TMPro;

/// <summary>
/// Shows Ananse's intro conversation one line at a time. Gameplay is held in
/// <see cref="GameState.Intro"/> until the last line has been read.
/// </summary>
public class DialogManager : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public RectTransform backgroundBox;

    private Message[] currentMessages;
    private int activeMessage = 0;

    /// <summary>True while a conversation is on screen.</summary>
    public static bool isActive { get; private set; }

    private void Awake()
    {
        // Static state outlives the scene, so a conversation abandoned mid-way must not carry over.
        isActive = false;
    }

    private void OnDestroy()
    {
        isActive = false;
    }

    private void Start()
    {
        backgroundBox.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        // On phones a tap also raises the simulated mouse click, so one check covers both.
        if (isActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            NextMessage();
        }
    }

    public void OpenDialog(Message[] messages)
    {
        if (messages == null || messages.Length == 0) return;

        currentMessages = messages;
        activeMessage = 0;
        isActive = true;

        if (GameManager.Instance != null) GameManager.Instance.BeginDialogue();
        DisplayMessage();

        backgroundBox.LeanScale(Vector3.one, 0.5f).setEaseInOutExpo();
    }

    public void DisplayMessage()
    {
        messageText.text = currentMessages[activeMessage].message;
        AnimateTextColor();
    }

    /// <summary>Fades the new line in using LeanTween.</summary>
    private void AnimateTextColor()
    {
        LeanTween.textAlpha(messageText.rectTransform, 0, 0);
        LeanTween.textAlpha(messageText.rectTransform, 1, 0.5f);
    }

    public void NextMessage()
    {
        if (!isActive) return;

        activeMessage++;
        if (activeMessage < currentMessages.Length)
        {
            DisplayMessage();
            return;
        }

        isActive = false;
        EnableItems();
        backgroundBox.LeanScale(Vector3.zero, 0.5f).setEaseInOutExpo();
    }

    /// <summary>Hands control back to the player once the conversation ends.</summary>
    public void EnableItems()
    {
        if (GameManager.Instance != null) GameManager.Instance.EndDialogue();
    }
}
