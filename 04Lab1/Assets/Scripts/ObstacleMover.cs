using UnityEngine;

/// <summary>
/// Controls obstacle movement (04 Image 3 and 04 Image 4), scoring when jumped over,
/// and recycling obstacles so gameplay has a continuous flow toward the objective.
/// </summary>
public class ObstacleMover : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public bool scrollObstacle = true;
    public float moveSpeed = 4.2f;
    public float leftResetX = -10.5f;
    public float rightSpawnX = 10.5f;
    public float minDistanceToOtherObstacle = 5.5f;

    [Tooltip("If true, landing on top of this obstacle (e.g. Brick Ledge 04 Image 4) allows grinding/jumping off.")]
    public bool allowGrindOnTop = false;

    [Header("References")]
    public Transform playerTransform;
    public Transform otherObstacle;

    private bool _scoredThisPass = false;

    private void Start()
    {
        if (playerTransform == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
            {
                playerTransform = pc.transform;
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        if (scrollObstacle)
        {
            transform.Translate(Vector3.left * (moveSpeed * Time.deltaTime), Space.World);
        }

        // Award score when the player successfully jumps over / passes the obstacle
        if (!_scoredThisPass && playerTransform != null && transform.position.x < playerTransform.position.x - 0.6f)
        {
            _scoredThisPass = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(1);
            }
        }

        // Recycle obstacle when it moves off the left edge of the screen
        if (scrollObstacle && transform.position.x < leftResetX)
        {
            float spawnX = rightSpawnX + Random.Range(0f, 2.0f);
            if (otherObstacle != null && otherObstacle.position.x > spawnX - minDistanceToOtherObstacle)
            {
                spawnX = otherObstacle.position.x + minDistanceToOtherObstacle + Random.Range(0.5f, 1.5f);
            }

            Vector3 pos = transform.position;
            pos.x = spawnX;
            transform.position = pos;
            _scoredThisPass = false;
        }
    }
}
