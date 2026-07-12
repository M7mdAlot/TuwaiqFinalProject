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

        Debug.Log("XEndingManager SETUP: deathPanel=" + (deathPanel != null ? deathPanel.name : "NULL(auto-find)")
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
        if (deathPanel == null) deathPanel = FindByName("DEATH");

        if (deathPanel != null)
            deathPanel.SetActive(true);

        Debug.Log("X DEATH PANEL");
    }

    GameObject FindByName(string targetName)
    {
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in all)
            if (t.name == targetName) return t.gameObject;
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

        ForceShow(goodEndingCutsceneObject);

        if (goodEndingVideo != null)
        {
            ForceShow(goodEndingVideo.gameObject);
            goodEndingVideo.Play();
        }

        Debug.Log("X GOOD ENDING -> cutscene=" + (goodEndingCutsceneObject != null) +
                  ", video=" + (goodEndingVideo != null) +
                  (goodEndingCutsceneObject == null && goodEndingVideo == null
                      ? "  <-- NOTHING ASSIGNED! Assign Good Ending Cutscene Object / Video." : ""), this);
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

        ForceShow(badEndingCutsceneObject);

        if (badEndingVideo != null)
        {
            ForceShow(badEndingVideo.gameObject);
            badEndingVideo.Play();
        }

        Debug.Log("X BAD ENDING -> cutscene=" + (badEndingCutsceneObject != null) +
                  ", video=" + (badEndingVideo != null) +
                  (badEndingCutsceneObject == null && badEndingVideo == null
                      ? "  <-- NOTHING ASSIGNED! Assign Bad Ending Cutscene Object / Video." : ""), this);
    }

    // Activate the object + every parent so an inactive parent can't hide it, and put it on top.
    void ForceShow(GameObject go)
    {
        if (go == null) return;
        for (Transform t = go.transform; t != null; t = t.parent)
            t.gameObject.SetActive(true);
        go.transform.SetAsLastSibling();

        UnityEngine.UI.Graphic g = go.GetComponent<UnityEngine.UI.Graphic>();
        if (g != null)
        {
            Canvas c = go.GetComponent<Canvas>();
            if (c == null) c = go.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 31000;
        }
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
