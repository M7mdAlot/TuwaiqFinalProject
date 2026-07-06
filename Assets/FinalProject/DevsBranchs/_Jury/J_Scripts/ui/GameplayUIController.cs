using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Aegis.Systems;

// Bridges Jury's own health/ammo events into Mohammed's persistent Aegis.Systems.UIManager
// (a DontDestroyOnLoad singleton), so the same HUD keeps working no matter which scene the
// player ends up in. The local fields below still work as a fallback if UIManager.Instance
// isn't loaded yet in whatever scene this is sitting in.
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
        if (playerHealth == null) return;

        float pct = playerHealth.maxHealth > 0
            ? (float)playerHealth.currentHealth / playerHealth.maxHealth
            : 0f;

        if (UIManager.Instance != null)
            UIManager.Instance.SetHealthNormalized(pct);

        if (healthFillImage != null)
            healthFillImage.fillAmount = pct;
    }

    // Wire to AegisWeaponController.onAmmoChanged in the Inspector.
    public void RefreshAmmo()
    {
        if (weaponController == null) return;

        if (UIManager.Instance != null)
            UIManager.Instance.SetAmmo(weaponController.CurrentAmmo, weaponController.MagazineSize);

        if (ammoText != null)
            ammoText.text = weaponController.CurrentAmmo + " / " + weaponController.MagazineSize;
    }

    // NOTE: if this scene also uses Mohammed's CrisisManager, that system already drives
    // UIManager.SetCrisisTimer/HideCrisisTimer end-to-end — don't wire both to the same event
    // or the timer text will fight itself. Only use these two if you're NOT using CrisisManager.

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

        if (UIManager.Instance != null)
            UIManager.Instance.HideCrisisTimer();
    }
}
