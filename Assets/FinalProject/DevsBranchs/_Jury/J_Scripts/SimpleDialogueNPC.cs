using UnityEngine;

public class SimpleDialogueNPC : MonoBehaviour
{
    [Header("Dialogue")]
    public string speakerName = "Scientist";

    [TextArea(2, 5)]
    public string[] dialogueLines;

    public void StartDialogue()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("No DialogueManager found in scene.");
            return;
        }

        DialogueManager.Instance.StartDialogue(speakerName, dialogueLines);
    }
}