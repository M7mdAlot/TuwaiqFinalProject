using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIController : MonoBehaviour
{
    [Header("Main HUD (hidden during dialogue)")]
    public GameObject gameplayUIPanel;

    [Header("Health")]
    public SimpleHealth playerHealth;
    public Image healthFillImage;

    [Header("Ammo")]
    public AegisWeaponController weaponController;
    public TMP_Text ammoText;

    [Header("Timer / Bomb (hidden until the bomb event starts)")]
    public GameObject timerPanel;

    void Start()
    {
        if (timerPanel != null)
            timerPanel.SetActive(false);

        RefreshHealth();
        RefreshAmmo();
    }

    // Wire to DialogueManager.onDialogueOpened / onDialogueClosed in the Inspector.
    public void HideHUD()
    {
        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);
    }

    public void ShowHUD()
    {
        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(true);
    }

    // Wire to SimpleHealth.onHealthChanged in the Inspector.
    public void RefreshHealth()
    {
        if (playerHealth == null || healthFillImage == null) return;

        float pct = playerHealth.maxHealth > 0
            ? (float)playerHealth.currentHealth / playerHealth.maxHealth
            : 0f;

        healthFillImage.fillAmount = pct;
    }

    // Wire to AegisWeaponController.onAmmoChanged in the Inspector.
    public void RefreshAmmo()
    {
        if (weaponController == null || ammoText == null) return;

        ammoText.text = weaponController.CurrentAmmo + " / " + weaponController.MagazineSize;
    }

    // Wire to AegisStoryManager.onBombRunStarted in the Inspector.
    public void ShowTimer()
    {
        if (timerPanel != null)
            timerPanel.SetActive(true);
    }

    // Wire to AegisStoryManager.onBombDisabled / onBombExpired in the Inspector.
    public void HideTimer()
    {
        if (timerPanel != null)
            timerPanel.SetActive(false);
    }
}
