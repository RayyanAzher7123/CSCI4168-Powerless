 using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject camera_go;
    [SerializeField] private GameObject flashlight;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference move_ia;
    [SerializeField] private InputActionReference look_ia;
    [SerializeField] private InputActionReference interact_ia;
    [SerializeField] private InputActionReference attack_ia;
    [SerializeField] private InputActionReference reload_ia;
    [SerializeField] private InputActionReference crouch_ia;
    [SerializeField] private InputActionReference sprint_ia;
    [SerializeField] private InputActionReference hold_ia;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float sensitivity;
    [SerializeField] private float reachDistance;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip keyPickupSound;
    [SerializeField] private AudioClip batteryPickupSound;
    [SerializeField] private AudioClip gearPickupSound;
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip flashlightClickSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip footstepSound;

    private AudioSource audioSource;

    // Internal variables
    private PlayerCharacter playerCharacter;
    private Light flashlight_l;
    private Vector3 move_v;
    private Vector3 rotate_v;
    private Vector3 targetRotate_v;
    private Rigidbody player_rb;
    private Camera camera_c;
    private bool isDead = false;
    private float timer;
    private Quaternion flashlight_q;
    private bool holdToggle;

    // Footstep variables
    private float walkStepInterval = 0.6f;
    private float sprintStepInterval = 0.35f;
    private float footstepTimer = 0f;




    void Start()
    {
        playerCharacter = GetComponent<PlayerCharacter>();
        flashlight_l = flashlight.GetComponentInChildren<Light>();
        player_rb = player.GetComponent<Rigidbody>();
        camera_c = player.GetComponentInChildren<Camera>();

        audioSource = gameObject.AddComponent<AudioSource>();
        timer = 0;
        holdToggle = false;
    }

    void Update()
    {
        getInputs();

        // Reset level when dead
        //if (isDead)
        //    restartScene();
    }

    private void FixedUpdate()
    {
        if (!isDead)
            applyInputs();
    }

    private void applyInputs()
    {
        // Movement speed selection
        if (getCrouch())
        {
            player.transform.localScale = new Vector3(1f, 1f, 1f);
            applyMovement(crouchSpeed);
        }
        else if (getSprint())
        {
            player.transform.localScale = new Vector3(1f, 1.5f, 1f);
            applyMovement(sprintSpeed);
        }
        else
        {
            player.transform.localScale = new Vector3(1f, 1.5f, 1f);
            applyMovement(walkSpeed);
        }

        applyRotation();
    }

    private void applyMovement(float speed)
    {
        player_rb.AddForce(move_v * speed);
        // FOOTSTEP SOUND
        if (move_v.magnitude > 0.1f && player_rb.linearVelocity.magnitude > 0.1f)
        {
            // Decrease timer
            footstepTimer -= Time.deltaTime;

            // Select interval based on movement (walk / sprint)
            float interval = getSprint() ? sprintStepInterval : walkStepInterval;

            // Play sound when timer reaches zero
            if (footstepTimer <= 0f)
            {
                audioSource.PlayOneShot(footstepSound, 0.4f);
                footstepTimer = interval;
            }
        }
        else
        {
            // Reset timer when player stops
            footstepTimer = 0f;
        }

    }

    private void applyRotation()
    {
        rotate_v.x += targetRotate_v.x * sensitivity;
        rotate_v.y -= targetRotate_v.y * sensitivity;
        rotate_v.y = Mathf.Clamp(rotate_v.y, -70f, 70f);

        player.transform.localRotation = Quaternion.Slerp(
            player.transform.localRotation,
            Quaternion.Euler(0, rotate_v.x, 0),
            0.2f);

        camera_go.transform.localRotation = Quaternion.Slerp(
            camera_c.transform.localRotation,
            Quaternion.Euler(rotate_v.y, 0, 0),
            0.2f);
    }

    private void applyInteract()
    {
        
        if (Physics.Raycast(camera_go.transform.position, camera_go.transform.forward, out RaycastHit hit, reachDistance))
        {
            Debug.Log(hit.transform.name);
            // KEY PICKUP
            if (hit.transform.CompareTag("Key"))
            {
                audioSource.PlayOneShot(keyPickupSound);
                playerCharacter.addKey();
                Destroy(hit.transform.gameObject);
            }

            // BATTERY PICKUP
            if (hit.transform.CompareTag("Battery"))
            {
                audioSource.PlayOneShot(batteryPickupSound);
                playerCharacter.addBattery();
                Destroy(hit.transform.gameObject);
            }

            // GEAR PICKUP
            if (hit.transform.CompareTag("Gear"))
            {
                audioSource.PlayOneShot(gearPickupSound);
                playerCharacter.addGear();
                Destroy(hit.transform.gameObject);
            }

            // DOOR OPEN
            if (hit.transform.CompareTag("Door"))
            {
                if (playerCharacter.getKeys() > 0)
                {
                    audioSource.PlayOneShot(doorOpenSound);
                    Destroy(hit.transform.gameObject);
                    playerCharacter.removeKey();
                }
            }

            // GENERATOR (Load next scene)
            if (hit.transform.CompareTag("Generator"))
            {
                if (playerCharacter.getGear() > 0)
                {
                    loadNextScene();
                }
            }
        }
    }

    private void getInputs()
    {
        getMovement();
        getRotation();

        if (getInteract())
            applyInteract();

        // FLASHLIGHT (Attack)
        if (getAttack())
        {
            Battery currentBattery = playerCharacter.getBattery();

            if (currentBattery != null && currentBattery.getIsLowBattery() && currentBattery.getBatteryLife() > 0)
            {
                timer += Time.deltaTime;
                if (timer >= 0.1f)
                {
                    flashlight_l.enabled = true;
                    currentBattery.decrementTime();
                    if (timer >= 0.2f)
                    {
                        timer = 0;
                    }
                }
                else
                {
                    flashlight_l.enabled = false;
                }
            }
            else if (currentBattery != null && !currentBattery.getIsLowBattery())
            {
                if (!flashlight_l.enabled)
                    audioSource.PlayOneShot(flashlightClickSound);

                flashlight_l.enabled = true;
                currentBattery.decrementTime();
            }
            else
            {
                flashlight_l.enabled = false;
            }
        }
        else
        {
            if (flashlight_l.enabled)
                audioSource.PlayOneShot(flashlightClickSound);

            flashlight_l.enabled = false;
        }
        
        if (getHold() && !holdToggle)
        {
            flashlight_q = flashlight.transform.rotation;
            
            holdToggle = true;
        } 
        else if (getHold() && holdToggle)
        {
            flashlight.transform.rotation = flashlight_q;
        }
        else
        {
            flashlight.transform.rotation = camera_go.transform.rotation * Quaternion.Euler(0f , 270f, 0f);
            
            holdToggle = false;
        }

        // RELOAD
        if (getReload())
        {
            audioSource.PlayOneShot(reloadSound, 0.8f);
            playerCharacter.reloadFlashlight();
        }
    }

    private void getMovement()
    {
        Quaternion targetRotation_q = Quaternion.Euler(transform.rotation.eulerAngles);

        move_v = move_ia.action.ReadValue<Vector2>();
        move_v = new Vector3(move_v.x, 0f, move_v.y);
        move_v = targetRotation_q * move_v;
        move_v = move_v.normalized * walkSpeed;
    }

    private void getRotation()
    {
        targetRotate_v = look_ia.action.ReadValue<Vector2>();
    }

    private bool getHold() => hold_ia.action.IsPressed();
    private bool getInteract() => interact_ia.action.IsPressed();
    private bool getAttack() => attack_ia.action.IsPressed();
    private bool getReload() => reload_ia.action.WasCompletedThisFrame();
    private bool getCrouch() => crouch_ia.action.IsPressed();
    private bool getSprint() => sprint_ia.action.IsPressed();

    public bool flashlightOn()
    {
        return getAttack() &&
               playerCharacter.getBattery() != null &&
               playerCharacter.getBattery().getBatteryLife() > 0;
    }

    public void killPlayer()
    {
        isDead = true;
    }

    private void loadNextScene()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;

        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            Debug.Log("No scene to load");
    }

    private void restartScene()
    {
        if (getInteract())
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}