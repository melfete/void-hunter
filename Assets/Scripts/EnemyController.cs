using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    private static readonly int AnimIsDead = Animator.StringToHash("IsDead");
    private static readonly int AnimIsAttacking = Animator.StringToHash("IsAttacking");

    public float moveSpeed = 2f;
    public Transform[] patrolPoints;
    public float stoppingDistance = 0.5f;

    public int playerDamage = 10;

    public bool isFloatingEnemy;
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 1f;

    public Transform playerTransform;
    public float chaseRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public float attackAnimationDuration = 0.5f;
    public float returnToPatrolRange = 8f;
    public float deathYValue = 380.99f;


    public Transform enemyGroundCheck;
    public float enemyGroundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public float platformCheckDistance = 0.8f;
    public float jumpDetectionHeight = 0.5f;
    public float enemyJumpForce = 8f;
    public LayerMask platformLayer;

    public GameObject enemyFireballPrefab;
    public Transform enemyFirePoint;
    public float enemyFireballSpeed = 10f;

    public bool IsDead { get; private set; }

    private enum EnemyState { Patrol, Chase, Attack, Return, Dead }
    private EnemyState _currentState;

    private int _currentPatrolPointIndex;
    private Rigidbody2D _rb;
    private float nextAttackTime;

    private Animator animator;
    private bool isAttackingPlayer;
    private bool isEnemyGrounded;
    private Vector3 originalPosition;

    private AudioSource audioSource;      // Einmal-Sounds (Angriff, Tod, Schuss, Treffer)
    private AudioSource loopSource;       // Lauf-/Schwebe-Schleife, separat damit Stop() keine Einmal-Sounds abschneidet
    public AudioClip runSound;
    public AudioClip floatLoopSound;
    public AudioClip attackSound;
    public AudioClip deathSound;
    public AudioClip shootProjectileSound;

    private bool enemyFacingRight;

    private EnemyHealth enemyHealth;


    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (_rb == null)
        {
            Debug.LogError("EnemyController: Rigidbody2D not found on " + gameObject.name + ". Please add one.");
            enabled = false;
            return;
        }

        if (animator == null)
        {
            Debug.LogError("EnemyController: Animator not found on " + gameObject.name + ". Please add one.");
            enabled = false;
            return;
        }
        if (audioSource == null)
        {
            Debug.LogWarning("EnemyController: AudioSource-Komponente fehlt am Gegner " + gameObject.name + "! Sounds werden nicht abgespielt.");
        }
        else
        {
            loopSource = CreateLoopSource(audioSource);
        }
        if (enemyHealth == null)
        {
            Debug.LogError("EnemyController: EnemyHealth-Komponente fehlt an " + gameObject.name + ". Tod wird nicht korrekt initialisiert.");
        }


        // Der im Inspector zugewiesene FirePoint hat Vorrang (z.B. "WizardFirePoint" bei Enemy3).
        if (enemyFirePoint == null)
        {
            enemyFirePoint = transform.Find("EnemyFirePoint");
        }
        if (enemyFirePoint == null && enemyFireballPrefab != null)
        {
            Debug.LogError("EnemyController: " + gameObject.name + " hat ein Feuerball-Prefab, aber keinen FirePoint. Bitte 'Enemy Fire Point' im Inspector zuweisen.");
        }


        if (playerTransform == null)
        {
            GameObject playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                playerTransform = playerGO.transform;
            }
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("EnemyController: No patrol points assigned. Enemy will only chase or remain in its 'running' state.");
            patrolPoints = new Transform[1];
            GameObject defaultPatrolPoint = new GameObject("DefaultPatrolPoint_" + gameObject.name);
            defaultPatrolPoint.transform.position = transform.position;
            defaultPatrolPoint.transform.parent = transform.parent;
            patrolPoints[0] = defaultPatrolPoint.transform;
            _currentState = EnemyState.Patrol;
        }
        else
        {
            transform.position = patrolPoints[_currentPatrolPointIndex].position;
            _currentState = EnemyState.Patrol;
        }
        
        originalPosition = transform.position;
        nextAttackTime = Time.time;

        if (isFloatingEnemy)
        {
            _rb.gravityScale = 0;
            _rb.linearVelocity = Vector2.zero;
        }

        if (enemyGroundCheck == null && !isFloatingEnemy)
        {
            Debug.LogWarning("EnemyController: EnemyGroundCheck not assigned. Creating a default one.");
            GameObject gc = new GameObject("EnemyGroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0, -0.6f, 0);
            enemyGroundCheck = gc.transform;
        }

        UpdateEnemyFacingDirection(enemyFacingRight); 
    }

    // Gesamte KI im Physik-Takt, da sie in jedem Schritt Geschwindigkeiten setzt.
    void FixedUpdate()
    {
        if (IsDead)
        {
            _rb.linearVelocity = Vector2.zero;
            animator.SetBool(AnimIsDead, true);
            StopEnemyMovementSounds();
            return;
        }

        if (transform.position.y < deathYValue)
        {
       
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(enemyHealth.maxHealth + 1); 
            }
          
        }

        animator.SetBool(AnimIsAttacking, isAttackingPlayer);

        if (_rb.linearVelocity.x > 0.01f && !enemyFacingRight)
        {
            UpdateEnemyFacingDirection(true);
        }
        else if (_rb.linearVelocity.x < -0.01f && enemyFacingRight)
        {
            UpdateEnemyFacingDirection(false);
        }
        
        if (!isFloatingEnemy)
        {
            isEnemyGrounded = Physics2D.OverlapCircle(enemyGroundCheck.position, enemyGroundCheckRadius, groundLayer);
        }

        float distanceToPlayer = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;

        switch (_currentState)
        {
            case EnemyState.Patrol:
                HandlePatrolState(distanceToPlayer);
                break;
            case EnemyState.Chase:
                HandleChaseState(distanceToPlayer);
                break;
            case EnemyState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
            case EnemyState.Return:
                HandleReturnState(distanceToPlayer);
                break;
            case EnemyState.Dead:
                break;
        }
        UpdateMovementSounds();
    }

    private void UpdateEnemyFacingDirection(bool newFacingRight)
    {
        if (enemyFacingRight == newFacingRight) return;

        enemyFacingRight = newFacingRight;

        // Das Spiegeln ueber localScale spiegelt automatisch auch alle Kindobjekte
        // (z.B. EnemyFirePoint) mit - deren Position darf nicht zusaetzlich negiert werden.
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * (enemyFacingRight ? -1f : 1f);
        transform.localScale = newScale;
    }


    bool IsPlatformAhead()
    {
        float lookDirectionX = transform.localScale.x > 0 ? -1 : 1;
        Vector2 raycastOrigin = (Vector2)transform.position + new Vector2(lookDirectionX * 0.1f, jumpDetectionHeight);

        RaycastHit2D hitPlatform = Physics2D.Raycast(raycastOrigin, Vector2.right * lookDirectionX, platformCheckDistance, platformLayer);
        
        return hitPlatform.collider != null && hitPlatform.collider.gameObject != gameObject;
    }

    void HandlePatrolState(float distanceToPlayer)
    {
        if (playerTransform != null && distanceToPlayer <= chaseRange)
        {
            _currentState = EnemyState.Chase;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetPosition = patrolPoints[_currentPatrolPointIndex].position;
        Vector2 directionToTarget = (targetPosition - (Vector2)transform.position).normalized;
        
        if (isFloatingEnemy)
        {
            _rb.linearVelocity = directionToTarget * moveSpeed;

            float newY = originalPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, (newY - transform.position.y) * floatSpeed);
        }
        else
        {
            if (isEnemyGrounded && IsPlatformAhead())
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, enemyJumpForce);
                return; 
            }
            _rb.linearVelocity = new Vector2(directionToTarget.x * moveSpeed, _rb.linearVelocity.y);
        }

        if (Vector2.Distance(transform.position, targetPosition) < stoppingDistance)
        {
            _currentPatrolPointIndex = (_currentPatrolPointIndex + 1) % patrolPoints.Length;
        }
    }

    void HandleChaseState(float distanceToPlayer)
    {
        if (playerTransform == null)
        {
            _currentState = EnemyState.Return;
            return;
        }

        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            _currentState = EnemyState.Attack;
            return;
        }

        if (distanceToPlayer > returnToPatrolRange)
        {
            _currentState = EnemyState.Return;
            return;
        }

        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        if (isFloatingEnemy)
        {
            _rb.linearVelocity = directionToPlayer * moveSpeed;

            float newY = originalPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, (newY - transform.position.y) * floatSpeed);
        }
        else
        {
            if (isEnemyGrounded && IsPlatformAhead())
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, enemyJumpForce);
                return;
            }
            _rb.linearVelocity = new Vector2(directionToPlayer.x * moveSpeed, _rb.linearVelocity.y);
        }
    }

    void HandleAttackState(float distanceToPlayer)
    {
        _rb.linearVelocity = Vector2.zero;
        isAttackingPlayer = true;
        StopEnemyMovementSounds();

        if (playerTransform == null)
        {
            _currentState = EnemyState.Return;
            isAttackingPlayer = false;
            return;
        }

        if (distanceToPlayer > attackRange)
        {
            _currentState = EnemyState.Chase;
            isAttackingPlayer = false;
            return;
        }
        
        if (Time.time >= nextAttackTime)
        {
            animator.SetBool(AnimIsAttacking, true); 
            if (audioSource != null && attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
            nextAttackTime = Time.time + attackCooldown;
            StartCoroutine(ResetAttackState());
        }
    }

    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(attackAnimationDuration); 
        isAttackingPlayer = false;
        animator.SetBool(AnimIsAttacking, false);
    }

    public void ShootEnemyFireball()
    {
        if (enemyFireballPrefab == null || enemyFirePoint == null)
        {
            Debug.LogWarning("EnemyFireballPrefab or EnemyFirePoint is null. Cannot shoot fireball.");
            return;
        }

        Vector2 fireDirection = enemyFacingRight ? Vector2.right : Vector2.left;

        GameObject fireballGO = Instantiate(enemyFireballPrefab, enemyFirePoint.position, Quaternion.identity);
        
        if (fireDirection.x < 0)
        {
            fireballGO.transform.localScale = new Vector3(Mathf.Abs(fireballGO.transform.localScale.x) * -1f, fireballGO.transform.localScale.y, fireballGO.transform.localScale.z);
        }
        else
        {
            fireballGO.transform.localScale = new Vector3(Mathf.Abs(fireballGO.transform.localScale.x) * 1f, fireballGO.transform.localScale.y, fireballGO.transform.localScale.z);
        }


        Rigidbody2D fbRb = fireballGO.GetComponent<Rigidbody2D>();
        if (fbRb != null)
        {
            fbRb.linearVelocity = fireDirection * enemyFireballSpeed;
        }
        Projectile projectileScript = fireballGO.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDirection(fireDirection);
        }

        if (audioSource != null && shootProjectileSound != null)
        {
            audioSource.PlayOneShot(shootProjectileSound);
        }
    }

    void HandleReturnState(float distanceToPlayer)
    {
        if (playerTransform != null && distanceToPlayer <= chaseRange)
        {
            _currentState = EnemyState.Chase;
            return;
        }

        Vector2 returnTarget = patrolPoints.Length > 0 ? (Vector2)patrolPoints[0].position : (Vector2)originalPosition;
        Vector2 directionToReturn = (returnTarget - (Vector2)transform.position).normalized;
        
        if (isFloatingEnemy)
        {
            _rb.linearVelocity = directionToReturn * moveSpeed;

            float newY = originalPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, (newY - transform.position.y) * floatSpeed);
        }
        else
        {
            if (isEnemyGrounded && IsPlatformAhead())
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, enemyJumpForce);
                return;
            }
            _rb.linearVelocity = new Vector2(directionToReturn.x * moveSpeed, _rb.linearVelocity.y);
        }

        if (Vector2.Distance(transform.position, returnTarget) < stoppingDistance)
        {
            _currentState = EnemyState.Patrol;
            _rb.linearVelocity = Vector2.zero; 
        }
    }
    
    public void FinalizeDeath()
    {
        if (IsDead) return; 

        IsDead = true;
        _currentState = EnemyState.Dead;
        animator.SetBool(AnimIsDead, true);
        StopEnemyMovementSounds();

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        _rb.linearVelocity = Vector2.zero;
        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }
        
        Destroy(gameObject, 2f); 
    }

    private AudioSource CreateLoopSource(AudioSource template)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.outputAudioMixerGroup = template.outputAudioMixerGroup;
        source.volume = template.volume;
        source.pitch = template.pitch;
        source.spatialBlend = template.spatialBlend;
        source.rolloffMode = template.rolloffMode;
        source.minDistance = template.minDistance;
        source.maxDistance = template.maxDistance;
        source.priority = template.priority;
        return source;
    }

    void UpdateMovementSounds()
    {
        if (loopSource == null || IsDead || isAttackingPlayer) return;

        bool isMoving = _rb.linearVelocity.magnitude > 0.01f;
        AudioClip wantedClip = null;

        if (isFloatingEnemy)
        {
            if (isMoving) wantedClip = floatLoopSound;
        }
        else if (isEnemyGrounded && isMoving)
        {
            wantedClip = runSound;
        }

        if (wantedClip == null)
        {
            StopEnemyMovementSounds();
        }
        else if (loopSource.clip != wantedClip || !loopSource.isPlaying)
        {
            loopSource.clip = wantedClip;
            loopSource.Play();
        }
    }

    // Stoppt nur die Bewegungs-Schleife, nie die Einmal-Sounds.
    void StopEnemyMovementSounds()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(playerDamage, transform);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(playerDamage, transform);
            }
        }
    }
}