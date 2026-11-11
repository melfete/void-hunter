using UnityEngine;
using UnityEngine.UI;

// Hintergrund fuers Hauptmenue aus den Parallax-Ebenen des Spiels (Himmel, Wolken, Berge, Wald, Bueschen).
// Die Ebenen bewegen sich sanft: langsames Schwingen plus leichte Parallaxe zur Mausposition.
// Liegt am Menue-Canvas; die Sprites werden im Inspector zugewiesen (hinten -> vorne).
public class MenuBackdrop : MonoBehaviour
{
    public Sprite[] layers;              // Reihenfolge: hinterste Ebene zuerst
    public float swayAmount = 14f;       // Pixel (bei 1920x1080) fuer die vorderste Ebene
    public float swaySpeed = 0.12f;
    public float mouseParallax = 22f;    // Pixel fuer die vorderste Ebene
    public float smoothing = 3f;

    private RectTransform[] layerRects;
    private Vector2 mouseOffset;

    void Start()
    {
        if (layers == null || layers.Length == 0) return;

        RectTransform root = new GameObject("Backdrop", typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(transform, false);
        root.SetAsFirstSibling(); // hinter allen Menues
        Stretch(root, 0f);

        layerRects = new RectTransform[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            GameObject go = new GameObject("Layer" + i, typeof(RectTransform));
            go.transform.SetParent(root, false);
            Image img = go.AddComponent<Image>();
            img.sprite = layers[i];
            img.raycastTarget = false;
            layerRects[i] = img.rectTransform;
            // etwas groesser als der Bildschirm, damit beim Bewegen keine Raender sichtbar werden
            Stretch(layerRects[i], 40f);
        }
    }

    void Update()
    {
        if (layerRects == null) return;

        Vector2 mouse = new Vector2(Input.mousePosition.x / Mathf.Max(1, Screen.width) - 0.5f,
                                    Input.mousePosition.y / Mathf.Max(1, Screen.height) - 0.5f);
        mouseOffset = Vector2.Lerp(mouseOffset, mouse, 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime));

        float sway = Mathf.Sin(Time.unscaledTime * swaySpeed * Mathf.PI * 2f);
        for (int i = 0; i < layerRects.Length; i++)
        {
            float depth = layerRects.Length > 1 ? (float)i / (layerRects.Length - 1) : 1f; // 0 hinten, 1 vorne
            Vector2 offset = new Vector2(sway * swayAmount * depth, 0f) - mouseOffset * mouseParallax * depth;
            layerRects[i].anchoredPosition = offset;
        }
    }

    private static void Stretch(RectTransform rt, float overscan)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-overscan, -overscan);
        rt.offsetMax = new Vector2(overscan, overscan);
    }
}
