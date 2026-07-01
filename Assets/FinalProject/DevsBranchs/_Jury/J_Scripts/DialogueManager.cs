using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Input")]
    public string interactActionName = "Interact";

    [Header("Settings")]
    public bool lockMovementDuringDialogue = true;
    public float startInputDelay = 0.25f;

    private InputAction interactAction;

    private string[] currentLines;
    private string currentSpeaker;
    private int currentIndex;
    private bool isDialogueOpen;
    private float nextInputAllowedTime;

    void Awake()
    {
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
        if (interactAction == null) return;
        if (Time.time < nextInputAllowedTime) return;

        if (interactAction.WasPressedThisFrame())
            ShowNextLine();
    }

    public void StartDialogue(string speakerName, string[] lines)
    {
        if (lines == null || lines.Length == 0)
            return;

        currentSpeaker = speakerName;
        currentLines = lines;
        currentIndex = 0;

        isDialogueOpen = true;
        nextInputAllowedTime = Time.time + startInputDelay;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (speakerNameText != null)
            speakerNameText.text = currentSpeaker;

        if (continueText != null)
            continueText.text = "Press E to continue";

        if (lockMovementDuringDialogue && movementController != null)
            movementController.SetMovementLocked(true);

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        if (dialogueBodyText != null)
            dialogueBodyText.text = currentLines[currentIndex];
    }

    void ShowNextLine()
    {
        currentIndex++;

        if (currentIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    void EndDialogue()
    {
        isDialogueOpen = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (lockMovementDuringDialogue && movementController != null)
            movementController.SetMovementLocked(false);
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
}