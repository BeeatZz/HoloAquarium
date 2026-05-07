using UnityEngine;
using DG.Tweening;

public class ProtectionSystem : MonoBehaviour
{
    public static ProtectionSystem Instance { get; private set; }

    [Header("Settings")]
    public GameObject barrierPrefab;
    public float barrierCost = 25f;
    public float barrierDuration = 10f;

    [Header("State")]
    public bool protectionModeActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            UnityEngine.Object.Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ToggleProtectionMode(bool active)
    {
        protectionModeActive = active;
        if (active && FeedingSystem.Instance != null)
        {
            FeedingSystem.Instance.ToggleFeedingMode(false);
        }
    }

    public void TryPlaceBarrierAt(Vector2 position)
    {
        if (barrierPrefab == null) return;
        if (!CurrencyManager.Instance.CanAfford(barrierCost)) return;

        Vector3 worldPoint = LevelManager.Instance.ClampToPlayArea(
            new Vector3(position.x, position.y, 0)
        );

        UnityEngine.Collider2D hit = Physics2D.OverlapPoint(worldPoint);
        if (hit != null && hit.GetComponent<Enemy>() != null) return;

        CurrencyManager.Instance.Spend(barrierCost);
        SpawnBarrier(worldPoint);
    }

    private void SpawnBarrier(Vector3 position)
    {
        GameObject barrier = UnityEngine.Object.Instantiate(barrierPrefab, position, Quaternion.identity);
        SoundBarrier sb = barrier.GetComponent<SoundBarrier>();

        if (sb != null)
        {
            sb.duration = barrierDuration;
        }
    }
}