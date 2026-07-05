using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIController : MonoBehaviour
{
    [Header("Gameplay Panels")]
    public GameObject gameplayUIPanel;
    public GameObject deathPanel;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject dialoguePanel;

    [Header("Health UI")]
    public Image healthFillImage;
    public RectTransform healthFillRect;
    public TMP_Text healthText;

    [Header("Damage UI")]
    public GameObject damageOverlay;
    public float damageOverlayDuration = 1f;

    private float originalHealthWidth;

    void Awake()
    {
        if (healthFillRect != null)
        {
            originalHealthWidth = healthFillRect.sizeDelta.x;

            if (originalHealthWidth <= 0f)
                originalHealthWidth = healthFillRect.rect.width;

            if (originalHealthWidth <= 0f)
                originalHealthWidth = 300f;
        }
    }

    void Start()
    {
        ShowGameplayUI();

        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (damageOverlay != null)
            damageOverlay.SetActive(false);
    }

    public void ShowGameplayUI()
    {
        Time.timeScale = 1f;

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(true);

        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        float amount = 0f;

        if (maxHealth > 0)
            amount = (float)currentHealth / maxHealth;

        amount = Mathf.Clamp01(amount);

        // ينقص صورة الهيلث بار إذا كانت Image Type = Filled
        if (healthFillImage != null)
            healthFillImage.fillAmount = amount;

        // ينقص عرض صورة الهيلث بار إذا كانت صورة عادية
        if (healthFillRect != null)
        {
            healthFillRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                originalHealthWidth * amount
            );
        }

        // يغير نص الدم
        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }

    public void ShowDamageOverlay()
    {
        if (damageOverlay == null) return;

        damageOverlay.SetActive(true);

        CancelInvoke(nameof(HideDamageOverlay));
        Invoke(nameof(HideDamageOverlay), damageOverlayDuration);
    }

    void HideDamageOverlay()
    {
        if (damageOverlay != null)
            damageOverlay.SetActive(false);
    }

    public void ShowDeathPanel()
    {
        Time.timeScale = 0f;

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (deathPanel != null)
            deathPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}