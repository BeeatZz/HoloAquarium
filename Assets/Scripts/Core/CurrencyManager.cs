using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Settings")]
    public float dropLifetime = 8f;
    public GameObject currencyDropPrefab;
    public RectTransform counterIcon;

    [Header("Runtime")]
    public float currentCurrency;
    public float totalCollected;

    public event Action<float> OnCurrencyChanged;
    public event Action<float> OnCurrencyCollected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public Vector3 GetCounterScreenPos()
    {
        return counterIcon.position;
    }
    public void Add(float amount)
    {
        currentCurrency += amount;
        totalCollected += amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
        OnCurrencyCollected?.Invoke(amount);
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
            return;
        }

        GameObject drop = Instantiate(currencyDropPrefab, position, Quaternion.identity);
        CurrencyDrop dropComponent = drop.GetComponent<CurrencyDrop>();

        if (dropComponent != null)
            dropComponent.Init(amount, dropLifetime);
    }
}