using UnityEngine;

// Gemeinsame Hilfsfunktionen, die von mehreren Menues benutzt werden.
public static class GameUtils
{
    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
