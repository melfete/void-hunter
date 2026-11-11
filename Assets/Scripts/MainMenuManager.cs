using UnityEngine;
using UnityEngine.UI;

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

    void Update()
    {
        // Esc fuehrt aus den Einstellungen zurueck.
        if (Input.GetKeyDown(KeyCode.Escape) && settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
        }
    }

    public void PlayGame()
    {
        SceneFader.LoadScene(gameSceneName);
    }

    // Weicher Wechsel: erst das aktuelle Panel ausblenden, dann das neue einblenden.
    public void OpenSettings()
    {
        UIPanelFade.Hide(mainPanel, () => UIPanelFade.Show(settingsPanel));
    }

    public void CloseSettings()
    {
        UIPanelFade.Hide(settingsPanel, () => UIPanelFade.Show(mainPanel));
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
