using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Aegis.Player;

// X campaign ending manager. Mirrors AegisEndingManager but locks the NEW player
// (Mohammed's MovementController / WeaponHandler that X actually uses) instead of the
// old TinyToaster/AegisWeaponController. Call PlayGoodEnding/PlayBadEnding from
// CrisisManager's OnCrisisSucceeded / OnCrisisFailed events (defuse in time = good).
public class XEndingManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject deathPanel;
    public GameObject gameplayUIPanel;
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Good Ending Video")]
    public GameObject goodEndingCutsceneObject;
    public VideoPlayer goodEndingVideo;

    [Header("Bad Ending Video")]
    public GameObject badEndingCutsceneObject;
    public VideoPlayer badEndingVideo;

    [Header("Player (X)")]
    public GameObject xPlayer;
    public MovementController movementController;
    public WeaponHandler weaponController;

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

        Debug.Log("X DEATH PANEL");
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

        if (goodEndingVideo != null)
            goodEndingVideo.Play();

        Debug.Log("X GOOD ENDING VIDEO");
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

        if (badEndingVideo != null)
            badEndingVideo.Play();

        Debug.Log("X BAD ENDING VIDEO");
    }

    void DisablePlayerControl()
    {
        if (movementController != null)
            movementController.enabled = false;

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
