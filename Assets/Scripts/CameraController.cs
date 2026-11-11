using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    [Tooltip("Ungefaehre Zeit in Sekunden, die die Kamera braucht, um das Ziel einzuholen. Kleiner = straffer.")]
    public float smoothTime = 0.15f;
    public float zOffset = -10f;
    public float minYValue = 0f;

    public float minXValue = 0f;
    public float maxXValue = 10f;

    private Vector3 velocity;
    private bool warnedMissingTarget;

    void LateUpdate()
    {
        if (target == null)
        {
            if (!warnedMissingTarget)
            {
                Debug.LogWarning("CameraController: Kein Ziel zugewiesen! Bitte weise ein 'target' im Inspector zu.");
                warnedMissingTarget = true;
            }
            return;
        }

        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, zOffset);
        desiredPosition.y = Mathf.Max(desiredPosition.y, minYValue);
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minXValue, maxXValue);

        // SmoothDamp ist unabhaengig von der Framerate (Lerp mit festem Faktor nicht).
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
