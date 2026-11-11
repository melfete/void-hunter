using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Sorgt dafuer, dass "Play" im Editor immer mit der ersten Szene der Build-Liste (Hauptmenue) startet -
// egal, welche Szene gerade geoeffnet ist. Nach dem Beenden ist wieder die bearbeitete Szene offen.
// Ein-/Ausschalten: Menue "Void Hunter > Play startet im Hauptmenue".
[InitializeOnLoad]
public static class PlayFromMainMenu
{
    private const string MenuPath = "Void Hunter/Play startet im Hauptmenue";
    private const string PrefKey = "VoidHunter.PlayFromMainMenu";

    static PlayFromMainMenu()
    {
        // Erst nach dem Laden des Editors anwenden, wenn die Build-Einstellungen verfuegbar sind.
        EditorApplication.delayCall += Apply;
    }

    private static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, true);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        Enabled = !Enabled;
        Apply();
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    private static void Apply()
    {
        SceneAsset startScene = null;
        if (Enabled && EditorBuildSettings.scenes.Length > 0)
        {
            startScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[0].path);
            if (startScene == null)
            {
                Debug.LogWarning("PlayFromMainMenu: Erste Szene der Build-Liste nicht gefunden.");
            }
        }
        EditorSceneManager.playModeStartScene = startScene; // null = aktuelle Szene (Unity-Standard)
    }
}
