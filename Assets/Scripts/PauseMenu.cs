using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;

    public GameObject pauseMenuUI;

    public Button resumeButton;
    public Button quitButton;
    public Button mainMenuButton;

    public string mainMenuSceneName = "MainMenu";

    public Slider volumeSlider;

    void Awake()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(Resume);
        }
        else
        {
            Debug.LogWarning("PauseMenu: Resume Button wurde im Inspector nicht zugewiesen!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        else
        {
            Debug.LogWarning("PauseMenu: Quit Button wurde im Inspector nicht zugewiesen!");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenuScene);
        }
        else
        {
            Debug.LogWarning(
                "PauseMenu: Main Menu Button wurde im Inspector nicht zugewiesen! Falls vorhanden, bitte zuweisen.");
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            Debug.LogWarning("PauseMenu: Volume Slider wurde im Inspector nicht zugewiesen!");
        }
    }

    void Start()
    {
        isPaused = false;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f;

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
        }
    }

    void Update()
    {
        // Waehrend Game Over oder der Endsequenz darf das Pausemenue die Zeit nicht wieder starten.
        bool gameOver = GameManager.instance != null && GameManager.instance.IsGameOver;
        bool endSequence = GameEndManager.instance != null && GameEndManager.instance.IsEffectActive;
        if (gameOver || endSequence) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        UIPanelFade.Hide(pauseMenuUI);

        Time.timeScale = 1f;
        isPaused = false;

        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeAllAudio();
        }
    }

    void Pause()
    {
        UIPanelFade.Show(pauseMenuUI);

        Time.timeScale = 0f;
        isPaused = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.PauseAllAudio();
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
        }
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void LoadMainMenuScene()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeAllAudio();
        }

        SceneFader.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        GameUtils.QuitGame();
    }
}
