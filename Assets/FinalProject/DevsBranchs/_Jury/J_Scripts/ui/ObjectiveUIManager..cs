using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectiveUIManager : MonoBehaviour
{
    [Header("Temporary Objective")]
    public RectTransform tempObjectiveBadge;
    public CanvasGroup tempObjectiveBadgeCanvas;

    public RectTransform tempObjectivePanel;
    public CanvasGroup tempObjectiveCanvas;

    public TMP_Text tempTitleText;
    public TMP_Text tempObjectiveText;

    [Header("Persistent Objective")]
    public TMP_Text persistentObjectiveText;

    [Header("Animation Settings")]
    public float badgeShowTime = 0.45f;
    public float unfoldTime = 0.35f;
    public float stayTime = 2.2f;
    public float foldTime = 0.35f;

    [Header("Slide Settings")]
    public Vector2 visiblePosition;
    public Vector2 hiddenOffset = new Vector2(0f, 250f);

    private Coroutine objectiveRoutine;

    void Start()
    {
        if (tempObjectivePanel != null)
        {
            visiblePosition = tempObjectivePanel.anchoredPosition;
            tempObjectivePanel.gameObject.SetActive(false);
        }

        if (tempObjectiveBadge != null)
            tempObjectiveBadge.gameObject.SetActive(false);

        if (tempObjectiveCanvas != null)
            tempObjectiveCanvas.alpha = 0f;

        if (tempObjectiveBadgeCanvas != null)
            tempObjectiveBadgeCanvas.alpha = 0f;
    }

    public void ShowObjective(string objectiveText)
    {
        ShowObjective("NEW OBJECTIVE", objectiveText);
    }

    public void ShowObjective(string title, string objectiveText)
    {
        if (objectiveRoutine != null)
            StopCoroutine(objectiveRoutine);

        objectiveRoutine = StartCoroutine(ShowObjectiveRoutine(title, objectiveText));
    }

    IEnumerator ShowObjectiveRoutine(string title, string objectiveText)
    {
        if (tempTitleText != null)
            tempTitleText.text = title;

        if (tempObjectiveText != null)
            tempObjectiveText.text = objectiveText;

        // 1) يظهر الأوبجكت الصغير قبل القائمة
        if (tempObjectiveBadge != null)
        {
            tempObjectiveBadge.gameObject.SetActive(true);

            if (tempObjectiveBadgeCanvas != null)
                tempObjectiveBadgeCanvas.alpha = 0f;

            float t = 0f;

            while (t < badgeShowTime)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / badgeShowTime);

                if (tempObjectiveBadgeCanvas != null)
                    tempObjectiveBadgeCanvas.alpha = p;

                yield return null;
            }
        }

        // 2) القائمة تنفتح من فوق لتحت كأنها مطوية
        if (tempObjectivePanel != null)
        {
            tempObjectivePanel.gameObject.SetActive(true);
            tempObjectivePanel.anchoredPosition = visiblePosition + hiddenOffset;
            tempObjectivePanel.localScale = new Vector3(1f, 0f, 1f);
        }

        if (tempObjectiveCanvas != null)
            tempObjectiveCanvas.alpha = 1f;

        float openTimer = 0f;

        while (openTimer < unfoldTime)
        {
            openTimer += Time.deltaTime;
            float p = Mathf.Clamp01(openTimer / unfoldTime);
            p = Smooth(p);

            if (tempObjectivePanel != null)
            {
                tempObjectivePanel.anchoredPosition = Vector2.Lerp(visiblePosition + hiddenOffset, visiblePosition, p);
                tempObjectivePanel.localScale = new Vector3(1f, Mathf.Lerp(0f, 1f, p), 1f);
            }

            yield return null;
        }

        yield return new WaitForSeconds(stayTime);

        // 3) القائمة تنطوي وترجع تختفي
        float closeTimer = 0f;

        while (closeTimer < foldTime)
        {
            closeTimer += Time.deltaTime;
            float p = Mathf.Clamp01(closeTimer / foldTime);
            p = Smooth(p);

            if (tempObjectivePanel != null)
            {
                tempObjectivePanel.anchoredPosition = Vector2.Lerp(visiblePosition, visiblePosition + hiddenOffset, p);
                tempObjectivePanel.localScale = new Vector3(1f, Mathf.Lerp(1f, 0f, p), 1f);
            }

            if (tempObjectiveCanvas != null)
                tempObjectiveCanvas.alpha = Mathf.Lerp(1f, 0f, p);

            if (tempObjectiveBadgeCanvas != null)
                tempObjectiveBadgeCanvas.alpha = Mathf.Lerp(1f, 0f, p);

            yield return null;
        }

        if (tempObjectivePanel != null)
            tempObjectivePanel.gameObject.SetActive(false);

        if (tempObjectiveBadge != null)
            tempObjectiveBadge.gameObject.SetActive(false);

        // 4) بعدها يتحدث الأوبجكتف الدائم في الزاوية
        if (persistentObjectiveText != null)
            persistentObjectiveText.text = objectiveText;
    }

    float Smooth(float value)
    {
        return value * value * (3f - 2f * value);
    }
}