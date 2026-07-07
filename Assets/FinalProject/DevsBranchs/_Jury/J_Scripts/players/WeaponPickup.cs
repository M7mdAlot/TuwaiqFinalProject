using UnityEngine;
using Aegis.Core;

// Implements Aegis.Core.IInteractable additively (bridge only) so Mohammed's
// InteractionController can pick this up too. The existing SimpleInteractable +
// PlayerHoldInteractor flow (Inspector-wired to PickupWeapon()) is untouched.
public class WeaponPickup : MonoBehaviour, IInteractable
{
    public AegisStoryManager storyManager;
    public AegisWeaponController weaponController;

    [Header("Objects")]
    public GameObject pickupModelToHide;

    [Header("Settings")]
    public bool disablePickupAfterUse = true;

    public string Prompt => "Hold F to pick up weapon";
    public float HoldDuration => 0f;
    public bool CanInteract => gameObject.activeInHierarchy;

    void IInteractable.Interact(GameObject interactor)
    {
        PickupWeapon();
    }

    public void PickupWeapon()
    {
        if (weaponController != null)
            weaponController.enabled = true;

        if (storyManager != null)
            storyManager.OnWeaponPickedUp();

        if (pickupModelToHide != null)
            pickupModelToHide.SetActive(false);

        if (disablePickupAfterUse)
            gameObject.SetActive(false);

        Debug.Log("Aegis weapon picked up.");
    }
}