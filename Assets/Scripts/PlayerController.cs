using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject camera_go;
    [SerializeField] private GameObject flashlight;
    [SerializeField] private TMPro.TextMeshProUGUI interactText;

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
    [SerializeField] private float standingCameraY = 1.5f;
    [SerializeField] private float crouchingCameraY = 0.65f;
    [SerializeField] private float cameraSmooth = 8f;
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
    private CapsuleCollider capsule;
    private float originalHeight;
    private float crouchHeight;
    private Vector3 originalCenter;
    private Vector3 crouchCenter;

    private bool isDead = false;
    private Quaternion flashlight_q;
    private bool wasFlashlightOn = false;
    private bool holdToggle;

    // Footstep variables
    private float walkStepInterval = 0.6f;
    private float sprintStepInterval = 0.35f;
    private float footstepTimer = 0f;




    void Start()
    {
        playerCharacter = GetComponent<PlayerCharacter>();

        flashlight_l = flashlight.GetComponentInChildren<Light>(); //verify
        if (flashlight_l == null)
            Debug.LogError("Flashlight not assigned in PlayerController!");
        flashlight_l.enabled = false;
        wasFlashlightOn = false;
        holdToggle = false;

        player_rb = player.GetComponent<Rigidbody>();

        camera_c = player.GetComponentInChildren<Camera>();

        capsule = player.GetComponent<CapsuleCollider>();

        originalHeight = capsule.height;
        crouchHeight = originalHeight * 0.5f;
        originalCenter = capsule.center;
        crouchCenter = new Vector3(originalCenter.x, originalCenter.y * 0.5f, originalCenter.z);

        isDead = false;

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        getInputs();
        UpdateInteractUI(); // Guides player on what they can interact with
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

            applyMovement(crouchSpeed);

        }
        else if (getSprint())
        {
            ///player.transform.localScale = new Vector3(1f, 1.5f, 1f);
            applyMovement(sprintSpeed);
        }
        else
        {
            //player.transform.localScale = new Vector3(1f, 1.5f, 1f);
            applyMovement(walkSpeed);
        }


        applyRotation();
        applyCrouchHeight();
        applyColliderCrouchHeight();
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
        rotate_v.y = Mathf.Clamp(rotate_v.y, -20f, 20f);

        player.transform.localRotation = Quaternion.Slerp(
            player.transform.localRotation,
            Quaternion.Euler(0, rotate_v.x, 0),
            0.2f);

        camera_go.transform.localRotation = Quaternion.Slerp(
            camera_c.transform.localRotation,
            Quaternion.Euler(rotate_v.y, 0, 0),
            0.2f);
    }

    private void applyCrouchHeight()
    {
        Vector3 camPos = camera_go.transform.localPosition;
        float targetY = getCrouch() ? crouchingCameraY : standingCameraY;
        camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * cameraSmooth);
        camera_go.transform.localPosition = camPos;
    }

    private void applyColliderCrouchHeight()
    {
        // Adjust collider to pass under obstacles
        if (getCrouch())
        {
            capsule.height = Mathf.Lerp(capsule.height, crouchHeight, Time.deltaTime * 12f);
            capsule.center = Vector3.Lerp(capsule.center, crouchCenter, Time.deltaTime * 12f);
            return;
        }

        if (!getCrouch() && canStandUp())
        {
            capsule.height = Mathf.Lerp(capsule.height, originalHeight, Time.deltaTime * 12f);
            capsule.center = Vector3.Lerp(capsule.center, originalCenter, Time.deltaTime * 12f);
        }
    }

    private bool canStandUp()
    {
        return !Physics.Raycast(player.transform.position, Vector3.up, originalHeight * 0.6f);
    }



    private void applyInteract()
    {
        GameObject target = FindBestInteractable();

        if (target == null)
            return;

        Debug.Log("Interacting with: " + target.name);

        string tag = target.tag;

        if (tag == "Key")
        {
            audioSource.PlayOneShot(keyPickupSound);
            playerCharacter.addKey();
            Destroy(target);
        }
        else if (tag == "Battery")
        {
            audioSource.PlayOneShot(batteryPickupSound);
            playerCharacter.addBattery();
            Destroy(target);
        }
        else if (tag == "Gear")
        {
            audioSource.PlayOneShot(gearPickupSound);
            playerCharacter.addGear();
            Destroy(target);
        }
        else if (tag == "Door")
        {
            if (playerCharacter.getKeys() > 0)
            {
                audioSource.PlayOneShot(doorOpenSound);
                Destroy(target);
                playerCharacter.removeKey();
            }
        }
        else if (tag == "Generator")
        {
            if (playerCharacter.getGear() > 0)
                loadNextScene();
        }
    }

    private bool IsInteractable(GameObject go)
    {
        string tag = go.tag;
        return tag == "Key" || tag == "Battery" || tag == "Gear" || tag == "Door" || tag == "Generator";
    }

    private GameObject FindBestInteractable()
    {
        Vector3 origin = camera_go.transform.position;
        float radius = reachDistance;

        Collider[] hits = Physics.OverlapSphere(origin, radius);

        GameObject best = null;
        float bestScore = Mathf.Infinity;

        foreach (Collider col in hits)
        {
            if (!IsInteractable(col.gameObject))
                continue;

            // Use center of collider (works even with MeshColliders)
            Vector3 center = col.bounds.center;

            // Convert world to screen space
            Vector3 screenPos = camera_c.WorldToScreenPoint(center);

            // Ignore objects behind camera
            if (screenPos.z < 0f)
                continue;

            // Distance from screen center (smaller = more centered)
            float dx = screenPos.x - (Screen.width * 0.5f);
            float dy = screenPos.y - (Screen.height * 0.5f);
            float score = dx * dx + dy * dy;

            if (score < bestScore)
            {
                bestScore = score;
                best = col.gameObject;
            }
        }

        return best;
    }

    private void UpdateInteractText(GameObject target)
    {
        if (target == null)
        {
            interactText.text = "";
            return;
        }

        string tag = target.tag;

        switch (tag)
        {
            case "Key":
                interactText.text = "Press E to pick up Key";
                break;

            case "Battery":
                interactText.text = "Press E to pick up Battery";
                break;

            case "Gear":
                interactText.text = "Press E to pick up Gear";
                break;

            case "Door":
                interactText.text = "Press E to open Door";
                break;

            case "Generator":
                interactText.text = "Press E to start Generator";
                break;

            default:
                interactText.text = "";
                break;
        }
    }
    private void UpdateInteractUI()
    {
        GameObject target = FindBestInteractable();
        UpdateInteractText(target);
    }

    private void getInputs()
    {
        getMovement();
        getRotation();

        // flashlight ON/OFF
        HandleFlashlight();

        // then rotation logic
        // only handle rotation if flashlight is ON
        if (flashlight_l.enabled && getAttack())
            HandleFlashlightRotation();

        // then reload
        HandleFlashlightReload();

        if (getInteract())
            applyInteract();

    }

    private void HandleFlashlightRotation()
    {
        bool holding = getHold();

        // FIRST TIME PRESS — store current flashlight rotation
        if (holding && !holdToggle)
        {
            flashlight_q = flashlight.transform.rotation;
            holdToggle = true;
        }
        // CONTINUOUS HOLD — freeze rotation exactly
        else if (holding && holdToggle)
        {
            flashlight.transform.rotation = flashlight_q;
        }
        // RELEASE HOLD — snap back to camera instantly + reset
        else if (!holding && holdToggle)
        {
            Quaternion target = Quaternion.Euler(0, 280f, 0);
            flashlight.transform.localRotation = Quaternion.Slerp(
            flashlight.transform.localRotation,
            target,
            Time.deltaTime * 8f
    );
        }
    }

    private void HandleFlashlightReload()
    {
        Battery currentBattery = playerCharacter.getBattery();
        if (currentBattery == null) return;

        if (getReload())
        {
            audioSource.PlayOneShot(reloadSound, 0.8f);
            playerCharacter.reloadFlashlight();
        }
    }



    private void HandleFlashlight()
    {
        // If player has 
        // -battery equipped
        // -has battery life 
        // -is not dead
        // then turn on flashlight
        Battery currentBattery = playerCharacter.getBattery();
        bool attackInput = getAttack();   // true while holding flashlight button

        // TURN FLASHLIGHT ON
        if (attackInput && !wasFlashlightOn)
        {
            if (currentBattery != null && currentBattery.getBatteryLife() > 0 && !isDead)
            {
                audioSource.PlayOneShot(flashlightClickSound);
                flashlight_l.enabled = true;
            }
        }

        // TURN FLASHLIGHT OFF
        if (!attackInput && wasFlashlightOn)
        {
            audioSource.PlayOneShot(flashlightClickSound);
            flashlight_l.enabled = false;
        }

        // BATTERY DRAIN
        if (flashlight_l.enabled && currentBattery != null && !isDead)
        {
            currentBattery.decrementTime();
        }

        // Auto shutdown when battery dies
        if (flashlight_l.enabled && currentBattery != null && currentBattery.getBatteryLife() <= 0)
        {
            flashlight_l.enabled = false;
            audioSource.PlayOneShot(flashlightClickSound);
        }

        wasFlashlightOn = attackInput;
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
        // Freeze motion IMMEDIATELY
        player_rb.isKinematic = true;
        player_rb.linearVelocity = Vector3.zero;
        player_rb.angularVelocity = Vector3.zero;
    }

    public bool getIsDead()
    {
        return isDead;
    }

    public GameObject getPlayerGameObject()
    {
        return player;
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