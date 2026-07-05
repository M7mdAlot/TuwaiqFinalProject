using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject chooseCharacterPanel;
    public GameObject settingsPanel;
    public GameObject pausePanel;
    public GameObject dialoguePanel;
    public GameObject gameplayUIPanel;

    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Character Select Buttons")]
    public Button aegisButton;
    public Button nullButton;
    public Button cancelCharacterButton;
    public Button closeCharacterButton;

    [Header("Settings Buttons")]
    public Button settingsBackButton;

    [Header("Pause Buttons")]
    public Button resumeButton;
    public Button pauseSettingsButton;
    public Button pauseMainMenuButton;

    [Header("Scene Names")]
    public string aegisSceneName = "AegisScene";
    public string nullSceneName = "NullScene";
    public string mainMenuSceneName = "MAIN MENU";

    [Header("Options")]
    public bool loadSceneImmediatelyAfterCharacterSelect = true;

    private GameObject previousPanel;

    void Awake()
    {
        HookButtons();
    }

    void Start()
    {
        ShowMainMenu();
        Time.timeScale = 1f;
        ShowCursor();
    }

    void HookButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(ShowChooseCharacter);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettingsFromMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (aegisButton != null)
            aegisButton.onClick.AddListener(ChooseAegis);

        if (nullButton != null)
            nullButton.onClick.AddListener(ChooseNull);

        if (cancelCharacterButton != null)
            cancelCharacterButton.onClick.AddListener(CancelChooseCharacter);

        if (closeCharacterButton != null)
            closeCharacterButton.onClick.AddListener(CancelChooseCharacter);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(BackFromSettings);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (pauseSettingsButton != null)
            pauseSettingsButton.onClick.AddListener(OpenSettingsFromPause);

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(LoadMainMenuScene);
    }

    public void ShowMainMenu()
    {
        Time.timeScale = 1f;
        ShowCursor();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (chooseCharacterPanel != null)
            chooseCharacterPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);

        previousPanel = mainMenuPanel;
    }

    public void ShowChooseCharacter()
    {
        Time.timeScale = 1f;
        ShowCursor();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (chooseCharacterPanel != null)
            chooseCharacterPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);

        previousPanel = mainMenuPanel;
    }

    public void CancelChooseCharacter()
    {
        Time.timeScale = 1f;
        ShowCursor();

        if (chooseCharacterPanel != null)
            chooseCharacterPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);

        previousPanel = mainMenuPanel;

        Debug.Log("Canceled character select. Back to main menu.");
    }

    public void OpenSettingsFromMainMenu()
    {
        Time.timeScale = 1f;
        ShowCursor();

        previousPanel = mainMenuPanel;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (chooseCharacterPanel != null)
            chooseCharacterPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);
    }

    public void OpenSettingsFromPause()
    {
        Time.timeScale = 0f;
        ShowCursor();

        previousPanel = pausePanel;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (chooseCharacterPanel != null)
            chooseCharacterPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void BackFromSettings()
    {
        ShowCursor();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (previousPanel != null)
            previousPanel.SetActive(true);
        else
            ShowMainMenu();
    }

    public void ChooseAegis()
    {
        PlayerPrefs.SetString("SelectedCharacter", "Aegis");
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        if (loadSceneImmediatelyAfterCharacterSelect)
            SceneManager.LoadScene(aegisSceneName);
    }

    public void ChooseNull()
    {
        PlayerPrefs.SetString("SelectedCharacter", "Null");
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        if (loadSceneImmediatelyAfterCharacterSelect)
            SceneManager.LoadScene(nullSceneName);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        HideCursor();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(true);
    }

    public void OpenPauseMenu()
    {
        Time.timeScale = 0f;
        ShowCursor();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (chooseCharacterPanel != null)
            chooseCharacterPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        previousPanel = pausePanel;
    }

    public void LoadMainMenuScene()
    {
        Time.timeScale = 1f;
        ShowCursor();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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