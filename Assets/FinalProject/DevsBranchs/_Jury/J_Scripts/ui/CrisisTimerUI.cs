using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aegis.Systems;

// Shows the crisis (bomb / reactor) countdown on YOUR own HUD, reading Mohammed's
// CrisisManager directly each frame. No dependency on his UIManager being wired.
// Put this on a UI object and assign the CrisisManager + your timer panel/text/fill.
public class CrisisTimerUI : MonoBehaviour
{
    [Header("Source")]
    public CrisisManager crisisManager;

    [Header("Your UI")]
    [Tooltip("Panel shown only while the crisis is running.")]
    public GameObject timerPanel;
    [Tooltip("Countdown text, e.g. 00:45.")]
    public TMP_Text timerText;
    [Tooltip("Optional: an Image (Type = Filled) that drains full -> empty.")]
    public Image fillImage;

    void Start()
    {
        if (timerPanel != null) timerPanel.SetActive(false);
    }

    void Update()
    {
        if (crisisManager == null) return;

        bool active = crisisManager.HasTriggered && !crisisManager.IsResolved;

        if (timerPanel != null && timerPanel.activeSelf != active)
            timerPanel.SetActive(active);

        if (!active) return;

        float remaining = Mathf.Max(0f, crisisManager.TimeRemaining);
        float duration = crisisManager.Duration;

        if (timerText != null)
        {
            int s = Mathf.CeilToInt(remaining);
            timerText.text = (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
        }

        if (fillImage != null)
            fillImage.fillAmount = duration > 0f ? remaining / duration : 0f;
    }
}
