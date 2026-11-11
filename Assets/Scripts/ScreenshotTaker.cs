using System;
using System.IO;
using UnityEngine;

// Speichert bei F12 einen Screenshot als PNG im Ordner "Screenshots" im Projektordner
// (im Build: neben der .exe). Das Skript legt sich beim Spielstart selbst an und
// bleibt ueber Szenenwechsel erhalten - es muss an kein Objekt gehaengt werden.
public class ScreenshotTaker : MonoBehaviour
{
    public KeyCode screenshotKey = KeyCode.F12;

    private static ScreenshotTaker instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateInstance()
    {
        if (instance != null) return;

        GameObject go = new GameObject("ScreenshotTaker");
        go.AddComponent<ScreenshotTaker>();
    }

    void Awake()
    {
        // Falls das Skript zusaetzlich von Hand an ein Objekt gehaengt wurde: nur eine Instanz behalten.
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        // Application.dataPath zeigt im Editor auf ".../Void Hunter/Assets" - der Elternordner ist das Projekt.
        string folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Screenshots");
        Directory.CreateDirectory(folder);

        string fileName = "Screenshot_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".png";
        string path = Path.Combine(folder, fileName);

        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Screenshot gespeichert: " + path);
    }
}
