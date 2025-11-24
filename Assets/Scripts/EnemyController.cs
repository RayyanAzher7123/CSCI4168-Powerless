using UnityEngine;
using UnityEngine.AI;
using System.Collections;


public class EnemyController : MonoBehaviour
{
    [SerializeField] private GameObject enemy_go;
    [SerializeField] private GameObject player_go;
    [SerializeField] private GameObject flashlight_go;
    [SerializeField] private GameObject idle_go;
    [SerializeField] private GameObject idlePoints_go;
    [SerializeField] private GameObject active_go;

    [SerializeField] private idle_enemyState idle_es;
    [SerializeField] private attacking_enemyState attacking_es;
    [SerializeField] private frozen_enemyState frozen_es;
    [SerializeField] private returning_enemyState returning_es;
    [SerializeField] private kill_enemyState  kill_es;
    [SerializeField] private GenericState state;
    private GenericState nextState;
    
    [SerializeField] private float moveSpeed;
    [SerializeField] private float agroDistance;
    [SerializeField] private float attackDistance;
    [SerializeField] private float idleDistance;
    [SerializeField] private float flashlightFreezeAngle;
    [SerializeField] private float flashlightFreezeDistance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip freezeClip;

    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float footstepInterval = 0.5f;
    private float footstepTimer = 0f;


    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip ambientClip;

    [SerializeField] private AudioClip growlClip;
    [SerializeField] private float growlMinDelay = 5f;
    [SerializeField] private float growlMaxDelay = 15f;
    private float growlTimer;

    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip finalAttackClip;



    private Vector3[] idlePoints_v3;
    private Vector3 currentIdlePoint_v3;
    private int numIdlePoints;
    private bool canSeePlayer;
    private bool isInLight;
    private Light flashlight;
    private NavMeshAgent enemy_nma;
    private Collider enemy_c;
    
    void Start()
    {
        idle_es.Setup(enemy_go);
        attacking_es.Setup(enemy_go);
        frozen_es.Setup(enemy_go);
        returning_es.Setup(enemy_go);
        kill_es.Setup(enemy_go);
        state =  idle_es;
        nextState = idle_es;

        setupIdlePoints();
        
        flashlight = flashlight_go.GetComponent<Light>();
        flashlightFreezeAngle = flashlight.spotAngle / 2f + 3f;

        enemy_nma = enemy_go.GetComponent<NavMeshAgent>();
        enemy_c = enemy_go.GetComponent<CapsuleCollider>();
        
        InvokeRepeating("checkPlayer", 0, 1f);
    }

    void Update()
    {
        state.Do();
        HandleFootsteps();
        HandleAmbient();
        HandleGrowls();
    }

    void FixedUpdate()
    {
        state.FixedDo();
        selectState();
    }

    private void selectState()
    {
        switch (state)
        {
            case idle_enemyState:
                if (inLight())
                {
                    nextState = frozen_es;
                } 
                else if (canSeePlayer)
                {
                    nextState = attacking_es;
                } 
                else if (!inIdlePosition())
                { 
                    nextState = returning_es; 
                }
                break;
            case attacking_enemyState:
                if (canKill())
                {
                    nextState = kill_es;
                }
                if (inLight())
                {
                    nextState = frozen_es;
                } 
                else if (!canSeePlayer)
                {
                    nextState = returning_es;
                }
                break;
            case frozen_enemyState:
                if (!inLight())
                {
                    if (canSeePlayer)
                    {
                        nextState = attacking_es;
                    } 
                    else if (!inIdlePosition())
                    {
                        nextState = returning_es;
                    }
                    else
                    {
                        nextState =  idle_es;
                    }
                }
                break;
            case returning_enemyState:
                if (inLight())
                {
                    nextState = frozen_es;
                } 
                else if (canSeePlayer)
                {
                    nextState = attacking_es;
                } 
                else if (inIdlePosition())
                { 
                    nextState = idle_es; 
                }
                break;
        }

        if (state != nextState)
        {
            state.Exit();
            Debug.Log(nextState);
            state = nextState;
            state.Enter();
        }
    }

