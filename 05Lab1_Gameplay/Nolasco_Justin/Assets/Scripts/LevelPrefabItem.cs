using UnityEngine;

/// <summary>
/// Attached to modular 3D Prefabs in Level 1 and Level 2:
/// - CollectibleOrb: spins and awards score toward unlocking the Goal Portal
/// - MovingHazard: patrols back and forth; hitting it deducts 1 Life (triggers Loss Condition at 0 Lives)
/// - CheckpointArch: updates player's respawn checkpoint
/// - BoostRamp: boosts player forward/upward when driven onto
/// - GoalPortal: checks if all Orbs are collected and triggers Victory Condition
/// </summary>
public class LevelPrefabItem : MonoBehaviour
{
    public enum PrefabType
    {
        CollectibleOrb,
        MovingHazard,
        CheckpointArch,
        BoostRamp,
        GoalPortal
    }

    [Header("Prefab Type & Behavior")]
    public PrefabType itemType = PrefabType.CollectibleOrb;
    public float spinSpeed = 90f;
    public float moveAmplitude = 3.5f;
    public float moveFrequency = 1.6f;

    private Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        if (itemType == PrefabType.CollectibleOrb || itemType == PrefabType.GoalPortal)
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        }
        else if (itemType == PrefabType.MovingHazard)
        {
            transform.Rotate(Vector3.up, spinSpeed * 1.5f * Time.deltaTime, Space.Self);
            float offset = Mathf.Sin(Time.time * moveFrequency) * moveAmplitude;
            transform.position = _startPos + Vector3.right * offset;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleInteraction(other.GetComponentInParent<LevelPlayerController>());
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleInteraction(collision.gameObject.GetComponentInParent<LevelPlayerController>());
    }

    private void HandleInteraction(LevelPlayerController player)
    {
        if (player == null) return;

        switch (itemType)
        {
            case PrefabType.CollectibleOrb:
                if (LevelGameManager.Instance != null)
                {
                    LevelGameManager.Instance.CollectOrb();
                }
                gameObject.SetActive(false);
                break;

            case PrefabType.MovingHazard:
                if (LevelGameManager.Instance != null)
                {
                    LevelGameManager.Instance.OnPlayerHitHazardOrFell("Crashed into a Red Moving Hazard!");
                }
                if (LevelGameManager.Instance == null || !LevelGameManager.Instance.isGameOver)
                {
                    player.RespawnAtCheckpoint();
                }
                break;

            case PrefabType.CheckpointArch:
                player.SetCheckpoint(transform.position);
                break;

            case PrefabType.BoostRamp:
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce((transform.forward * 10f + Vector3.up * 6f), ForceMode.VelocityChange);
                }
                break;

            case PrefabType.GoalPortal:
                if (LevelGameManager.Instance != null)
                {
                    LevelGameManager.Instance.ReachGoalPortal();
                }
                break;
        }
    }
}
