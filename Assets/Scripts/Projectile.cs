using UnityEngine;

// Wird fuer Spieler-Projektile und Gegner-Feuerbaelle benutzt.
// Wen ein Projektil trifft, wird ueber "hitLayers" im Prefab festgelegt
// (Spieler-Projektil: Enemy + Wall, Gegner-Feuerball: Player + Wall).
public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int projectileDamage = 10;
    public float projectileKnockbackForce = 5f;
    public float lifetime = 2f;
    public LayerMask hitLayers;

    private Rigidbody2D rb;
    private bool hasHit;

    // Awake statt Start: SetDirection wird direkt nach Instantiate aufgerufen,
    // bevor Start laufen wuerde.
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Projectile: Rigidbody2D fehlt an " + gameObject.name + "!");
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 direction)
    {
        if (rb == null) return;
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other)
    {
        if (hasHit) return;
        if (((1 << other.gameObject.layer) & hitLayers) == 0) return;

        hasHit = true;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(projectileDamage);
            enemyHealth.TakeKnockback(transform, projectileKnockbackForce);
        }
        else
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(projectileDamage, transform);
            }
        }

        Destroy(gameObject);
    }
}
