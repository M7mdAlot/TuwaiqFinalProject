using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public AegisStoryManager storyManager;
    public AegisWeaponController weaponController;

    [Header("Objects")]
    public GameObject pickupModelToHide;

    [Header("Settings")]
    public bool disablePickupAfterUse = true;

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