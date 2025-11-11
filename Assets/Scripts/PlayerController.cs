using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject camera_go;
    [SerializeField] private GameObject flashlight;
    [SerializeField] private InputActionReference move_ia;
    [SerializeField] private InputActionReference look_ia;
    [SerializeField] private InputActionReference interact_ia;
    [SerializeField] private InputActionReference attack_ia;
    [SerializeField] private InputActionReference reload_ia;
    [SerializeField] private InputActionReference crouch_ia;
    [SerializeField] private InputActionReference sprint_ia;
    
    
    [SerializeField] private float walkSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float sensitivity;
    [SerializeField] private float reachDistance;

    private PlayerCharacter playerCharacter;
    private Light flashlight_l;
    private Vector3 move_v;
    private Vector3 rotate_v;
    private Vector3 targetRotate_v;
    private Rigidbody player_rb;
    private Camera camera_c;
    
    void Start()
    {
        playerCharacter = GetComponent<PlayerCharacter>();
        flashlight_l = flashlight.GetComponent<Light>();
        player_rb = player.GetComponent<Rigidbody>();
        camera_c = player.GetComponentInChildren<Camera>();
    }

    void Update()
    {
        getInputs();
    }

    private void FixedUpdate()
    {
        applyInputs();
    }

    private void applyInputs()
    {
        if (getCrouch())
        {
            player.transform.localScale = new Vector3(1f, 0.5f, 1f);
            applyMovement(crouchSpeed);
        } 
        else if (getSprint())
        {
            player.transform.localScale = new Vector3(1f, 1f, 1f);
            applyMovement(sprintSpeed);
        }
        else
        {
            player.transform.localScale = new Vector3(1f, 1f, 1f);
            applyMovement(walkSpeed);
        }
        
        applyRotation();
    }

    private void applyMovement(float speed)
    {
        player_rb.AddForce(move_v * speed);
    }

    private void applyRotation()
    {
        rotate_v.x += targetRotate_v.x * sensitivity;
        rotate_v.y -= targetRotate_v.y * sensitivity;
        rotate_v.y = Mathf.Clamp(rotate_v.y, -70f, 70f);
        
        player.transform.localRotation = Quaternion.Slerp(player.transform.localRotation, Quaternion.Euler(0, rotate_v.x, 0), 0.2f);
        camera_go.transform.localRotation = Quaternion.Slerp(camera_c.transform.localRotation, Quaternion.Euler(rotate_v.y, 0, 0), 0.2f);
    }

    private void applyInteract()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, reachDistance))
        {
            if (hit.transform.gameObject.CompareTag("Key"))
            {
                playerCharacter.addKey();
                Destroy(hit.transform.gameObject);
            }

            if (hit.transform.gameObject.CompareTag("Battery"))
            {
                playerCharacter.addBattery();
                Destroy(hit.transform.gameObject);
            }

            if (hit.transform.gameObject.CompareTag("Door"))
            {
                if (playerCharacter.getKeys() > 0)
                {
                    Destroy(hit.transform.gameObject);
                    playerCharacter.removeKey();
                }
            }
        }
    }

    private void getInputs()
    {
        getMovement();
        getRotation();
        
        if (getInteract())
        {
            applyInteract();
        }
        
        if (getAttack())
        {
            Battery currentBattery = playerCharacter.getBattery();
            if (currentBattery != null && currentBattery.getBatteryLife() > 0)
            {
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
            flashlight_l.enabled = false;
        }

        if (getReload())
        {
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

    private bool getInteract()
    {
        return interact_ia.action.IsPressed();
    }
    
    private bool getAttack()
    {
        return attack_ia.action.IsPressed();
    }
    
    private bool getReload()
    {
        return reload_ia.action.WasCompletedThisFrame();
    }
    
    private bool getCrouch()
    {
        return crouch_ia.action.IsPressed();
    }
    
    private bool getSprint()
    {
        return sprint_ia.action.IsPressed();
    }

    public bool flashlightOn()
    {
        if (getAttack() && playerCharacter.getBattery() != null && playerCharacter.getBattery().getBatteryLife() > 0)
        {
            return true;
        }
        return false;
    }
}