    // Return true if enemy is close enough to player
    private bool canKill()
    {
        return Vector3.Distance(player_go.transform.position, enemy_go.transform.position) < attackDistance;
    }
    
    private bool inIdlePosition()
    {
        return Vector3.Distance(currentIdlePoint_v3, enemy_go.transform.position) < idleDistance;
    }

    private void checkPlayer()
    {
        if (isInAgroRange())
        {
            canSeePlayer = true;
        }
        else
        {
            canSeePlayer = false;
        }
    }

    private bool inLight()
    {
        return inFlashlight() && inAmbientLight();
    }

    private bool inAmbientLight()
    {
        //TODO: Ambient light logic
        return true;
    }
    
    private bool inFlashlight()
    {
        return flashlight.isActiveAndEnabled &&
               isInFlashlightRange() &&
               isInFlashlightCone() &&
               canSeeFlashlight();
    }

    private bool canSeeFlashlight()
    {
        Physics.Raycast(flashlight_go.transform.position, enemy_go.transform.position - flashlight_go.transform.position, out RaycastHit hit);
        return hit.collider == enemy_c;
    }

    private bool isInFlashlightCone()
    {
        return Vector3.Angle(flashlight_go.transform.forward, enemy_go.transform.position - flashlight_go.transform.position) < flashlightFreezeAngle;
    }

    private bool isInFlashlightRange()
    {
        return Vector3.Distance(player_go.transform.position, enemy_go.transform.position) < flashlightFreezeDistance;
    }
    
    private bool isInAgroRange()
    {
        return Vector3.Distance(player_go.transform.position, enemy_go.transform.position) < agroDistance;
    }

    private void setupIdlePoints()
    {
        Transform[] ips = idlePoints_go.GetComponentsInChildren<Transform>();
        numIdlePoints = ips.Length;
        idlePoints_v3 = new Vector3[numIdlePoints];

        int i = 0;
        foreach (Transform t in ips)
        {
            idlePoints_v3[i] = t.position;
            i++;
        }
        
        currentIdlePoint_v3 =  idlePoints_v3[0];
    }
    
    public void activateIdleModel()
    {
        idle_go.SetActive(true);
        active_go.SetActive(false);
    }
    
    public void activateActiveModel()
    {
        idle_go.SetActive(false);
        active_go.SetActive(true);
    }

    public void huntPlayer()
    {
        enemy_nma.SetDestination(player_go.transform.position);
    }

    public void moveToIdlePoint()
    {
        enemy_nma.SetDestination(currentIdlePoint_v3);
    }

    public void chooseIdlePoint()
    {
        currentIdlePoint_v3 = idlePoints_v3[Random.Range(0, idlePoints_v3.Length)];
    }

    public void stopEnemyMovement()
    {
        enemy_nma.SetDestination(enemy_go.transform.position);
    }

    public void killPlayer()
    {
        player_go.GetComponentInChildren<PlayerController>().killPlayer();
    }

    public void PlayFreezeSound()
    {
        audioSource.PlayOneShot(freezeClip);
    }

    private void HandleFootsteps()
    {
    if (enemy_nma.velocity.magnitude > 0.1f)
    {
        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            audioSource.PlayOneShot(footstepClip);
            footstepTimer = footstepInterval;
        }
    }
    }
    public void PlayAttackSound()
    {
        audioSource.PlayOneShot(attackClip);
    }

    private void HandleAmbient()
    {
    if (!ambientSource.isPlaying)
    {
        ambientSource.clip = ambientClip;
        ambientSource.loop = true;
        ambientSource.Play();
    }
    }

    private void HandleGrowls()
    {
    growlTimer -= Time.deltaTime;
    if (growlTimer <= 0f)
    {
        audioSource.PlayOneShot(growlClip);
        growlTimer = Random.Range(growlMinDelay, growlMaxDelay);
    }
    }

    public void PlayFinalAttackAndDie()
    {
        StartCoroutine(FinalAttackSequence());
    }

    private IEnumerator FinalAttackSequence()
    {
    
        audioSource.PlayOneShot(finalAttackClip);
        yield return new WaitForSeconds(0.7f); 
        UnityEngine.SceneManagement.SceneManager.LoadScene("DeathScene");

    }


}
