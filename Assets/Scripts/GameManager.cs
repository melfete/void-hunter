using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Der GameManager lebt nur in der Spielszene. Bei jedem (Neu-)Laden der Szene
    // uebernimmt die neue Instanz mit ihren im Inspector zugewiesenen UI-Referenzen.
    public static GameManager instance;

    public Transform respawnPoint;
    public float respawnDelay = 2f;

    public GameObject gameOverPanel;
    public Button restartButton;
    public Button quitButton;

    public bool IsGameOver { get; private set; }

    private GameObject currentPlayer;
    private AudioSource musicSource;

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Start()
    {
        GameObject musicGameObject = GameObject.Find("BackgroundMusic");
        if (musicGameObject != null)
        {
            musicSource = musicGameObject.GetComponent<AudioSource>();
            if (musicSource == null)
            {
                Debug.LogWarning("GameManager: GameObject 'BackgroundMusic' gefunden, aber keine AudioSource-Komponente darauf!");
            }
        }
        else
        {
            Debug.LogWarning("GameManager: GameObject 'BackgroundMusic' (fuer Musik) nicht in der Szene gefunden! Hintergrundmusik kann nicht gesteuert werden.");
        }

        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }

        InitializeGameOverUI();

        IsGameOver = false;
        Time.timeScale = 1f;
        ResumeAllAudio();
    }

    private void InitializeGameOverUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
            restartButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameManager: RestartButton ist nicht zugewiesen!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
            quitButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameManager: QuitButton ist nicht zugewiesen!");
        }
    }

    public void PauseAllAudio()
    {
        AudioListener.pause = true;
    }

    public void ResumeAllAudio()
    {
        AudioListener.pause = false;
    }

    public void RegisterPlayer(GameObject player)
    {
        currentPlayer = player;
    }

    public void PlayerDied()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        if (currentPlayer != null)
        {
            PauseAllAudio();
            StartCoroutine(HandlePlayerDeath());
        }
    }

    private IEnumerator HandlePlayerDeath()
    {
        yield return new WaitForSeconds(respawnDelay);
        GameOver();
    }

    public void RespawnPlayer()
    {
        if (currentPlayer != null && respawnPoint != null)
        {
            currentPlayer.SetActive(true);
            currentPlayer.transform.position = respawnPoint.position;

            PlayerHealth playerHealth = currentPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }

            if (gameOverPanel != null && gameOverPanel.activeSelf)
            {
                gameOverPanel.SetActive(false);
                if (restartButton != null) restartButton.gameObject.SetActive(false);
                if (quitButton != null) quitButton.gameObject.SetActive(false);
            }
            IsGameOver = false;
            Time.timeScale = 1f;
            ResumeAllAudio();
        }
        else
        {
            Debug.LogError("GameManager: Kann Spieler nicht respawnen! currentPlayer oder respawnPoint fehlen.");
        }
    }

    public void GameOver()
    {
        IsGameOver = true;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (restartButton != null) restartButton.gameObject.SetActive(true);
            if (quitButton != null) quitButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("GameManager: GameOverPanel fehlt - Game-Over-Bildschirm kann nicht angezeigt werden.");
        }
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        ResumeAllAudio();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        PauseAllAudio();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
