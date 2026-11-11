using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameEndManager : MonoBehaviour
{
    public static GameEndManager instance;

    public float effectDuration = 2.0f;
    public float fadeToBlackDuration = 1.0f;

    public Camera mainCamera;
    public Transform portalTransform;
    public float cameraZoomTarget = 0.5f;
    public float cameraMoveSpeed = 1.0f;
    public float cameraZoomSpeed = 1.0f;

    public CanvasGroup endScreenCanvasGroup;
    public TextMeshProUGUI endScreenTextOverlay;
    public Button quitButtonOverlay;

    public string targetSceneName;

    private bool effectActive = false;
    public bool IsEffectActive => effectActive;
    private Vector3 initialCameraPosition;
    private float initialCameraOrthographicSize;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("GameEndManager: Keine Hauptkamera (MainCamera) mit Tag 'MainCamera' gefunden! Kameraeffekt nicht möglich.");
                enabled = false;
                return;
            }
        }

        initialCameraPosition = mainCamera.transform.position;
        initialCameraOrthographicSize = mainCamera.orthographicSize;

        if (endScreenCanvasGroup != null)
        {
            endScreenCanvasGroup.alpha = 0;
            endScreenCanvasGroup.gameObject.SetActive(false);
            
            if (endScreenTextOverlay != null)
            {
                endScreenTextOverlay.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("GameEndManager: End Screen Text Overlay nicht zugewiesen! Text wird nicht angezeigt.");
            }
            
            if (quitButtonOverlay != null)
            {
                quitButtonOverlay.gameObject.SetActive(false);
                quitButtonOverlay.onClick.AddListener(QuitGame);
            }
            else
            {
                Debug.LogWarning("GameEndManager: Quit Button Overlay nicht zugewiesen! Button wird nicht angezeigt.");
            }
        }
        else
        {
            Debug.LogError("GameEndManager: End Screen Canvas Group nicht zugewiesen! Schwarzblende und Endbildschirm werden nicht funktionieren.");
            enabled = false;
            return;
        }

        if (portalTransform == null)
        {
            Debug.LogError("GameEndManager: Portal Transform nicht zugewiesen! Kamera kann nicht zum Portal bewegen.");
        }
    }

    public void StartEndGameEffect(GameObject player)
    {
        if (effectActive) return;
        effectActive = true;


        if (GameManager.instance != null)
        {
            GameManager.instance.PauseAllAudio();
        }

        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null) playerRb.bodyType = RigidbodyType2D.Kinematic;
            player.SetActive(false);
        }
        
        if (mainCamera != null && portalTransform != null)
        {
            StartCoroutine(PerformEndGameSequence());
        }
        else
        {
            Debug.LogWarning("GameEndManager: Kamera oder Portal nicht zugewiesen. Überspringe Kameraeffekt, gehe direkt zur Schwarzblende.");
            StartCoroutine(FadeToBlackAndShowEndScreen());
        }
    }

    IEnumerator PerformEndGameSequence()
    {
        float timer = 0f;
        Vector3 camStartPos = mainCamera.transform.position;
        float camStartSize = mainCamera.orthographicSize;
        Vector3 camTargetPos = new Vector3(portalTransform.position.x, portalTransform.position.y, mainCamera.transform.position.z);
        float camTargetSize = cameraZoomTarget;

        while (timer < effectDuration)
        {
            timer += Time.unscaledDeltaTime;
            float normalizedTime = timer / effectDuration;

            mainCamera.transform.position = Vector3.Lerp(camStartPos, camTargetPos, normalizedTime * cameraMoveSpeed);
            mainCamera.orthographicSize = Mathf.Lerp(camStartSize, camTargetSize, normalizedTime * cameraZoomSpeed);

            yield return null;
        }

        mainCamera.transform.position = camTargetPos;
        mainCamera.orthographicSize = camTargetSize;

        Time.timeScale = 0f;
        yield return StartCoroutine(FadeToBlackAndShowEndScreen());
    }

    IEnumerator FadeToBlackAndShowEndScreen()
    {
        if (endScreenCanvasGroup == null)
        {
            Debug.LogError("GameEndManager: End Screen Canvas Group ist NULL. Fade-Effekt nicht möglich.");
            yield break;
        }

        endScreenCanvasGroup.gameObject.SetActive(true);
        endScreenCanvasGroup.blocksRaycasts = false;
        endScreenCanvasGroup.interactable = false;

        float timer = 0f;
        while (timer < fadeToBlackDuration)
        {
            timer += Time.unscaledDeltaTime;
            endScreenCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeToBlackDuration);
            yield return null;
        }
        endScreenCanvasGroup.alpha = 1;

        if (endScreenTextOverlay != null)
        {
            endScreenTextOverlay.gameObject.SetActive(true);
        }
        
        if (quitButtonOverlay != null)
        {
            quitButtonOverlay.gameObject.SetActive(true);
            endScreenCanvasGroup.blocksRaycasts = true;
            endScreenCanvasGroup.interactable = true;
        }
    }
    
    public void QuitGame()
    {
        GameUtils.QuitGame();
    }
}