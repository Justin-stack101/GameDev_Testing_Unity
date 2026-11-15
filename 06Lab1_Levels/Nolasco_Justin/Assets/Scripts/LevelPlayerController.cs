using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 3D Player / Kart / Explorer Controller for IT2012 05Lab1 (Gameplay) & IT2107 06Lab1 (Levels).
/// Defines player actions/rules (driving, steering, jumping, track boundaries)
/// and triggers loss/penalty when falling off the track.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class LevelPlayerController : MonoBehaviour
{
    [Header("Movement Rules & Settings")]
    public float moveSpeed = 9.5f;
    public float turnSpeed = 120f;
    public float jumpForce = 7.5f;
    public bool isGrounded = true;

    [Header("Respawn / Checkpoint & Out-of-Bounds Loss Rule")]
    public Vector3 respawnPoint = new Vector3(0f, 1.2f, -18f);
    public float fallThresholdY = -6f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        respawnPoint = transform.position;
    }

    private void Update()
    {
        if (LevelGameManager.Instance != null &&
            (LevelGameManager.Instance.levelCompleted || LevelGameManager.Instance.isGameOver))
        {
            return;
        }

        // Out-of-Bounds Rule: Falling off the track deducts a life (or triggers Game Over if 0 lives left)
        if (transform.position.y < fallThresholdY)
        {
            if (LevelGameManager.Instance != null)
            {
                LevelGameManager.Instance.OnPlayerHitHazardOrFell("Fell off the track!");
            }
            if (LevelGameManager.Instance == null || !LevelGameManager.Instance.isGameOver)
            {
                RespawnAtCheckpoint();
            }
            return;
        }

        float vertical = 0f;
        float horizontal = 0f;
        bool jumpPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        }
#else
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
#endif

        // Rotate with A/D or Left/Right and move forward/back with W/S or Up/Down
        transform.Rotate(Vector3.up, horizontal * turnSpeed * Time.deltaTime);

        Vector3 forwardVel = transform.forward * (vertical * moveSpeed);
        Vector3 currentVel = _rb.linearVelocity;
        _rb.linearVelocity = new Vector3(forwardVel.x, currentVel.y, forwardVel.z);

        // Jump Rule: Player can only jump while grounded on the track
        if (isGrounded && jumpPressed)
        {
            isGrounded = false;
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, jumpForce, _rb.linearVelocity.z);
        }
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 desiredPos = transform.position - transform.forward * 7.5f + Vector3.up * 4.2f;
            cam.transform.position = Vector3.Lerp(cam.transform.position, desiredPos, Time.deltaTime * 8f);
            cam.transform.LookAt(transform.position + Vector3.up * 1.0f);
        }
    }

    public void SetCheckpoint(Vector3 point)
    {
        respawnPoint = point + Vector3.up * 1.0f;
        if (LevelGameManager.Instance != null)
        {
            LevelGameManager.Instance.ShowStatusNotice("Checkpoint Reached!");
        }
    }

    public void RespawnAtCheckpoint()
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.position = respawnPoint;
        transform.rotation = Quaternion.identity;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
