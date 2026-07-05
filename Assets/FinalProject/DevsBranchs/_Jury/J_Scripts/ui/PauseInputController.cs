using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInputController : MonoBehaviour
{
    public PlayerInput playerInput;
    public MainMenuUIController uiController;

    public string pauseActionName = "Pause";

    private InputAction pauseAction;
    private bool isPaused;

    void Awake()
    {
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (uiController == null)
            uiController = FindFirstObjectByType<MainMenuUIController>();

        if (playerInput != null)
            pauseAction = playerInput.actions.FindAction(pauseActionName, false);
    }

    void Update()
    {
        if (pauseAction == null) return;

        if (pauseAction.WasPressedThisFrame())
            TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            if (uiController != null)
                uiController.OpenPauseMenu();
            else
                Time.timeScale = 0f;
        }
        else
        {
            if (uiController != null)
                uiController.ResumeGame();
            else
                Time.timeScale = 1f;
        }
    }
}