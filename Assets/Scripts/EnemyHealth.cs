using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public float knockbackDuration = 0.2f;

    private EnemyController enemyController;

    public static event Action<GameObject> OnSpecificEnemyDied;
    public bool isBossEnemy = false;

    public GameObject healthBarPrefab;
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    private Image healthBarFillImage;
    private GameObject instantiatedHealthBar;

    public AudioClip hitSound;
    private AudioSource audioSource;


    void Start()
    {
        currentHealth = maxHealth;
        enemyController = GetComponent<EnemyController>();
        audioSource = GetComponent<AudioSource>();

        if (enemyController == null)
        {
            Debug.LogError("EnemyHealth: EnemyController not found on " + gameObject.name + ". Please add one.");
            enabled = false;
        }
        if (audioSource == null)
        {
            Debug.LogWarning("EnemyHealth: AudioSource-Komponente fehlt am Gegner " + gameObject.name + "! Hit-Sounds werden nicht abgespielt.");
        }


        if (healthBarPrefab != null)
        {
            instantiatedHealthBar = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity, transform);
            
            healthBarFillImage = instantiatedHealthBar.transform.Find("HealthBarBackground/HealthBarFill")?.GetComponent<Image>();
            
            if (healthBarFillImage == null)
            {
                Debug.LogWarning("EnemyHealth: HealthBarFill Image not found within healthBarPrefab. Check prefab hierarchy.");
            }
            UpdateHealthBar();
        }
        else
        {
            Debug.LogWarning("EnemyHealth: healthBarPrefab not assigned for " + gameObject.name + ". Health bar will not be displayed.");
        }
    }

    public void TakeDamage(int damage)
    {
        if (enemyController != null && enemyController.IsDead) return;

        currentHealth -= damage;
        
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die(); 
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFillImage != null)
        {
            float healthRatio = (float)currentHealth / maxHealth;
            healthBarFillImage.fillAmount = healthRatio;

            if (healthRatio > 0.5f)
            {
                healthBarFillImage.color = Color.green;
            }
            else if (healthRatio > 0.2f)
            {
                healthBarFillImage.color = Color.yellow;
            }
            else
            {
                healthBarFillImage.color = Color.red;
            }
        }
    }

    public void TakeKnockback(Transform attackerTransform, float knockbackForce)
    {
        if (enemyController != null && enemyController.IsDead) return;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 knockbackDirection = (transform.position - attackerTransform.position).normalized;
            
            if (attackerTransform.position.y > transform.position.y + 0.1f)
            {
                knockbackDirection.y = 0.5f; 
            }

            if (enemyController != null)
            {
                enemyController.enabled = false;
                rb.linearVelocity = Vector2.zero;
                StartCoroutine(ActivateEnemyControllerAfterDelay());
            }

            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody2D auf " + gameObject.name + " NICHT gefunden! Kann keinen Rückstoß anwenden.");
        }
    }

    private IEnumerator ActivateEnemyControllerAfterDelay()
    {
        yield return new WaitForSeconds(knockbackDuration);
        if (enemyController != null && !enemyController.IsDead)
        {
            enemyController.enabled = true;
        }
    }
    
    public void Die()
    {
        if (enemyController != null && enemyController.IsDead) return;
        
        if (isBossEnemy && OnSpecificEnemyDied != null)
        {
            OnSpecificEnemyDied(gameObject); 
        }
        
        if (instantiatedHealthBar != null)
        {
            Destroy(instantiatedHealthBar);
        }
        
        if (enemyController != null)
        {
            enemyController.FinalizeDeath();
        }
        else
        {
            Destroy(gameObject, 0.1f); 
        }
    }
}