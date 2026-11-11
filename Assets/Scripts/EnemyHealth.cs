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
    private Rigidbody2D rb;

    public static event Action<GameObject> OnSpecificEnemyDied;
    public bool isBossEnemy = false;

    // Optional: eigenes UI-Prefab. Ist keins zugewiesen, wird ein einfacher Balken per Code gebaut.
    public GameObject healthBarPrefab;
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    public Vector2 healthBarSize = new Vector2(1f, 0.12f);
    public bool showHealthBarOnlyWhenDamaged = true;
    private Image healthBarFillImage;
    private GameObject instantiatedHealthBar;

    // Per Code gebauter Balken (folgt dem Gegner, wird aber nicht mit ihm gespiegelt).
    private Transform builtBar;
    private Transform builtBarFill;
    private SpriteRenderer builtBarFillRenderer;
    private static Sprite pixelSprite;

    public AudioClip hitSound;
    private AudioSource audioSource;

    // Kurzes Aufblinken bei Treffern (es gibt keine eigenen Treffer-Sprites fuer Gegner).
    public Color hitFlashColor = new Color(1f, 0.35f, 0.35f, 1f);
    public float hitFlashDuration = 0.12f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashRoutine;


    public bool IsAlive => currentHealth > 0;

    void Awake()
    {
        // In Awake, damit IsAlive schon stimmt, bevor andere Skripte in Start() nachfragen.
        currentHealth = maxHealth;
    }

    void Start()
    {
        enemyController = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();
        // Liefert die im Prefab angelegte AudioSource; die zur Laufzeit ergaenzte Schleifen-Quelle kommt erst danach.
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

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
        }
        else
        {
            BuildHealthBar();
        }
        UpdateHealthBar();
    }

    void LateUpdate()
    {
        if (builtBar != null)
        {
            builtBar.position = transform.position + healthBarOffset;
        }
    }

    void OnDestroy()
    {
        if (builtBar != null)
        {
            Destroy(builtBar.gameObject);
        }
    }

    // Baut einen kleinen Balken aus zwei Sprites: dunkler Rahmen + farbige Fuellung.
    private void BuildHealthBar()
    {
        if (pixelSprite == null)
        {
            Texture2D tex = Texture2D.whiteTexture;
            // Pivot links-mittig, damit die Fuellung von rechts nach links schrumpft. 1 Sprite = 1x1 Einheit.
            pixelSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0f, 0.5f), tex.width);
        }

        int sortingLayer = spriteRenderer != null ? spriteRenderer.sortingLayerID : 0;
        int sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;
        float border = healthBarSize.y * 0.25f;

        builtBar = new GameObject("HealthBar_" + gameObject.name).transform;
        builtBar.position = transform.position + healthBarOffset;

        SpriteRenderer bg = CreateBarPart("Background", new Color(0.1f, 0.1f, 0.1f, 0.85f), sortingLayer, sortingOrder + 10);
        bg.transform.localPosition = new Vector3(-healthBarSize.x / 2f - border, 0f, 0f);
        bg.transform.localScale = new Vector3(healthBarSize.x + border * 2f, healthBarSize.y + border * 2f, 1f);

        builtBarFillRenderer = CreateBarPart("Fill", Color.green, sortingLayer, sortingOrder + 11);
        builtBarFill = builtBarFillRenderer.transform;
        builtBarFill.localPosition = new Vector3(-healthBarSize.x / 2f, 0f, 0f);
        builtBarFill.localScale = new Vector3(healthBarSize.x, healthBarSize.y, 1f);
    }

    private SpriteRenderer CreateBarPart(string partName, Color color, int sortingLayer, int sortingOrder)
    {
        GameObject part = new GameObject(partName);
        part.transform.SetParent(builtBar, false);
        SpriteRenderer sr = part.AddComponent<SpriteRenderer>();
        sr.sprite = pixelSprite;
        sr.color = color;
        sr.sortingLayerID = sortingLayer;
        sr.sortingOrder = sortingOrder;
        return sr;
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
        else
        {
            FlashOnHit();
        }
    }

    private void FlashOnHit()
    {
        if (spriteRenderer == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
        flashRoutine = null;
    }

    void UpdateHealthBar()
    {
        float healthRatio = Mathf.Clamp01((float)currentHealth / maxHealth);
        Color barColor = healthRatio > 0.5f ? Color.green : (healthRatio > 0.2f ? Color.yellow : Color.red);
        bool visible = !showHealthBarOnlyWhenDamaged || healthRatio < 1f;

        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = healthRatio;
            healthBarFillImage.color = barColor;
        }
        if (instantiatedHealthBar != null)
        {
            instantiatedHealthBar.SetActive(visible);
        }

        if (builtBar != null)
        {
            builtBarFill.localScale = new Vector3(healthBarSize.x * healthRatio, healthBarSize.y, 1f);
            builtBarFillRenderer.color = barColor;
            builtBar.gameObject.SetActive(visible);
        }
    }

    public void TakeKnockback(Transform attackerTransform, float knockbackForce)
    {
        if (enemyController != null && enemyController.IsDead) return;

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
        if (builtBar != null)
        {
            Destroy(builtBar.gameObject);
            builtBar = null;
        }

        // Blinken abbrechen, damit das Ausblenden mit der Originalfarbe startet.
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        
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