using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controls the skateboarder player (04 Image 2) in Play2.unity.
/// Allows jumping over obstacles (04 Image 3 and 04 Image 4) and horizontal movement.
/// Compatible with both Unity Input System and Legacy Input Manager.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.5f;
    public float jumpForce = 12.5f;
    public float minX = -7.5f;
    public float maxX = 7.5f;

    [Header("Ground Detection")]
    public float groundYThreshold = -2.05f;
    public bool isGrounded = true;

    private Rigidbody2D _rb;
    private int _groundContacts = 0;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.gravityScale = 2.8f;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        float horizontal = 0f;
        bool jumpPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame ||
                          Keyboard.current.wKey.wasPressedThisFrame ||
                          Keyboard.current.upArrowKey.wasPressedThisFrame;
        }
#else
        horizontal = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#endif

        Vector2 velocity = _rb.linearVelocity;
        velocity.x = horizontal * moveSpeed;
        _rb.linearVelocity = velocity;

        // Clamp horizontal position inside camera view
        Vector3 pos = transform.position;
        if (pos.x < minX || pos.x > maxX)
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;
        }

        // Ground check fallback by Y position or contact count
        bool canJump = isGrounded || _groundContacts > 0 || (transform.position.y <= groundYThreshold && Mathf.Abs(_rb.linearVelocity.y) < 0.05f);

        // Jump input (Space, W, or UpArrow)
        if (canJump && jumpPressed)
        {
            Jump();
        }
    }

    private void Jump()
    {
        isGrounded = false;
        _groundContacts = 0;
        Vector2 velocity = _rb.linearVelocity;
        velocity.y = jumpForce;
        _rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Ground") || collision.gameObject.CompareTag("Ground"))
        {
            _groundContacts = Mathf.Max(0, _groundContacts - 1);
            if (_groundContacts == 0)
            {
                isGrounded = false;
            }
        }
    }

    private void EvaluateCollision(Collision2D collision)
    {
        // Check if colliding with the Ground
        if (collision.gameObject.name.Contains("Ground") || collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    _groundContacts = Mathf.Max(1, _groundContacts);
                    return;
                }
            }
        }

        // Check if colliding with an Obstacle (04 Image 3 or 04 Image 4)
        ObstacleMover obstacle = collision.gameObject.GetComponent<ObstacleMover>();
        if (obstacle != null || collision.gameObject.name.Contains("Obstacle"))
        {
            // Allow landing on top of the flat brick ledge (04 Image 4) to grind/jump off!
            bool landedOnTop = false;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.65f && obstacle != null && obstacle.allowGrindOnTop)
                {
                    landedOnTop = true;
                    break;
                }
            }

            if (landedOnTop)
            {
                isGrounded = true;
                _groundContacts = Mathf.Max(1, _groundContacts);
            }
            else
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameOver();
                }
            }
        }
    }
}
