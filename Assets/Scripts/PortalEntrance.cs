using UnityEngine;

// Wird vom PortalManager zur Laufzeit ans Portal gehaengt und meldet, wenn der Spieler es betritt.
public class PortalEntrance : MonoBehaviour
{
    public PortalManager manager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (manager != null && other.CompareTag("Player"))
        {
            manager.OnPlayerEnteredPortal(other.gameObject);
        }
    }
}
