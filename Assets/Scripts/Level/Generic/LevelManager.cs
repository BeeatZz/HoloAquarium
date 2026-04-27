using UnityEngine;
using System;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Data")]
    public LevelGoal levelGoal;

    [Header("Runtime State")]
    public int currentPurchaseCount;
    public int gremDeathCount;
    public float elapsedTime;
    public bool levelActive;
    public bool levelComplete;

    public event Action OnLevelComplete;
    public event Action<int> OnPurchaseMade;
    public event Action<float> OnCurrencyThresholdReached;

    private int currentTierIndex;
    private bool thresholdReached;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (levelGoal == null)
        {
            Debug.LogError("LevelManager: No LevelGoal assigned.");
            return;
        }

        currentPurchaseCount = 0;
        gremDeathCount = 0;
        elapsedTime = 0f;
        currentTierIndex = 0;
        thresholdReached = false;
        levelActive = true;
        levelComplete = false;

        CurrencyManager.Instance.OnCurrencyChanged += CheckPurchaseThreshold;
    }

    private void Update()
    {
        if (!levelActive || levelComplete) return;
        elapsedTime += Time.deltaTime;
    }

    private void CheckPurchaseThreshold(float currentCurrency)
    {
        if (levelGoal == null || currentTierIndex >= levelGoal.purchaseTiers.Count) return;

        float tierCost = levelGoal.purchaseTiers[currentTierIndex].cost;

        if (currentCurrency >= tierCost && !thresholdReached)
        {
            thresholdReached = true;
            OnCurrencyThresholdReached?.Invoke(tierCost);
        }
        else if (currentCurrency < tierCost)
        {
            thresholdReached = false;
        }
    }

    public bool TryPurchase()
    {
        if (!levelActive || levelComplete) return false;
        if (levelGoal == null) return false;
        if (currentTierIndex >= levelGoal.purchaseTiers.Count) return false;

        float tierCost = levelGoal.purchaseTiers[currentTierIndex].cost;

        if (!CurrencyManager.Instance.Spend(tierCost)) return false;

        currentPurchaseCount++;
        currentTierIndex++;
        thresholdReached = false;

        OnPurchaseMade?.Invoke(currentPurchaseCount);

        if (currentPurchaseCount >= levelGoal.totalPurchasesRequired)
        {
            CompleteLevel();
        }

        return true;
    }

    public void RegisterGremDeath()
    {
        if (!levelActive || levelComplete) return;
        gremDeathCount++;
    }

    private void CompleteLevel()
    {
        levelComplete = true;
        levelActive = false;

        CurrencyManager.Instance.OnCurrencyChanged -= CheckPurchaseThreshold;

        int stars = EvaluateStars();
        Debug.Log($"Level Complete — Stars: {stars}, Deaths: {gremDeathCount}, Time: {elapsedTime:F1}s");

        OnLevelComplete?.Invoke();
    }

    private int EvaluateStars()
    {
        int stars = 1;
        if (gremDeathCount == 0) stars++;
        if (elapsedTime <= levelGoal.timeLimitSeconds) stars++;
        return stars;
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= CheckPurchaseThreshold;
    }
}