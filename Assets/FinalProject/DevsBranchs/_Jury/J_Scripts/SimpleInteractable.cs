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
        EquipToSocket,
        UnityEventOnly
    }

    [Header("Prompt")]
    public string promptText = "Press E to interact";
    public float holdTime = 5f;

    [Header("Interaction")]
    public InteractionType interactionType = InteractionType.UnityEventOnly;
    public bool oneTimeOnly = true;

    [Header("Target")]
    public GameObject targetObject;

    [Header("Equip Settings")]
    public Transform equipSocket;
    public Vector3 equippedLocalPosition;
    public Vector3 equippedLocalEulerAngles;
    public Vector3 equippedLocalScale = Vector3.one;

    [Header("After Interaction")]
    public bool disableCollidersAfterInteract = true;
    public bool disableThisObjectAfterInteract = false;

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

            case InteractionType.EquipToSocket:
                EquipObject(actualTarget);
                break;

            case InteractionType.UnityEventOnly:
                break;
        }

        onInteract?.Invoke();

        if (disableCollidersAfterInteract)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
                col.enabled = false;
        }

        if (disableThisObjectAfterInteract && interactionType != InteractionType.DestroySelf)
            gameObject.SetActive(false);
    }

    void EquipObject(GameObject objectToEquip)
    {
        if (objectToEquip == null) return;
        if (equipSocket == null) return;

        objectToEquip.SetActive(true);
        objectToEquip.transform.SetParent(equipSocket);

        objectToEquip.transform.localPosition = equippedLocalPosition;
        objectToEquip.transform.localRotation = Quaternion.Euler(equippedLocalEulerAngles);
        objectToEquip.transform.localScale = equippedLocalScale;

        Rigidbody rb = objectToEquip.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}