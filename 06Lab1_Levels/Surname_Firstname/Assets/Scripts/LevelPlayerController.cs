using UnityEngine;

/// <summary>
/// 3D Player / Kart / Explorer Controller for IT2107 06Lab1 Levels.
/// Supports smooth 3D movement, jumping over ramps/obstacles, checkpoint respawning,
/// and camera follow toward the level objective.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class LevelPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 9f;
    public float turnSpeed = 120f;
    public float jumpForce = 7.5f;
    public bool isGrounded = true;

    [Header("Respawn / Checkpoint")]
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
        if (LevelGameManager.Instance != null && LevelGameManager.Instance.levelCompleted)
            return;

        if (transform.position.y < fallThresholdY)
        {
            RespawnAtCheckpoint();
            return;
        }

        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");

        // Rotate with A/D or Left/Right and move forward/back with W/S or Up/Down
        transform.Rotate(Vector3.up, horizontal * turnSpeed * Time.deltaTime);

        Vector3 forwardVel = transform.forward * (vertical * moveSpeed);
        Vector3 currentVel = _rb.linearVelocity;
        _rb.linearVelocity = new Vector3(forwardVel.x, currentVel.y, forwardVel.z);

        // Jump with Space
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
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
