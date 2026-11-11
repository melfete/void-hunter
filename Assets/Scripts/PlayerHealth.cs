using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public Image healthBarFillImage;

    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    public float deathYValue = -10f;

    private Rigidbody2D rb;
    private PlayerController playerController;
    private Animator animator;

    public AudioClip hitSound;
    private AudioSource audioSource;

    void Start()
    {
        Debug.Log("PlayerHealth Start: Initializing...");
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (animator == null)
        {
            Debug.LogError("PlayerHealth: Animator-Komponente fehlt!");
        }
        if (audioSource == null)
        {
            Debug.LogWarning("PlayerHealth: AudioSource-Komponente fehlt am Spieler! Hit-Sounds werden nicht abgespielt.");
        }


        if (GameManager.instance != null)
        {
            GameManager.instance.RegisterPlayer(gameObject);
            Debug.Log("PlayerHealth: Registered player with GameManager.");
        }
        else
        {
            Debug.LogError("PlayerHealth: GameManager.instance ist NULL in Start()!");
        }

        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = (float)currentHealth / maxHealth;
        }

        if (animator != null)
        {
            animator.SetBool("IsDead", false);
        }
        Debug.Log("PlayerHealth Start: Finished. Initial Health: " + currentHealth);
    }

    void Update()
    {
        if (currentHealth > 0 && transform.position.y < deathYValue)
        {
            Die();
        }
    }

    public void TakeDamage(int damage, Transform attackerTransform)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + " hat " + damage + " Schaden genommen. Aktuelle Gesundheit: " + currentHealth);

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = (float)currentHealth / maxHealth;
        }

        if (currentHealth <= 0)
        {
            Debug.Log("PlayerHealth: Current health is 0 or less, calling Die().");
            Die();
            return;
        }

        if (rb != null && playerController != null)
        {
            playerController.enabled = false;
            rb.linearVelocity = Vector2.zero;
            Vector2 knockbackDirection = (transform.position - attackerTransform.position).normalized;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            StartCoroutine(ActivatePlayerControllerAfterDelay());
            Debug.Log("PlayerHealth: Applied knockback and disabled controller.");
        }
    }

    private System.Collections.IEnumerator ActivatePlayerControllerAfterDelay()
    {
        yield return new WaitForSeconds(knockbackDuration);
        if (playerController != null && currentHealth > 0)
        {
            playerController.enabled = true;
            rb.linearVelocity = Vector2.zero;
            Debug.Log("PlayerHealth: Controller re-enabled after knockback.");
        }
    }

    public void ResetHealth()
    {
        Debug.Log("PlayerHealth: ResetHealth called.");
        currentHealth = maxHealth;
        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = (float)currentHealth / maxHealth;
        }
        Debug.Log(gameObject.name + "s Gesundheit zurückgesetzt auf: " + currentHealth);

        if (animator != null)
        {
            animator.SetBool("IsDead", false);
        }
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; 
        }
    }
    
    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = (float)currentHealth / maxHealth;
        }
        Debug.Log(gameObject.name + " wurde geheilt um " + amount + ". Aktuelle Gesundheit: " + currentHealth);
    }
    
    void Die()
    {
        // Lebenspunkte auf 0 setzen, damit Die() beim Sturz-Tod nicht jeden Frame erneut ausgeloest wird.
        currentHealth = 0;
        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = 0f;
        }

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            Debug.Log("PlayerHealth: Animator IsDead set to true.");
        }
        else
        {
            Debug.LogError("PlayerHealth: Animator ist NULL in Die()!");
        }

        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("PlayerHealth: PlayerController disabled.");
        }
        else
        {
            Debug.LogError("PlayerHealth: PlayerController ist NULL in Die()!");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; 
            Debug.Log("PlayerHealth: Rigidbody stopped and set to kinematic.");
        }
        else
        {
            Debug.LogError("PlayerHealth: Rigidbody ist NULL in Die()!");
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.PlayerDied();
            Debug.Log("PlayerHealth: GameManager.PlayerDied() called.");
        }
        else
        {
            Debug.LogError("PlayerHealth: GameManager.instance ist NULL in Die()! Zerstöre GameObject als Fallback.");
            Destroy(gameObject);
        }
    }
}