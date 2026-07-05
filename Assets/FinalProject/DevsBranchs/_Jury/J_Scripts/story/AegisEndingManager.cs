using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class AegisEndingManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject deathPanel;
    public GameObject gameplayUIPanel;
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Good Ending Cutscene")]
    public GameObject goodEndingCutsceneObject;
    public PlayableDirector goodEndingTimeline;

    [Header("Bad Ending Cutscene")]
    public GameObject badEndingCutsceneObject;
    public PlayableDirector badEndingTimeline;

    [Header("Player")]
    public GameObject aegisPlayer;
    public TinyToasterController movementController;
    public AegisWeaponController weaponController;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MAIN MENU";

    private bool endingStarted;

    void Start()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (goodEndingCutsceneObject != null)
            goodEndingCutsceneObject.SetActive(false);

        if (badEndingCutsceneObject != null)
            badEndingCutsceneObject.SetActive(false);
    }

    public void ShowDeathPanel()
    {
        if (endingStarted) return;

        endingStarted = true;

        Time.timeScale = 0f;

        DisablePlayerControl();
        ShowCursor();

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (deathPanel != null)
            deathPanel.SetActive(true);

        Debug.Log("AEGIS DEATH PANEL");
    }

    public void PlayGoodEnding()
    {
        if (endingStarted) return;

        endingStarted = true;

        Time.timeScale = 1f;

        DisablePlayerControl();
        HideCursorForCutscene();

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);

        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (goodEndingCutsceneObject != null)
            goodEndingCutsceneObject.SetActive(true);

        if (goodEndingTimeline != null)
            goodEndingTimeline.Play();

        Debug.Log("AEGIS GOOD ENDING CUTSCENE");
    }

    public void PlayBadEnding()
    {
        if (endingStarted) return;

        endingStarted = true;

        Time.timeScale = 1f;

        DisablePlayerControl();
        HideCursorForCutscene();

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);

        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (badEndingCutsceneObject != null)
            badEndingCutsceneObject.SetActive(true);

        if (badEndingTimeline != null)
            badEndingTimeline.Play();

        Debug.Log("AEGIS BAD ENDING CUTSCENE");
    }

    void DisablePlayerControl()
    {
        if (movementController != null)
            movementController.SetMovementLocked(true);

        if (weaponController != null)
            weaponController.enabled = false;
    }

    void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void HideCursorForCutscene()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}