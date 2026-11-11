using UnityEngine;

// Gemeinsame Hilfsfunktionen, die von mehreren Menues benutzt werden.
public static class GameUtils
{
    // Beendet das Spiel bzw. stoppt den Play Mode im Editor.
    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
