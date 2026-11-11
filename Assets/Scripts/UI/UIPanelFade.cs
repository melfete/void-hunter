using System.Collections;
using UnityEngine;

// Laesst ein Menue-Panel weich erscheinen (Einblenden + leichtes Hochgleiten/Wachsen)
// und wieder verschwinden. Funktioniert auch bei Pause (timeScale 0).
// Wird von UIPolish automatisch an alle Objekte gehaengt, deren Name auf "Panel" endet.
//
// Zum Ein-/Ausblenden statt SetActive verwenden:
//     UIPanelFade.Show(panel);   UIPanelFade.Hide(panel);
[RequireComponent(typeof(RectTransform))]
public class UIPanelFade : MonoBehaviour
{
    public float fadeInDuration = 0.25f;
    public float fadeOutDuration = 0.15f;
    public float startScale = 0.96f;
    public float slideDistance = 18f;

    private CanvasGroup group;
    private RectTransform rect;
    private Vector2 basePosition;
    private bool hasBase;
    private Coroutine running;

    void Awake()
    {
        rect = (RectTransform)transform;
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        PlayIn();
    }

    void OnDisable()
    {
        // Endzustand herstellen, damit das Panel beim naechsten Mal sauber startet.
        running = null;
        if (hasBase) rect.anchoredPosition = basePosition;
        rect.localScale = Vector3.one;
        group.alpha = 1f;
    }

    public static void Show(GameObject panel)
    {
        if (panel == null) return;
        UIPanelFade fade = panel.GetComponent<UIPanelFade>();
        if (panel.activeSelf && fade != null)
        {
            fade.PlayIn(); // war gerade am Ausblenden -> umkehren
        }
        else
        {
            panel.SetActive(true); // OnEnable startet das Einblenden
        }
    }

    public static void Hide(GameObject panel, System.Action onHidden = null)
    {
        if (panel == null) return;
        UIPanelFade fade = panel.GetComponent<UIPanelFade>();
        if (fade == null || !panel.activeInHierarchy)
        {
            panel.SetActive(false);
            onHidden?.Invoke();
            return;
        }
        fade.PlayOut(onHidden);
    }

    private void PlayIn()
    {
        if (!hasBase)
        {
            basePosition = rect.anchoredPosition;
            hasBase = true;
        }
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Animate(true, null));
    }

    private void PlayOut(System.Action onHidden)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Animate(false, onHidden));
    }

    private IEnumerator Animate(bool show, System.Action onHidden)
    {
        float duration = show ? fadeInDuration : fadeOutDuration;
        float fromAlpha = show ? 0f : group.alpha;
        float toAlpha = show ? 1f : 0f;
        group.interactable = show;
        group.blocksRaycasts = show;

        // Bildschirmfuellende Panels (Abdunklung) nur ein-/ausblenden - Gleiten/Wachsen wuerde Raender zeigen.
        bool fullScreen = rect.anchorMin != rect.anchorMax;
        float fromScale = fullScreen ? 1f : startScale;
        float slide = fullScreen ? 0f : slideDistance;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            float p = show ? k : 1f - k; // 0 = versteckt, 1 = sichtbar

            group.alpha = Mathf.Lerp(fromAlpha, toAlpha, k);
            rect.localScale = Vector3.one * Mathf.Lerp(fromScale, 1f, p);
            rect.anchoredPosition = basePosition + Vector2.down * (slide * (1f - p));
            yield return null;
        }

        group.alpha = toAlpha;
        rect.localScale = Vector3.one;
        rect.anchoredPosition = basePosition;
        running = null;

        if (!show)
        {
            gameObject.SetActive(false);
            onHidden?.Invoke();
        }
    }
}
