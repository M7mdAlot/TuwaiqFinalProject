using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Mode")]
    public bool isGameplayScene = false;

    [Header("Panels That Should Show Cursor")]
    public GameObject[] panelsThatShowCursor;

    [Header("Settings")]
    public bool showCursorWhenAnyPanelIsOpen = true;

    void Start()
    {
        UpdateCursorState();
    }

    void Update()
    {
        UpdateCursorState();
    }

    void UpdateCursorState()
    {
       
        if (!isGameplayScene)
        {
            ShowCursor();
            return;
        }

        if (showCursorWhenAnyPanelIsOpen && IsAnyPanelOpen())
        {
            ShowCursor();
            return;
        }

        HideCursor();
    }

    bool IsAnyPanelOpen()
    {
        if (panelsThatShowCursor == null)
            return false;

        foreach (GameObject panel in panelsThatShowCursor)
        {
            if (panel != null && panel.activeInHierarchy)
                return true;
        }

        return false;
    }

    void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}