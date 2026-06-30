using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHoldInteractor : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerInput playerInput;
    public AegisWeaponController weaponController;

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
    public float defaultHoldTime = 5f;
    public bool drainProgressWhenReleased = true;
    public float drainSpeed = 2f;

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

        if (playerInput != null)
            interactAction = playerInput.actions.FindAction(interactActionName, false);

        if (loadFillRect != null)
            originalFillWidth = loadFillRect.rect.width;
    }

    void Start()
    {
        HidePrompt();
    }

    void Update()
    {
        if (InteractionIsBlocked())
        {
            currentInteractable = null;
            holdTimer = 0f;
            HidePrompt();
            return;
        }

        FindInteractable();
        HandleHoldInteraction();
    }

    bool InteractionIsBlocked()
    {
        return weaponController != null && weaponController.BlocksInteraction;
    }

    void FindInteractable()
    {
        SimpleInteractable foundInteractable = null;

        if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayers))
                foundInteractable = hit.collider.GetComponentInParent<SimpleInteractable>();
        }

        if (foundInteractable != currentInteractable)
        {
            currentInteractable = foundInteractable;
            holdTimer = 0f;
            SetLoadAmount(0f);
        }

        if (currentInteractable != null && currentInteractable.CanInteract())
            ShowPrompt(currentInteractable.GetPromptText());
        else
            HidePrompt();
    }

    void HandleHoldInteraction()
    {
        if (currentInteractable == null) return;
        if (!currentInteractable.CanInteract()) return;

        bool holdingInteract = interactAction != null && interactAction.IsPressed();

        float neededTime = currentInteractable.GetHoldTime(defaultHoldTime);

        if (holdingInteract)
        {
            holdTimer += Time.deltaTime;
            SetLoadAmount(holdTimer / neededTime);

            if (holdTimer >= neededTime)
            {
                currentInteractable.Interact(gameObject);

                holdTimer = 0f;
                SetLoadAmount(0f);
                HidePrompt();
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

    void ShowPrompt(string text)
    {
        if (interactPromptRoot != null && !interactPromptRoot.activeSelf)
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