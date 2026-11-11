using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Gibt Buttons ein weiches Gefuehl: leicht groesser beim Hovern/Auswaehlen, kurz kleiner beim Klicken.
// Wird von UIPolish automatisch an alle Buttons gehaengt. Laeuft auch bei Pause (timeScale 0).
public class UIButtonFeel : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    public float hoverScale = 1.05f;
    public float pressedScale = 0.96f;
    public float smoothing = 18f; // hoeher = schneller

    private Selectable selectable;
    private bool hovered;
    private bool selected;
    private bool pressed;

    void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    void OnDisable()
    {
        hovered = selected = pressed = false;
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        bool interactable = selectable == null || selectable.IsInteractable();
        float target = 1f;
        if (interactable && pressed) target = pressedScale;
        else if (interactable && (hovered || selected)) target = hoverScale;

        float s = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime));
        transform.localScale = new Vector3(s, s, 1f);
    }

    public void OnPointerEnter(PointerEventData e) { hovered = true; }
    public void OnPointerExit(PointerEventData e) { hovered = false; pressed = false; }
    public void OnPointerDown(PointerEventData e) { pressed = true; }
    public void OnPointerUp(PointerEventData e) { pressed = false; }
    public void OnSelect(BaseEventData e) { selected = e is not PointerEventData; } // nur Tastatur/Gamepad
    public void OnDeselect(BaseEventData e) { selected = false; }
}
