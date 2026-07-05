using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHoldInteractor : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerInput playerInput;
    public AegisWeaponController weaponController;
    public Animator animator;

    [Header("UI")]
    public GameObject interactPromptRoot;
    public TMP_Text interactPromptText;

    [Header("Loading Bar")]
    public RectTransform loadFillRect;

    [Header("Input")]
    public string interactActionName = "Interact";

    [Header("Detection")]
    public float interactRange = 3f;
    public LayerMask interactLayers = ~0;

    [Header("Hold Settings")]
    public float defaultHoldTime = 0f;
    public bool drainProgressWhenReleased = true;
    public float drainSpeed = 2f;

    [Header("Animation")]
    public bool playInteractAnimation = true;
    public string interactTriggerParam = "Interact";

    [Header("Debug")]
    public bool debugLogs = true;

    private InputAction interactAction;
    private SimpleInteractable currentInteractable;

    private float holdTimer;
    private float originalFillWidth;

    void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (weaponController == null)
            weaponController = GetComponent<AegisWeaponController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerHoldInteractor: Missing PlayerInput.");
            enabled = false;
            return;
        }

        interactAction = playerInput.actions.FindAction(interactActionName, false);

        if (interactAction == null)
            Debug.LogError("PlayerHoldInteractor: Missing Input Action: " + interactActionName);

        if (loadFillRect != null)
        {
            originalFillWidth = loadFillRect.sizeDelta.x;

            if (originalFillWidth <= 0f)
                originalFillWidth = loadFillRect.rect.width;

            if (originalFillWidth <= 0f)
                originalFillWidth = 200f;
        }
    }

    void Start()
    {
        HidePrompt();
    }

    void Update()
    {
        if (IsBlocked())
        {
            currentInteractable = null;
            holdTimer = 0f;
            HidePrompt();
            return;
        }

        FindInteractable();
        HandleInteraction();
    }

    bool IsBlocked()
    {
        return weaponController != null && weaponController.BlocksInteraction;
    }

    void FindInteractable()
    {
        SimpleInteractable found = null;

        if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayers, QueryTriggerInteraction.Collide))
            {
                found = hit.collider.GetComponentInParent<SimpleInteractable>();

                if (found != null && found != currentInteractable && debugLogs)
                    Debug.Log("LOOKING AT INTERACTABLE: " + found.name);
            }
        }

        if (found != currentInteractable)
        {
            currentInteractable = found;
            holdTimer = 0f;
            SetLoadAmount(0f);
        }

        if (currentInteractable != null && currentInteractable.CanInteract())
            ShowPrompt(currentInteractable.GetPromptText());
        else
            HidePrompt();
    }

    void HandleInteraction()
    {
        if (currentInteractable == null) return;
        if (!currentInteractable.CanInteract()) return;
        if (interactAction == null) return;

        bool pressedThisFrame = interactAction.WasPressedThisFrame();
        bool holding = interactAction.IsPressed();

        float neededTime = currentInteractable.GetHoldTime(defaultHoldTime);

        if (neededTime <= 0.05f)
        {
            if (pressedThisFrame)
            {
                PlayInteractAnimation();
                CompleteInteraction();
            }

            return;
        }

        if (holding)
        {
            holdTimer += Time.deltaTime;
            SetLoadAmount(holdTimer / neededTime);

            if (holdTimer >= neededTime)
            {
                PlayInteractAnimation();
                CompleteInteraction();
            }
        }
        else
        {
            if (drainProgressWhenReleased)
                holdTimer = Mathf.MoveTowards(holdTimer, 0f, drainSpeed * Time.deltaTime);
            else
                holdTimer = 0f;

            SetLoadAmount(holdTimer / neededTime);
        }
    }

    void PlayInteractAnimation()
    {
        if (!playInteractAnimation) return;
        if (animator == null) return;

        animator.ResetTrigger(interactTriggerParam);
        animator.SetTrigger(interactTriggerParam);

        if (debugLogs)
            Debug.Log("INTERACT ANIMATION TRIGGERED");
    }

    void CompleteInteraction()
    {
        if (debugLogs)
            Debug.Log("INTERACTED WITH: " + currentInteractable.name);

        currentInteractable.Interact(gameObject);

        currentInteractable = null;
        holdTimer = 0f;

        SetLoadAmount(0f);
        HidePrompt();
    }

    void ShowPrompt(string text)
    {
        if (interactPromptRoot != null)
            interactPromptRoot.SetActive(true);

        if (interactPromptText != null)
            interactPromptText.text = text;
    }

    void HidePrompt()
    {
        if (interactPromptRoot != null)
            interactPromptRoot.SetActive(false);

        SetLoadAmount(0f);
    }

    void SetLoadAmount(float value)
    {
        if (loadFillRect == null) return;

        value = Mathf.Clamp01(value);

        loadFillRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            originalFillWidth * value
        );
    }
}