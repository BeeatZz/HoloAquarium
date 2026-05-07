using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Data")]
    public LevelGoal levelGoal;

    [Header("References")]
    public WaveSpawner waveSpawner;

    [Header("Play Area")]
    public Vector2 playAreaMin = new Vector2(-4f, -3f);
    public Vector2 playAreaMax = new Vector2(4f, 3f);

    [Header("Runtime State")]
    public int currentPurchaseCount;
    public int gremDeathCount;
    public float elapsedTime;
    public float totalCurrencyCollected;
    public bool levelActive;
    public bool levelComplete;
    public bool is3DPerspectiveMode = false;
    [Header("Survival Settings")]
    public TextMeshProUGUI timerText;
    private bool survivalActive = false;
    private float survivalTimeRemaining;

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
        totalCurrencyCollected = 0f;
        currentTierIndex = 0;
        thresholdReached = false;
        levelActive = true;
        levelComplete = false;

        CurrencyManager.Instance.OnCurrencyChanged += CheckPurchaseThreshold;
        CurrencyManager.Instance.OnCurrencyCollected += OnCurrencyCollectedHandler;

        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!levelActive || levelComplete) return;

        elapsedTime += Time.deltaTime;

        if (survivalActive)
        {
            survivalTimeRemaining -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = $"SURVIVE: {Mathf.CeilToInt(survivalTimeRemaining)}s";
            }

            if (survivalTimeRemaining <= 0)
            {
                survivalActive = false;
                if (timerText != null) timerText.gameObject.SetActive(false);
                CompleteLevel();
            }
        }
    }

    public void StartSurvivalTimer(float duration)
    {
        if (survivalActive) return;

        survivalTimeRemaining = duration;
        survivalActive = true;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.transform.localScale = Vector3.zero;
            timerText.transform.localScale = Vector3.one; 
        }
    }

    public void CheckForDefeat()
    {
        if (!levelActive || levelComplete) return;

        Gremurin[] remainingGrems = UnityEngine.Object.FindObjectsByType<Gremurin>(FindObjectsSortMode.None);

        if (remainingGrems.Length == 0)
        {
            Debug.Log("Game Over");
            levelActive = false;
            
        }
    }

    public Vector3 ClampToPlayArea(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, playAreaMin.x, playAreaMax.x);
        position.y = Mathf.Clamp(position.y, playAreaMin.y, playAreaMax.y);
        return position;
    }

    private void CheckPurchaseThreshold(float currentCurrency)
    {
        if (levelGoal == null || currentTierIndex >= levelGoal.purchaseTiers.Count) return;
        if (!WaveRequirementMet()) return;

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

    private bool WaveRequirementMet()
    {
        if (waveSpawner == null) return true;
        if (levelGoal.requireAllWavesComplete) return waveSpawner.allWavesComplete;
        if (levelGoal.requiredWaveIndex >= 0) return waveSpawner.currentWaveIndex > levelGoal.requiredWaveIndex;
        return true;
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
            CompleteLevel();

        return true;
    }

    public void RegisterGremDeath()
    {
        if (!levelActive || levelComplete) return;
        gremDeathCount++;

        CheckForDefeat();
    }

    private void OnCurrencyCollectedHandler(float amount)
    {
        totalCurrencyCollected += amount;
    }

    private void CompleteLevel()
    {
        if (levelComplete) return;

        levelComplete = true;
        levelActive = false;
        survivalActive = false;

        CurrencyManager.Instance.OnCurrencyChanged -= CheckPurchaseThreshold;
        CurrencyManager.Instance.OnCurrencyCollected -= OnCurrencyCollectedHandler;

        int stars = EvaluateStars();
        Debug.Log($"Level Complete — Stars: {stars}, Deaths: {gremDeathCount}, Time: {elapsedTime:F1}s");

        OnLevelComplete?.Invoke();
    }

    private int EvaluateStars()
    {
        StarObjective[] objectives = GetComponentsInChildren<StarObjective>();
        if (objectives.Length == 0) return 1;

        int stars = 0;
        foreach (StarObjective obj in objectives)
        {
            if (obj.Evaluate()) stars++;
        }

        return Mathf.Max(1, stars);
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= CheckPurchaseThreshold;
            CurrencyManager.Instance.OnCurrencyCollected -= OnCurrencyCollectedHandler;
        }
    }
}