using UnityEngine;
using UnityEngine.InputSystem;
using Aegis.Input;

namespace Aegis.Systems
{
    /// <summary>
    /// Handles the pause menu and its settings sub-panel. Toggling pause freezes the game
    /// (Time.timeScale = 0), frees the cursor, disables gameplay input (so the camera/gun
    /// don't move while paused), and shows the pause panel. Uses its OWN Escape/Start input
    /// (created in code) so it keeps working even while gameplay input is disabled.
    ///
    /// Wire the pause menu's buttons to the public methods: Resume, OpenSettings, CloseSettings.
    /// Settings content itself (volume sliders etc.) can stay on the teammate's SettingsUIController.
    /// Tier 5.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _pauseMenu;
        [SerializeField] private GameObject _settingsMenu;

        [Header("Input")]
        [Tooltip("Used to freeze/unfreeze gameplay input (move, look, fire) while paused.")]
        [SerializeField] private InputReader _inputReader;

        [Header("Options")]
        [SerializeField] private bool _lockCursorWhenResumed = true;
        [SerializeField] private bool _allowToggleKey = true;

        public bool IsPaused { get; private set; }

        private InputAction _pauseAction;

        private void Awake()
        {
            // Standalone action so it fires even when the gameplay map is disabled (i.e. while paused).
            _pauseAction = new InputAction("Pause", InputActionType.Button);
            _pauseAction.AddBinding("<Keyboard>/escape");
            _pauseAction.AddBinding("<Gamepad>/start");
        }

        private void OnEnable()
        {
            if (_allowToggleKey)
            {
                _pauseAction.performed += OnPausePressed;
                _pauseAction.Enable();
            }
        }

        private void OnDisable()
        {
            _pauseAction.performed -= OnPausePressed;
            _pauseAction.Disable();
        }

        private void OnPausePressed(InputAction.CallbackContext ctx) => TogglePause();

        public void TogglePause() => SetPaused(!IsPaused);

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;

            if (_pauseMenu != null) _pauseMenu.SetActive(paused);
            if (paused && _settingsMenu != null) _settingsMenu.SetActive(false);

            Cursor.visible = paused;
            Cursor.lockState = paused
                ? CursorLockMode.None
                : (_lockCursorWhenResumed ? CursorLockMode.Locked : CursorLockMode.None);

            if (_inputReader != null)
            {
                if (paused) _inputReader.DisableGameplay();
                else _inputReader.EnableGameplay();
            }
        }

        /// <summary>Wire to the Resume button.</summary>
        public void Resume() => SetPaused(false);

        /// <summary>Wire to the Settings button (opens settings, hides the pause list).</summary>
        public void OpenSettings()
        {
            if (_settingsMenu != null) _settingsMenu.SetActive(true);
            if (_pauseMenu != null) _pauseMenu.SetActive(false);
        }

        /// <summary>Wire to the settings Back button (back to the pause list).</summary>
        public void CloseSettings()
        {
            if (_settingsMenu != null) _settingsMenu.SetActive(false);
            if (_pauseMenu != null) _pauseMenu.SetActive(true);
        }
    }
}
