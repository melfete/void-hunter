using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    public GameObject settingsPanel;
    public Slider volumeSlider;
    public Button backButtonSettings;

    public string gameSceneName = "DeineSpielSzene";

    void Start()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);

        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        if (backButtonSettings != null)
            backButtonSettings.onClick.AddListener(CloseSettings);
        
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            SetVolume(volumeSlider.value);
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void QuitGame()
    {
        GameUtils.QuitGame();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
}