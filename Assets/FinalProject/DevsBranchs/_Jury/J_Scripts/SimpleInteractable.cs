using UnityEngine;
using UnityEngine.Events;

public class SimpleInteractable : MonoBehaviour
{
    public enum InteractionType
    {
        DestroySelf,
        ActivateTarget,
        DeactivateTarget,
        ToggleTarget,
        UnityEventOnly
    }

    [Header("Prompt")]
    public string promptText = "Hold E to interact";
    public float holdTime = 5f;

    [Header("Interaction")]
    public InteractionType interactionType = InteractionType.UnityEventOnly;
    public bool oneTimeOnly = true;
    public GameObject targetObject;

    [Header("Events")]
    public UnityEvent onInteract;

    private bool hasInteracted;

    public bool CanInteract()
    {
        if (oneTimeOnly && hasInteracted)
            return false;

        return true;
    }

    public string GetPromptText()
    {
        return promptText;
    }

    public float GetHoldTime(float defaultHoldTime)
    {
        if (holdTime <= 0f)
            return defaultHoldTime;

        return holdTime;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract()) return;

        hasInteracted = true;

        GameObject actualTarget = targetObject != null ? targetObject : gameObject;

        onInteract?.Invoke();

        switch (interactionType)
        {
            case InteractionType.DestroySelf:
                Destroy(actualTarget);
                break;

            case InteractionType.ActivateTarget:
                actualTarget.SetActive(true);
                break;

            case InteractionType.DeactivateTarget:
                actualTarget.SetActive(false);
                break;

            case InteractionType.ToggleTarget:
                actualTarget.SetActive(!actualTarget.activeSelf);
                break;

            case InteractionType.UnityEventOnly:
                break;
        }
    }
}