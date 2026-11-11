using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Haengt nach jedem Szenenwechsel automatisch die UI-Animationen an:
//  - UIButtonFeel an alle Buttons
//  - UIPanelFade an alle Menue-Panels (Objekte, deren Name auf "Panel" endet)
// Dadurch muss in den Szenen nichts von Hand eingerichtet werden.
public static class UIPolish
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) => Apply();
    }

    private static void Apply()
    {
        foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (button.GetComponent<UIButtonFeel>() == null)
            {
                button.gameObject.AddComponent<UIButtonFeel>();
            }
        }

        foreach (RectTransform rect in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (rect.name.EndsWith("Panel") && rect.GetComponentInParent<Canvas>(true) != null
                && rect.GetComponent<UIPanelFade>() == null)
            {
                rect.gameObject.AddComponent<UIPanelFade>();
            }
        }
    }
}
