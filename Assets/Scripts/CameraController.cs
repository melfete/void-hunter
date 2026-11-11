using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public float zOffset = -10f;
    public float minYValue = 0f;

    public float minXValue = 0f; 
    public float maxXValue = 10f; 

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraController: Kein Ziel zugewiesen! Bitte weise ein 'target' im Inspector zu.");
            return;
        }

        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, zOffset);

        desiredPosition.y = Mathf.Max(desiredPosition.y, minYValue);

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minXValue, maxXValue); 

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}