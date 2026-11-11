using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Weiche Schwarzblende bei jedem Szenenwechsel. Legt sich beim Spielstart selbst an
// und bleibt ueber alle Szenen erhalten. Aufruf statt SceneManager.LoadScene:
//     SceneFader.LoadScene("MainMenu");
public class SceneFader : MonoBehaviour
{
    public static float fadeOutDuration = 0.35f;
    public static float fadeInDuration = 0.45f;

    private static SceneFader instance;
    private CanvasGroup group;
    private bool isLoading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (instance != null) return;

        GameObject go = new GameObject("SceneFader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneFader>();

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000; // ueber allen anderen UIs

        instance.group = go.AddComponent<CanvasGroup>();
        instance.group.alpha = 1f;          // Spielstart: aus Schwarz einblenden
        instance.group.blocksRaycasts = true;

        GameObject img = new GameObject("Black");
        img.transform.SetParent(go.transform, false);
        Image image = img.AddComponent<Image>();
        image.color = Color.black;
        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        SceneManager.sceneLoaded += (scene, mode) => instance.StartCoroutine(instance.FadeIn());
    }

    public static void LoadScene(string sceneName)
    {
        if (instance == null) { SceneManager.LoadScene(sceneName); return; }
        if (!instance.isLoading) instance.StartCoroutine(instance.FadeOutAndLoad(() => SceneManager.LoadScene(sceneName)));
    }

    public static void LoadScene(int buildIndex)
    {
        if (instance == null) { SceneManager.LoadScene(buildIndex); return; }
        if (!instance.isLoading) instance.StartCoroutine(instance.FadeOutAndLoad(() => SceneManager.LoadScene(buildIndex)));
    }

    private IEnumerator FadeOutAndLoad(System.Action load)
    {
        isLoading = true;
        group.blocksRaycasts = true; // keine Doppelklicks waehrend der Blende
        yield return Fade(group.alpha, 1f, fadeOutDuration);
        load();
        isLoading = false;
    }

    private IEnumerator FadeIn()
    {
        group.alpha = 1f;
        yield return null; // ein Frame warten, bis die neue Szene steht (keine Ruckler)
        yield return Fade(1f, 0f, fadeInDuration);
        if (!isLoading) group.blocksRaycasts = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // laeuft auch bei Pause (timeScale 0)
            group.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        group.alpha = to;
    }
}
