using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Aegis.Player;

// Aegis campaign ending manager. Endings are VIDEOS (VideoPlayer), not Timelines.
// Locks the current player (Mohammed's MovementController / WeaponHandler that Aegis
// uses now). Call PlayGoodEnding/PlayBadEnding from CrisisManager's
// OnCrisisSucceeded / OnCrisisFailed events (defuse in time = good ending).
public class AegisEndingManager : MonoBehaviour
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

    [Header("Player")]
    public GameObject aegisPlayer;
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

        Debug.Log("AegisEndingManager SETUP: deathPanel=" + (deathPanel != null ? deathPanel.name : "NULL(auto-find)")
            + ", goodVideo=" + (goodEndingVideo != null) + ", badVideo=" + (badEndingVideo != null), this);
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

        // Auto-find the DEATH panel by name if it wasn't wired (it lives in the persistent Canvas).
        if (deathPanel == null) deathPanel = FindDeathPanel();

        if (deathPanel != null) deathPanel.SetActive(true);
        else Debug.LogError("AEGIS DEATH PANEL: no death panel assigned AND none found by name. " +
                            "Assign the Death Panel field, or name the panel object 'DEATH'.", this);

        Debug.Log("AEGIS DEATH PANEL -> panel=" + (deathPanel != null ? deathPanel.name : "NULL"), this);
    }

    GameObject FindByName(string targetName)
    {
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in all)
            if (t.name == targetName) return t.gameObject;
        return null;
    }

    // Exact "DEATH" first, then any object whose name contains "death" (case-insensitive).
    GameObject FindDeathPanel()
    {
        GameObject g = FindByName("DEATH");
        if (g != null) return g;
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in all)
            if (t.name.ToLowerInvariant().Contains("death")) return t.gameObject;
        return null;
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

        Debug.Log("AEGIS GOOD ENDING VIDEO");
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

        Debug.Log("AEGIS BAD ENDING VIDEO");
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
