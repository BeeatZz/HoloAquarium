using UnityEngine;

public class DropSpawner : MonoBehaviour
{
    [Header("Settings")]
    public float outputTimer;
    private Gremurin gremurin;

    private void Start()
    {
        gremurin = GetComponent<Gremurin>();

        if (gremurin == null)
        {
            Debug.LogError("DropSpawner: No Gremurin component found on this GameObject.");
            return;
        }

        ResetTimer();
    }

    private void Update()
    {
        if (gremurin == null || gremurin.isDead) return;
        if (gremurin.data == null) return;
        if (gremurin.currentHunger <= 0) return;
        if (LevelManager.Instance != null && !LevelManager.Instance.levelActive) return;

        outputTimer -= Time.deltaTime;
        if (outputTimer <= 0f)
        {
            SpawnDrop();
            ResetTimer();
        }
    }

    private void SpawnDrop()
    {
        if (CurrencyManager.Instance == null) return;

        CurrencyManager.Instance.SpawnDrop(
            transform.position,
            gremurin.data.currencyOutputAmount
        );
    }

    private void ResetTimer()
    {
        if (gremurin.data == null) return;
        outputTimer = gremurin.data.currencyOutputRate;
    }
}