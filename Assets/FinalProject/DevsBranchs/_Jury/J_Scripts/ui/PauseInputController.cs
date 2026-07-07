using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInputController : MonoBehaviour
{
    public PlayerInput playerInput;
    public MainMenuUIController uiController;

    public string pauseActionName = "Pause";

    [Tooltip("Fallback pause panel name, used only if no MainMenuUIController is found.")]
    public string pausePanelName = "Panel PAUSE";

    public bool debugLogs = true;

    private InputAction pauseAction;
    private bool isPaused;

    void Update()
    {
        // This object persists across scenes (DontDestroyOnLoad on "manager"), so it wakes
        // once in the Main Menu where no player exists yet. Acquire input + UI lazily, and
        // search INCLUDING inactive objects (the menu canvas is inactive during gameplay).
        if (pauseAction == null)
            TryAcquireInput();

        if (uiController == null)
            uiController = FindFirstObjectByType<MainMenuUIController>(FindObjectsInactive.Include);

        bool pressed = false;

        if (pauseAction != null && pauseAction.WasPressedThisFrame())
            pressed = true;

        // Fallback: raw ESC, in case the Pause action isn't enabled on the found PlayerInput.
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            pressed = true;

        if (pressed)
            TogglePause();
    }

    void TryAcquireInput()
    {
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (playerInput != null)
            pauseAction = playerInput.actions.FindAction(pauseActionName, false);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (debugLogs)
            Debug.Log("PAUSE TOGGLED. isPaused=" + isPaused + " uiController=" + (uiController != null));

        if (uiController != null)
        {
            if (isPaused) uiController.OpenPauseMenu();
            else uiController.ResumeGame();
            return;
        }

        // No MainMenuUIController in this scene — drive the pause panel directly.
        GameObject panel = FindByNameIncludingInactive(pausePanelName);

        if (panel != null)
            panel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

        if (debugLogs && panel == null)
            Debug.LogWarning("PauseInputController: no pause panel named '" + pausePanelName + "' found.");
    }

    // Finds a GameObject by name even if it (or its parents) are inactive, across all
    // loaded scenes including DontDestroyOnLoad. Runs only when pausing, so cost is fine.
    GameObject FindByNameIncludingInactive(string targetName)
    {
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Transform t in all)
            if (t.name == targetName)
                return t.gameObject;

        return null;
    }
}
