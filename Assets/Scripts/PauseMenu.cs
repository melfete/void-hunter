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
            Debug.Log("PauseMenu: Listener fuer Resume Button hinzugefuegt.");
        }
        else
        {
            Debug.LogWarning("PauseMenu: Resume Button wurde im Inspector nicht zugewiesen!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            Debug.Log("PauseMenu: Listener fuer Quit Game Button hinzugefuegt.");
        }
        else
        {
            Debug.LogWarning("PauseMenu: Quit Button wurde im Inspector nicht zugewiesen!");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenuScene);
            Debug.Log("PauseMenu: Listener fuer Main Menu Button hinzugefuegt.");
        }
        else
        {
            Debug.LogWarning(
                "PauseMenu: Main Menu Button wurde im Inspector nicht zugewiesen! Falls vorhanden, bitte zuweisen.");
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
            Debug.Log("PauseMenu: Listener fuer Volume Slider hinzugefuegt.");
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
        Debug.Log("PauseMenu: Start-Methode aufgerufen. isPaused=" + isPaused + ", pauseMenuUI.activeSelf=" +
                  (pauseMenuUI != null ? pauseMenuUI.activeSelf.ToString() : "NULL"));
        Debug.Log("PauseMenu: Time.timeScale zu Beginn von Start: " + Time.timeScale);

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            Debug.Log("PauseMenu: Volume Slider initialisiert mit AudioListener.volume: " + AudioListener.volume);
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
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("PauseMenu: Spiel fortgesetzt. Time.timeScale: " + Time.timeScale);

        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeAllAudio();
        }
    }

    void Pause()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Time.timeScale = 0f;
        isPaused = true;
        Debug.Log("PauseMenu: Spiel pausiert. Time.timeScale: " + Time.timeScale);

        if (GameManager.instance != null)
        {
            GameManager.instance.PauseAllAudio();
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            Debug.Log("PauseMenu: Volume Slider aktualisiert beim Oeffnen des Menues: " + AudioListener.volume);
        }
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log("PauseMenu: Lautstaerke gesetzt auf: " + volume);
    }

    public void LoadMainMenuScene()
    {
        Debug.Log("PauseMenu: Lade Hauptmenue Szene: " + mainMenuSceneName);
        Time.timeScale = 1f;
        isPaused = false;

        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeAllAudio();
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("PauseMenu: Spiel beendet.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
