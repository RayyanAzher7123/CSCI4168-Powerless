using UnityEngine;

public class FlashlightBob : MonoBehaviour
{
    public float amplitude = 0.05f;
    public float speed = 8f;
    public float smooth = 8f;

    private PlayerController playerController;
    private Rigidbody playerRB;
    private Vector3 initialLocalPos;

    void Start()
    {
        // Find PlayerController (it is on Controller)
        playerController = FindObjectOfType<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("FlashlightBob: No PlayerController found!");
            enabled = false;
            return;
        }

        // Grab the Player rigidbody (assigned in inspector on PlayerController)
        playerRB = playerController.getPlayerGameObject().GetComponent<Rigidbody>();

        if (playerRB == null)
        {
            Debug.LogError("FlashlightBob: Player Rigidbody is missing!");
            enabled = false;
            return;
        }

        initialLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (playerController.getIsDead())
        {
            ResetPosition();
            return;
        }

        ApplyBob();
    }

    private void ApplyBob()
    {
        float moveAmount = playerRB.linearVelocity.magnitude;

        // No movement → reset bob position
        if (moveAmount < 0.1f)
        {
            ResetPosition();
            return;
        }

        float bob = Mathf.Sin(Time.time * speed) * amplitude;
        Vector3 targetPos = initialLocalPos + new Vector3(0, bob, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smooth);
    }

    private void ResetPosition()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            initialLocalPos,
            Time.deltaTime * smooth
        );
    }
}
