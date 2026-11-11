using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // Animator-Parameter einmal als Hash berechnen (schneller als Strings in jedem Frame).
    private static readonly int AnimDashLeft = Animator.StringToHash("DashLeft");
    private static readonly int AnimDashRight = Animator.StringToHash("DashRight");
    private static readonly int AnimFacingRight = Animator.StringToHash("FacingRight");
    private static readonly int AnimHealingLeft = Animator.StringToHash("HealingLeft");
    private static readonly int AnimHealingRight = Animator.StringToHash("HealingRight");
    private static readonly int AnimIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int AnimIsJumping = Animator.StringToHash("IsJumping");
    private static readonly int AnimIsMovingUp = Animator.StringToHash("IsMovingUp");
    private static readonly int AnimIsRunning = Animator.StringToHash("IsRunning");

    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 20;
    public float playerAttackKnockbackForce = 10f;

    public float wallSlidingSpeed = 2f;
    public Transform wallCheck;
    public LayerMask wallLayer;
    public float wallJumpForceX = 10f;
    public float wallJumpForceY = 15f;
    public float wallJumpTime = 0.2f;

    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;

    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileCooldown = 0.5f;
    private float nextProjectileTime = 0f;

    public int healAmount = 20;
    public float healCooldown = 5f;
    private float nextHealTime = 0f;
    private bool isHealing;

    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private float nextDashTime = 0f;
    private bool isDashing;

    public float verticalMoveSpeed = 3f;

    private Rigidbody2D rb;
    private float originalGravityScale = 1f;
    private bool isGrounded;
    private Animator animator;
    public bool facingRight = true;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpTimer;
    private bool isAttacking;
    private bool isMovingUp; 
    private bool isInPortalTrigger;
    private float moveInput;
    private float verticalInput;

    private float originalAttackPointLocalX;
    private float originalFirePointLocalX;
    private float originalWallCheckLocalX;

    private PlayerHealth playerHealth;

    private AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip attackSound;
    public AudioClip fireProjectileSound;
    public AudioClip healSound;
    public AudioClip dashSound;

    public List<AudioClip> grassStepSounds;
    public float timeBetweenSteps = 0.3f;
    private float nextStepTime = 0f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (rb == null || animator == null)
        {
            Debug.LogError("PlayerController: Rigidbody2D oder Animator fehlt am Spieler!");
            enabled = false;
            return;
        }
        if (groundCheck == null)
        {
            Debug.LogError("PlayerController: GroundCheck ist im Inspector nicht zugewiesen!");
            enabled = false;
            return;
        }
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource-Komponente fehlt am Spieler! Sounds werden nicht abgespielt.");
        }

        Transform foundAttackPoint = transform.Find("AttackPoint");
        if (foundAttackPoint != null)
        {
            attackPoint = foundAttackPoint;
            originalAttackPointLocalX = attackPoint.localPosition.x;
        }
        else
        {
            Debug.LogError("AttackPoint fehlt als Kindobjekt. Bitte stellen Sie sicher, dass ein GameObject namens 'AttackPoint' ein Kind von diesem Spieler ist.");
            enabled = false; 
            return;
        }

        Transform foundFirePoint = transform.Find("FirePoint");
        if (foundFirePoint != null)
        {
            firePoint = foundFirePoint;
            originalFirePointLocalX = firePoint.localPosition.x;
        }
        else
        {
            Debug.LogError("FirePoint fehlt als Kindobjekt. Bitte stellen Sie sicher, dass ein GameObject namens 'FirePoint' ein Kind von diesem Spieler ist.");
            enabled = false; 
            return;
        }

        Transform foundWallCheck = transform.Find("WallCheck");
        if (foundWallCheck != null)
        {
            wallCheck = foundWallCheck;
            originalWallCheckLocalX = wallCheck.localPosition.x;
        }
        else
        {
            Debug.LogError("WallCheck fehlt als Kindobjekt. Bitte stellen Sie sicher, dass ein GameObject namens 'WallCheck' ein Kind von diesem Spieler ist.");
            enabled = false;
            return;
        }

        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth-Komponente fehlt am Spieler-Objekt!");
            enabled = false; 
        }

        animator.SetBool(AnimFacingRight, facingRight);
        UpdateChildLocalPositions();
    }

    void Update()
    {
        // Im Pausemenue keine Eingaben verarbeiten (Input funktioniert auch bei timeScale 0).
        if (PauseMenu.isPaused) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Eingaben werden hier gelesen und in FixedUpdate fuer die Physik verwendet.
        moveInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        bool oldFacingRight = facingRight;
        if (!isDashing && !isAttacking)
        {
            if (moveInput > 0 && !facingRight)
            {
                facingRight = true;
            }
            else if (moveInput < 0 && facingRight)
            {
                facingRight = false;
            }
        }

        if (oldFacingRight != facingRight)
        {
            UpdateChildLocalPositions();
        }
        animator.SetBool(AnimFacingRight, facingRight);

        bool isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
        isWallSliding = isTouchingWall && !isGrounded && moveInput != 0 && !isDashing && !isAttacking;

        if (isWallSliding)
        {
            wallJumpTimer = wallJumpTime;
        }
        else if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.deltaTime;
        }

        // Einmalige Impulse (Spruenge) bleiben in Update, damit kein Tastendruck verloren geht.
        if (Input.GetButtonDown("Jump") && wallJumpTimer > 0 && !isGrounded && !isDashing && !isAttacking)
        {
            isWallJumping = true;
            wallJumpTimer = 0;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallJumpForceY);

            float wallJumpDirX = facingRight ? -1 : 1;
            rb.AddForce(new Vector2(wallJumpDirX * wallJumpForceX, 0), ForceMode2D.Impulse);

            StartCoroutine(DisableHorizontalMovementAfterWallJump());
            PlaySound(jumpSound);
        }

        animator.SetBool(AnimIsRunning, Mathf.Abs(moveInput) > 0.01f && !isDashing && !isAttacking && !isMovingUp);
        animator.SetBool(AnimIsJumping, !isGrounded);
        animator.SetBool(AnimIsAttacking, isAttacking);
        animator.SetBool(AnimIsMovingUp, isMovingUp);

        if (!isWallJumping && Input.GetButtonDown("Jump") && isGrounded && !isDashing && !isAttacking)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool(AnimIsJumping, true);
            PlaySound(jumpSound);
        }

        if (isGrounded && Mathf.Abs(moveInput) > 0.01f && !isDashing && !isAttacking && !isMovingUp && Time.time >= nextStepTime)
        {
            if (grassStepSounds != null && grassStepSounds.Count > 0)
            {
                PlaySound(grassStepSounds[Random.Range(0, grassStepSounds.Count)]);
                nextStepTime = Time.time + timeBetweenSteps;
            }
        }

        if (Time.time >= nextAttackTime && Input.GetButtonDown("Fire1") && !isDashing && !isAttacking)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }

        if (Time.time >= nextProjectileTime && Input.GetButtonDown("Fire2") && !isDashing && !isAttacking)
        {
            FireProjectile();
            nextProjectileTime = Time.time + projectileCooldown;
        }

        if (Input.GetKeyDown(KeyCode.H) && Time.time >= nextHealTime && !isHealing && !isDashing && !isAttacking)
        {
            if (playerHealth != null)
            {
                Heal();
                nextHealTime = Time.time + healCooldown;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextDashTime && !isDashing && !isWallSliding && !isAttacking)
        {
            Dash();
            nextDashTime = Time.time + dashCooldown;
        }
    }

    // Laufende Bewegung im Physik-Takt: unabhaengig von der Framerate.
    void FixedUpdate()
    {
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlidingSpeed));
        }

        if (isDashing || isWallJumping)
        {
            return; // Geschwindigkeit wurde beim Dash/Wall-Jump einmalig gesetzt.
        }

        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else if (isInPortalTrigger && Mathf.Abs(verticalInput) > 0.01f)
        {
            rb.linearVelocity = new Vector2(0f, verticalInput * verticalMoveSpeed);
            isMovingUp = true;
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            isMovingUp = false;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void UpdateChildLocalPositions()
    {
        float directionMultiplier = facingRight ? 1f : -1f;

        if (attackPoint != null)
        {
            attackPoint.localPosition = new Vector3(originalAttackPointLocalX * directionMultiplier, attackPoint.localPosition.y, attackPoint.localPosition.z);
        }
        if (firePoint != null)
        {
            firePoint.localPosition = new Vector3(originalFirePointLocalX * directionMultiplier, firePoint.localPosition.y, firePoint.localPosition.z);
        }
        if (wallCheck != null)
        {
            wallCheck.localPosition = new Vector3(originalWallCheckLocalX * directionMultiplier, wallCheck.localPosition.y, wallCheck.localPosition.z);
        }
    }

    private IEnumerator DisableHorizontalMovementAfterWallJump()
    {
        yield return new WaitForSeconds(wallJumpTime);
        isWallJumping = false;
    }

    void Attack()
    {
        isAttacking = true;
        animator.SetBool(AnimIsAttacking, true);
        StartCoroutine(ResetAttackAnimation());

        if (attackPoint == null)
        {
            Debug.LogWarning("AttackPoint ist null. Angriff kann nicht durchgeführt werden.");
            return;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage); 
                enemyHealth.TakeKnockback(transform, playerAttackKnockbackForce);
            }
        }
        PlaySound(attackSound);
    }

    private IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.3f); 
        isAttacking = false;
        animator.SetBool(AnimIsAttacking, false);
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("ProjectilePrefab oder FirePoint ist null. Projektil kann nicht abgefeuert werden.");
            return;
        }

        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectileScript = projectileGO.GetComponent<Projectile>();

        if (projectileScript != null)
        {
            Vector2 fireDirection = facingRight ? Vector2.right : Vector2.left;
            projectileScript.SetDirection(fireDirection);
        }
        else
        {
            Debug.LogWarning("ProjektilPrefab hat kein 'Projectile'-Skript!");
        }
        PlaySound(fireProjectileSound);
    }

    void Heal()
    {
        if (playerHealth != null)
        {
            isHealing = true;
            if (facingRight)
            {
                animator.SetBool(AnimHealingRight, true);
                animator.SetBool(AnimHealingLeft, false);
            }
            else
            {
                animator.SetBool(AnimHealingLeft, true);
                animator.SetBool(AnimHealingRight, false);
            }
            
            StartCoroutine(PerformHealing());
            PlaySound(healSound);
        }
    }

    private IEnumerator PerformHealing()
    {
        yield return new WaitForSeconds(1.0f);

        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount); 
        }
        
        animator.SetBool(AnimHealingRight, false);
        animator.SetBool(AnimHealingLeft, false);
        isHealing = false;
    }

    void Dash()
    {
        isDashing = true;
        rb.linearVelocity = Vector2.zero; 
        rb.gravityScale = 0f; 

        float dashDirection = facingRight ? 1f : -1f;

        if (facingRight)
        {
            if (attackPoint != null) attackPoint.localPosition = new Vector3(Mathf.Abs(originalAttackPointLocalX), attackPoint.localPosition.y, attackPoint.localPosition.z);
            if (firePoint != null) firePoint.localPosition = new Vector3(Mathf.Abs(originalFirePointLocalX), firePoint.localPosition.y, firePoint.localPosition.z);
            if (wallCheck != null) wallCheck.localPosition = new Vector3(Mathf.Abs(originalWallCheckLocalX), wallCheck.localPosition.y, wallCheck.localPosition.z);
            animator.SetBool(AnimDashRight, true);
            animator.SetBool(AnimDashLeft, false);
        }
        else
        {
            if (attackPoint != null) attackPoint.localPosition = new Vector3(-Mathf.Abs(originalAttackPointLocalX), attackPoint.localPosition.y, attackPoint.localPosition.z);
            if (firePoint != null) firePoint.localPosition = new Vector3(-Mathf.Abs(originalFirePointLocalX), firePoint.localPosition.y, firePoint.localPosition.z);
            if (wallCheck != null) wallCheck.localPosition = new Vector3(-Mathf.Abs(originalWallCheckLocalX), wallCheck.localPosition.y, wallCheck.localPosition.z);
            animator.SetBool(AnimDashLeft, true);
            animator.SetBool(AnimDashRight, false);
        }
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f); 
        StartCoroutine(EndDash());
        PlaySound(dashSound);
    }

    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;

        rb.gravityScale = isInPortalTrigger ? 0f : originalGravityScale;

        animator.SetBool(AnimDashRight, false);
        animator.SetBool(AnimDashLeft, false);
        
        UpdateChildLocalPositions(); 
    }
    
    public void SetInPortalTrigger(bool inTrigger)
    {
        isInPortalTrigger = inTrigger;
        if (!inTrigger)
        {
            isMovingUp = false;
            rb.gravityScale = originalGravityScale;
        } else {
            rb.gravityScale = 0f;
        }
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(wallCheck.position, 0.2f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
        }
    }
}