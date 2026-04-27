using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Settings")]
    public float dropLifetime = 8f;
    public GameObject currencyDropPrefab;

    [Header("Runtime")]
    public float currentCurrency;

    public event Action<float> OnCurrencyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Add(float amount)
    {
        currentCurrency += amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    public bool Spend(float amount)
    {
        if (currentCurrency < amount) return false;
        currentCurrency -= amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
        return true;
    }

    public bool CanAfford(float amount)
    {
        return currentCurrency >= amount;
    }

    public void SpawnDrop(Vector3 position, float amount)
    {
        if (currencyDropPrefab == null)
        {
            Debug.LogWarning("CurrencyManager: No drop prefab assigned.");
            return;
        }

        GameObject drop = Instantiate(currencyDropPrefab, position, Quaternion.identity);
        CurrencyDrop dropComponent = drop.GetComponent<CurrencyDrop>();

        if (dropComponent != null)
        {
            dropComponent.Init(amount, dropLifetime);
        }
    }
}