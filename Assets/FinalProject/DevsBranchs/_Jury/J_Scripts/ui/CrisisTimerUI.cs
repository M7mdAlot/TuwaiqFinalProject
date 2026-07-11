using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aegis.Systems;

// Shows the reactor/crisis countdown on your HUD, reading Mohammed's CrisisManager. Auto-finds
// the CrisisManager and the timer panel/text/fill by name — just drop it on any UI object.
public class CrisisTimerUI : MonoBehaviour
{
    [Header("Optional — left empty, all are auto-found")]
    public CrisisManager crisisManager;
    public GameObject timerPanel;   // auto-finds "TIMER BG"
    public TMP_Text timerText;      // auto-finds "TIMER TEXT"
    public Image fillImage;         // optional filled Image that drains

    void Start()
    {
        Debug.Log("CrisisTimerUI: CrisisManager found=" + (FindFirstObjectByType<CrisisManager>() != null)
            + ", 'TIMER BG' found=" + (FindByName("TIMER BG") != null)
            + ", 'TIMER TEXT' found=" + (FindByName("TIMER TEXT") != null), this);
    }

    void Update()
    {
        if (crisisManager == null)
            crisisManager = FindFirstObjectByType<CrisisManager>();
        if (crisisManager == null) return;

        if (timerText == null)
        {
            GameObject go = FindByName("TIMER TEXT");
            if (go != null) timerText = go.GetComponent<TMP_Text>();
        }
        if (timerPanel == null) timerPanel = FindByName("TIMER BG");

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

    GameObject FindByName(string n)
    {
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in all) if (t.name == n) return t.gameObject;
        return null;
    }
}
