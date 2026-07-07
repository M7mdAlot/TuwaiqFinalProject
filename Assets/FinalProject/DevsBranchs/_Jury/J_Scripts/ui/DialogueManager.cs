using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("References")]
    public PlayerInput playerInput;
    public TinyToasterController movementController;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text speakerNameText;
    public TMP_Text dialogueBodyText;
    public TMP_Text continueText;

    [Header("Events")]
    public UnityEvent onDialogueOpened;
    public UnityEvent onDialogueClosed;

    [Header("Input")]
    public string interactActionName = "Interact";

    [Header("Settings")]
    public bool lockMovementDuringDialogue = true;
    public float startInputDelay = 0.25f;

    private InputAction interactAction;

    private List<DialogueLine> currentLines = new List<DialogueLine>();
    private int currentIndex;
    private bool isDialogueOpen;
    private float nextInputAllowedTime;

    private struct DialogueLine
    {
        public string speaker;
        public string text;

        public DialogueLine(string speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }
    }

    void Awake()
    {
        // A duplicate "manager" in a gameplay scene would otherwise grab Instance and then be
        // destroyed (the persistent Main-Menu manager wins), leaving Instance pointing at a
        // destroyed object == null. Don't overwrite Instance if a live one already owns it.
        if (Instance != null && Instance != this)
            return;

        Instance = this;

        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (movementController == null)
            movementController = FindFirstObjectByType<TinyToasterController>();

        if (playerInput != null)
            interactAction = playerInput.actions.FindAction(interactActionName, false);
    }

    void Start()
    {
        CloseDialogueInstant();
    }

    void Update()
    {
        if (!isDialogueOpen) return;
        if (Time.time < nextInputAllowedTime) return;

        // Lazily grab the Interact action (this manager may have woken in the Main Menu
        // before any player existed, so interactAction can be null here).
        if (interactAction == null)
        {
            if (playerInput == null)
                playerInput = FindFirstObjectByType<PlayerInput>();
            if (playerInput != null)
                interactAction = playerInput.actions.FindAction(interactActionName, false);
        }

        bool advance = false;

        if (interactAction != null && interactAction.WasPressedThisFrame())
            advance = true;

        // Direct fallbacks so the line always advances (E / Space / left-click).
        if (Keyboard.current != null &&
            (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            advance = true;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            advance = true;

        if (advance)
            ShowNextLine();
    }

    public void StartDialogue(string speakerName, string[] lines)
    {
        if (lines == null || lines.Length == 0)
            return;

        currentLines.Clear();

        foreach (string line in lines)
            currentLines.Add(new DialogueLine(speakerName, line));

        OpenDialogue();
    }

    public void StartDialogueLines(string[] rawLines)
    {
        if (rawLines == null || rawLines.Length == 0)
            return;

        currentLines.Clear();

        foreach (string rawLine in rawLines)
        {
            DialogueLine parsedLine = ParseLine(rawLine);
            currentLines.Add(parsedLine);
        }

        OpenDialogue();
    }

    DialogueLine ParseLine(string rawLine)
    {
        if (string.IsNullOrWhiteSpace(rawLine))
            return new DialogueLine("", "");

        int colonIndex = rawLine.IndexOf(':');

        if (colonIndex > 0)
        {
            string speaker = rawLine.Substring(0, colonIndex).Trim();
            string text = rawLine.Substring(colonIndex + 1).Trim();

            return new DialogueLine(speaker, text);
        }

        return new DialogueLine("", rawLine.Trim());
    }

    void OpenDialogue()
    {
        currentIndex = 0;
        isDialogueOpen = true;
        nextInputAllowedTime = Time.time + startInputDelay;

        Debug.Log("DIALOGUE OpenDialogue: lines=" + currentLines.Count
            + " | dialoguePanel=" + (dialoguePanel != null)
            + " | bodyText=" + (dialogueBodyText != null)
            + " | speakerText=" + (speakerNameText != null), this);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (continueText != null)
            continueText.text = "Press E to continue";

        if (lockMovementDuringDialogue && movementController != null)
            movementController.SetMovementLocked(true);

        onDialogueOpened?.Invoke();

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        if (currentLines.Count == 0) return;

        DialogueLine line = currentLines[currentIndex];

        if (speakerNameText != null)
            speakerNameText.text = line.speaker;

        if (dialogueBodyText != null)
            dialogueBodyText.text = line.text;
    }

    void ShowNextLine()
    {
        currentIndex++;

        if (currentIndex >= currentLines.Count)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    public void EndDialogue()
    {
        isDialogueOpen = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (lockMovementDuringDialogue && movementController != null)
            movementController.SetMovementLocked(false);

        onDialogueClosed?.Invoke();
    }

    void CloseDialogueInstant()
    {
        isDialogueOpen = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public bool IsDialogueOpen()
    {
        return isDialogueOpen;
    }

    // Compatibility methods for other scripts / CampaignManager
    public void SetDialogue(string speakerName, string[] lines)
    {
        StartDialogue(speakerName, lines);
    }

    public void SetDialogue(string[] lines)
    {
        StartDialogueLines(lines);
    }

    public void SetDialogue(string speakerName, string line)
    {
        StartDialogue(speakerName, new string[] { line });
    }

    public void SetDialogue(string line)
    {
        StartDialogueLines(new string[] { line });
    }

    public void SetDialogue(List<string> lines)
    {
        StartDialogueLines(lines.ToArray());
    }

    public void SetDialogue(string speakerName, List<string> lines)
    {
        StartDialogue(speakerName, lines.ToArray());
    }

    public void SetDialogue(object data)
    {
        Debug.LogWarning("SetDialogue was called with unsupported data type: " + data);
    }

    public void SetDialogue(object data1, object data2)
    {
        Debug.LogWarning("SetDialogue was called with unsupported data types: " + data1 + ", " + data2);
    }
}