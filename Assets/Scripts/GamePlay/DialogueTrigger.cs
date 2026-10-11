using UnityEngine;

/// <summary>Opens the level's intro conversation as soon as the level starts.</summary>
public class DialogueTrigger : MonoBehaviour
{
    public Message[] messages;

    private void Start()
    {
        StartDialogue();
    }

    public void StartDialogue()
    {
        DialogManager dialogManager = FindAnyObjectByType<DialogManager>();
        if (dialogManager != null) dialogManager.OpenDialog(messages);
    }
}

/// <summary>One line of dialogue, edited in the Inspector.</summary>
[System.Serializable]
public class Message
{
    public string message;
}
