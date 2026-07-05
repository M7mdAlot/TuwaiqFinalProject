using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [Header("Audio")]
    public Slider audioSlider;
    public AudioMixer audioMixer;
    public string volumeParameterName = "MasterVolume";

    [Header("How To Play")]
    public GameObject howToPlayPanel;
    public Button howToPlayButton;
    public Button closeHowToPlayButton;

    const string VolumePrefKey = "MasterVolumeValue";

    void Awake()
    {
        if (audioSlider != null)
            audioSlider.onValueChanged.AddListener(SetVolume);

        if (howToPlayButton != null)
            howToPlayButton.onClick.AddListener(OpenHowToPlay);

        if (closeHowToPlayButton != null)
            closeHowToPlayButton.onClick.AddListener(CloseHowToPlay);
    }

    void Start()
    {
        float savedValue = PlayerPrefs.GetFloat(VolumePrefKey, 1f);

        if (audioSlider != null)
            audioSlider.value = savedValue;

        SetVolume(savedValue);
        CloseHowToPlay();
    }

    public void SetVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);

        if (audioMixer != null)
        {
            float volumeDb = Mathf.Log10(value) * 20f;
            audioMixer.SetFloat(volumeParameterName, volumeDb);
        }
        else
        {
            AudioListener.volume = value;
        }

        PlayerPrefs.SetFloat(VolumePrefKey, value);
        PlayerPrefs.Save();
    }

    public void OpenHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }
}