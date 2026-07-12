using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aegis.Systems;

// Shows the reactor/crisis countdown. Watches whichever CrisisManager is actually TRIGGERED
// (so duplicate managers don't break it), and FORCES the timer panel visible + on top when the
// crisis is running (so it can't be buried behind another canvas). Auto-finds everything by name.
public class CrisisTimerUI : MonoBehaviour
{
    [Header("Optional — left empty, all are auto-found")]
    public CrisisManager crisisManager;
    public GameObject timerPanel;   // auto-finds "TIMER BG"
    public TMP_Text timerText;      // auto-finds "TIMER TEXT"
    public Image fillImage;         // optional filled Image that drains

    private bool _forced;

    void Start()
    {
        Debug.Log("CrisisTimerUI: CrisisManager found=" + (FindFirstObjectByType<CrisisManager>() != null)
            + ", 'TIMER BG' found=" + (FindByName("TIMER BG") != null)
            + ", 'TIMER TEXT' found=" + (FindByName("TIMER TEXT") != null), this);
    }

    void Update()
    {
        // Find the manager that is actually counting down (handles more than one CrisisManager).
        CrisisManager active = FindTriggeredManager();

        if (timerText == null)
        {
            GameObject go = FindByName("TIMER TEXT");
            if (go != null) timerText = go.GetComponent<TMP_Text>();
        }
        if (timerPanel == null) timerPanel = FindByName("TIMER BG");

        bool show = active != null;

        if (timerPanel != null)
        {
            if (show) ForceVisible(timerPanel);
            else if (timerPanel.activeSelf) { timerPanel.SetActive(false); _forced = false; }
        }

        if (!show) return;

        // Make sure the text object is on too (it may be a separate object from the panel).
        if (timerText != null && !timerText.gameObject.activeInHierarchy)
            for (Transform t = timerText.transform; t != null; t = t.parent) t.gameObject.SetActive(true);

        float remaining = Mathf.Max(0f, active.TimeRemaining);
        float duration = active.Duration;

        if (timerText != null)
        {
            int s = Mathf.CeilToInt(remaining);
            timerText.text = (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
        }
        if (fillImage != null)
            fillImage.fillAmount = duration > 0f ? remaining / duration : 0f;
    }

    CrisisManager FindTriggeredManager()
    {
        foreach (CrisisManager c in FindObjectsByType<CrisisManager>(FindObjectsSortMode.None))
            if (c != null && c.HasTriggered && !c.IsResolved) return c;
        return null;
    }

    // Same trick as the death panel: activate the panel + parents + content, fix any CanvasGroup,
    // and stamp it with a high sort order so no other canvas can hide it.
    void ForceVisible(GameObject panel)
    {
        for (Transform t = panel.transform; t != null; t = t.parent)
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);

        if (_forced) return; // heavy stuff once
        _forced = true;

        foreach (Transform child in panel.GetComponentsInChildren<Transform>(true))
            child.gameObject.SetActive(true);

        for (Transform t = panel.transform; t != null; t = t.parent)
        {
            CanvasGroup cg = t.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }
        }

        Canvas c = panel.GetComponent<Canvas>();
        if (c == null) c = panel.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 30000;
        if (panel.GetComponent<GraphicRaycaster>() == null)
            panel.AddComponent<GraphicRaycaster>();

        panel.transform.SetAsLastSibling();
    }

    GameObject FindByName(string n)
    {
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in all) if (t.name == n) return t.gameObject;
        return null;
    }
}
