using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

// Ablauf: Erst wenn ALLE Boss-Gegner (Wizards) tot sind, erscheint das Portal.
// Der Spieler laeuft hinein, wird ins Portal gezogen (schrumpft, dreht sich, blendet aus)
// und erst danach startet der Endbildschirm ueber den GameEndManager.
public class PortalManager : MonoBehaviour
{
    public GameObject portalObject;
    public Transform portalSpawnLocation;
    public float portalActivationDelay = 1f;
    public string targetSceneName;

    [Header("Portal betreten")]
    public float portalTriggerRadius = 1f;
    public int portalSortingOrder = 20;       // vor Hintergrund und Figuren zeichnen
    public float pullDuration = 1.2f;         // Dauer des Hineinziehens
    public float pullSpinDegrees = 540f;      // Drehung waehrend des Hineinziehens

    private PlayerController playerController;
    private bool portalActivated;
    private bool playerEntering;

    void Start()
    {
        if (portalObject == null)
        {
            Debug.LogError("PortalManager: Portal Object not assigned!");
            enabled = false;
            return;
        }

        portalObject.SetActive(false);

        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerController = playerGO.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("PortalManager: Player GameObject not found with Tag 'Player' at Start.");
        }
    }

    void OnEnable()
    {
        EnemyHealth.OnSpecificEnemyDied += OnBossEnemyDied;
    }

    void OnDisable()
    {
        EnemyHealth.OnSpecificEnemyDied -= OnBossEnemyDied;
    }

    void OnBossEnemyDied(GameObject diedEnemy)
    {
        if (portalActivated) return;

        int remaining = CountLivingBosses(diedEnemy);
        if (remaining > 0)
        {
            Debug.Log("PortalManager: Boss besiegt, noch " + remaining + " uebrig.");
            return;
        }

        portalActivated = true;
        StartCoroutine(ActivatePortalAfterDelay());
    }

    private int CountLivingBosses(GameObject justDied)
    {
        int count = 0;
        foreach (EnemyHealth enemy in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
        {
            if (enemy.isBossEnemy && enemy.gameObject != justDied && enemy.IsAlive)
            {
                count++;
            }
        }
        return count;
    }

    IEnumerator ActivatePortalAfterDelay()
    {
        yield return new WaitForSeconds(portalActivationDelay);

        Vector3 pos = portalSpawnLocation != null ? portalSpawnLocation.position : portalObject.transform.position;
        pos.z = 0f; // Portal lag bei z = 20 und damit eventuell hinter dem Hintergrund
        portalObject.transform.position = pos;
        portalObject.SetActive(true);

        // Der Effekt-Renderer war in der Szene deaktiviert - sichtbar machen und nach vorne sortieren.
        Renderer portalRenderer = portalObject.GetComponent<Renderer>();
        if (portalRenderer != null)
        {
            portalRenderer.enabled = true;
            portalRenderer.sortingOrder = portalSortingOrder;
        }
        VisualEffect vfx = portalObject.GetComponent<VisualEffect>();
        if (vfx != null)
        {
            vfx.Reinit();
            vfx.Play();
        }

        // Eingang: Trigger-Bereich, den der Spieler betreten kann.
        CircleCollider2D trigger = portalObject.GetComponent<CircleCollider2D>();
        if (trigger == null) trigger = portalObject.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = portalTriggerRadius;

        PortalEntrance entrance = portalObject.GetComponent<PortalEntrance>();
        if (entrance == null) entrance = portalObject.AddComponent<PortalEntrance>();
        entrance.manager = this;
    }

    public void OnPlayerEnteredPortal(GameObject player)
    {
        if (playerEntering) return;
        playerEntering = true;
        StartCoroutine(PullPlayerIntoPortal(player));
    }

    IEnumerator PullPlayerIntoPortal(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        // Physik aus: keine Schwerkraft, keine Treffer mehr waehrend des Hineinziehens.
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // "Walk Up"-Animation (Ruecken zur Kamera) passt zum Hineingehen ins Portal.
        Animator anim = player.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsJumping", false);
            anim.SetBool("IsAttacking", false);
            anim.SetBool("IsAttacking2", false);
            anim.SetBool("IsMovingUp", true);
        }

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        Color startColor = sr != null ? sr.color : Color.white;
        Vector3 startPos = player.transform.position;
        Vector3 startScale = player.transform.localScale;
        Vector3 target = portalObject.transform.position;
        target.z = startPos.z;

        float t = 0f;
        while (t < pullDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / pullDuration);

            player.transform.position = Vector3.Lerp(startPos, target, k);
            player.transform.localScale = Vector3.Lerp(startScale, startScale * 0.1f, k);
            player.transform.rotation = Quaternion.Euler(0f, 0f, pullSpinDegrees * k);
            if (sr != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(startColor.a, 0f, k * k); // erst gegen Ende ausblenden
                sr.color = c;
            }
            yield return null;
        }

        if (GameEndManager.instance != null)
        {
            GameEndManager.instance.targetSceneName = targetSceneName;
            GameEndManager.instance.portalTransform = portalObject.transform;
            GameEndManager.instance.StartEndGameEffect(player);
        }
        else
        {
            Debug.LogWarning("PortalManager: GameEndManager.instance nicht gefunden! Endbildschirm wird nicht angezeigt.");
        }
    }
}
