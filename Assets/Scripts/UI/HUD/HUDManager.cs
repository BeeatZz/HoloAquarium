using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Currency")]
    public TextMeshProUGUI currencyText;

    [Header("Timer")]
    public TextMeshProUGUI timerText;

    [Header("Buy Button")]
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;

    [Header("Feeding Toggle")]
    public Button feedingToggleButton;
    public TextMeshProUGUI feedingToggleText;

    [Header("Shop")]
    public Button shopButton;

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
        CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrency;
        UpdateCurrency(CurrencyManager.Instance.currentCurrency);

        LevelManager.Instance.OnCurrencyThresholdReached += OnThresholdReached;
        LevelManager.Instance.OnPurchaseMade += OnPurchaseMade;

        SetBuyButtonState(false);

        buyButton.onClick.AddListener(OnBuyClicked);
        feedingToggleButton.onClick.AddListener(OnFeedingToggleClicked);
        shopButton.onClick.AddListener(OnShopClicked);

        UpdateFeedingToggleText();
    }

    private void Update()
    {
        if (!LevelManager.Instance.levelActive) return;
        UpdateTimer();
    }

    private void SetBuyButtonState(bool active)
    {
        CanvasGroup cg = buyButton.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = buyButton.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = active ? 1f : 0.3f;
        cg.interactable = active;
        cg.blocksRaycasts = active;
    }

    private void UpdateCurrency(float amount)
    {
        if (currencyText != null)
            currencyText.text = $"{Mathf.FloorToInt(amount)}";
    }

    private void UpdateTimer()
    {
        if (timerText == null) return;

        float elapsed = LevelManager.Instance.elapsedTime;
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void OnThresholdReached(float cost)
    {
        if (buyButton == null) return;
        SetBuyButtonState(true);

        if (buyButtonText != null)
            buyButtonText.text = $"Buy ({Mathf.FloorToInt(cost)})";
    }

    private void OnBuyClicked()
    {
        bool success = LevelManager.Instance.TryPurchase();
        if (success)
            SetBuyButtonState(false);
    }

    private void OnPurchaseMade(int count)
    {
        SetBuyButtonState(false);
    }

    private void OnFeedingToggleClicked()
    {
        bool current = FeedingSystem.Instance.feedingModeActive;
        FeedingSystem.Instance.ToggleFeedingMode(!current);
        UpdateFeedingToggleText();
    }

    private void UpdateFeedingToggleText()
    {
        if (feedingToggleText == null) return;
        feedingToggleText.text = FeedingSystem.Instance.feedingModeActive
            ? "Feeding: ON"
            : "Feeding: OFF";
    }

    private void OnShopClicked()
    {
        ShopPopup.Instance?.Show();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrency;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnCurrencyThresholdReached -= OnThresholdReached;
            LevelManager.Instance.OnPurchaseMade -= OnPurchaseMade;
        }
    }
}