using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Aegis.Player;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    /// <summary>True while any dialogue is on screen — NPCs read this to freeze in place.</summary>
    public static bool DialogueActive { get; private set; }

    private MovementController[] frozenMovers;

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

        // Direct fallbacks so the line always advances. NOT left-click — that's Fire and
        // would conflict (and could shoot while you're advancing dialogue).
        if (Keyboard.current != null &&
            (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
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

    // If the panel/text fields weren't wired in the Inspector, find them by name at runtime
    // (searches everything, including inactive objects and the persistent Canvas). This
    // removes the fragile cross-scene wiring that kept breaking.
    void EnsureUI()
    {
        if (dialoguePanel != null && dialogueBodyText != null && speakerNameText != null)
            return;

        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Transform t in all)
        {
            string n = t.name.ToLower();

            if (dialoguePanel == null && n.Contains("panel") && n.Contains("dialo"))
                dialoguePanel = t.gameObject;

            if (speakerNameText == null && n.Contains("names"))
            {
                TMP_Text tmp = t.GetComponent<TMP_Text>();
                if (tmp != null) speakerNameText = tmp;
            }

            if (dialogueBodyText == null && n.Contains("dialouge") && n.Contains("text"))
            {
                TMP_Text tmp = t.GetComponent<TMP_Text>();
                if (tmp != null) dialogueBodyText = tmp;
            }
        }
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

        EnsureUI(); // auto-find the dialogue panel/text if they weren't wired in the Inspector

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

        DialogueActive = true;
        FreezePlayers(true);

        onDialogueOpened?.Invoke();

        ShowCurrentLine();
    }

    // Freeze/unfreeze the player(s) by toggling Mohammed's MovementController.
    void FreezePlayers(bool freeze)
    {
        if (freeze)
        {
            frozenMovers = FindObjectsByType<MovementController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (MovementController m in frozenMovers)
                if (m != null) m.enabled = false;
        }
        else if (frozenMovers != null)
        {
            foreach (MovementController m in frozenMovers)
                if (m != null) m.enabled = true;
            frozenMovers = null;
        }
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

        DialogueActive = false;
        FreezePlayers(false);

        onDialogueClosed?.Invoke();
    }

    void CloseDialogueInstant()
    {
        isDialogueOpen = false;
        DialogueActive = false;

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