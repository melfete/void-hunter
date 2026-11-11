using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortalManager : MonoBehaviour
{
    public GameObject portalObject;
    public Transform portalSpawnLocation;
    public float portalActivationDelay = 1f;
    public float portalEffectStartDelay = 1.5f;
    public string targetSceneName;

    private Animator portalAnimator;
    private PlayerController playerController;
    private bool portalIsActive = false;
    private bool effectTriggered = false;

    void Start()
    {
        if (portalObject == null)
        {
            Debug.LogError("PortalManager: Portal Object not assigned!");
            enabled = false;
            return;
        }

        portalObject.SetActive(false); 

        portalAnimator = portalObject.GetComponent<Animator>();
        if (portalAnimator == null)
        {
            Debug.LogWarning("PortalManager: No Animator found on portalObject. Visual activation might not work.");
        }

        GameObject playerGO = GameObject.FindWithTag("Player");
        if(playerGO != null)
        {
            playerController = playerGO.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("PortalManager: Player GameObject not found with Tag 'Player' at Start. Player reference might be missing for end effect.");
        }
    }

    void OnEnable()
    {
        EnemyHealth.OnSpecificEnemyDied += ActivatePortal;
    }

    void OnDisable()
    {
        EnemyHealth.OnSpecificEnemyDied -= ActivatePortal;
    }

    void ActivatePortal(GameObject diedEnemy)
    {
        Debug.Log("PortalManager: Specific enemy " + diedEnemy.name + " died! Activating portal.");
        Invoke("DoActivatePortal", portalActivationDelay);
    }

    void DoActivatePortal()
    {
        if (portalSpawnLocation != null)
        {
            portalObject.transform.position = portalSpawnLocation.position;
        }
        
        portalObject.SetActive(true);
        portalIsActive = true;

        if (portalAnimator != null)
        {
            
        }
        Debug.Log("PortalManager: Portal activated!");
        
        StartCoroutine(StartEndGameEffectAfterDelay());
    }

    IEnumerator StartEndGameEffectAfterDelay()
    {
        Debug.Log("PortalManager: Warte " + portalEffectStartDelay + " Sekunden für Portal-Animation, bevor Sog-Effekt startet.");
        yield return new WaitForSeconds(portalEffectStartDelay);

        if (!effectTriggered && GameEndManager.instance != null)
        {
            effectTriggered = true; 
            Debug.Log("PortalManager: Verzögerung abgelaufen, starte GameEndManager Effekt.");
            
            GameEndManager.instance.targetSceneName = targetSceneName;
            GameEndManager.instance.portalTransform = portalObject.transform; 
            GameEndManager.instance.StartEndGameEffect(playerController?.gameObject); 
            
            if (playerController != null) playerController.enabled = false;
            
            enabled = false;
        }
        else if (GameEndManager.instance == null)
        {
            Debug.LogWarning("PortalManager: GameEndManager.instance nicht gefunden nach Verzögerung! Szene wird nicht automatisch geladen.");
        
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
         
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            
        }
    }
    
}