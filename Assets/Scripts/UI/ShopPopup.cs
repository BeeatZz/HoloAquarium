using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopPopup : MonoBehaviour
{
    public static ShopPopup Instance { get; private set; }

    [Header("References")]
    public GameObject itemTemplate;
    public Transform itemContainer;
    public Button closeButton;

    [Header("Available Grems")]
    public List<ShopItem> availableGrems;

    public bool isVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        closeButton?.onClick.AddListener(Hide);
        Hide();
    }

    public void Show()
    {
        isVisible = true;
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        PopulateShop();
    }

    public void Hide()
    {
        isVisible = false;
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private void PopulateShop()
    {
        // Clear existing items
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        foreach (ShopItem item in availableGrems)
        {
            GameObject entry = Instantiate(itemTemplate, itemContainer);
            entry.SetActive(true);

            // Name
            TextMeshProUGUI nameText = entry.transform.Find("NameText")
                ?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = item.gremData.gremName;

            // Cost
            TextMeshProUGUI costText = entry.transform.Find("CostText")
                ?.GetComponent<TextMeshProUGUI>();
            if (costText != null)
                costText.text = $"{item.cost}";

            // Photo
            Image photo = entry.transform.Find("Photo")?.GetComponent<Image>();
            if (photo != null && item.gremData.sprite != null)
                photo.sprite = item.gremData.sprite;

            // Buy button
            Button buyBtn = entry.transform.Find("BuyButton")?.GetComponent<Button>();
            if (buyBtn != null)
            {
                ShopItem captured = item;
                buyBtn.onClick.AddListener(() => OnBuyGrem(captured));
            }
        }
    }

    private void OnBuyGrem(ShopItem item)
    {
        if (!CurrencyManager.Instance.Spend(item.cost)) return;

        Hide();
        GremEggSpawner.Instance?.SpawnEgg(item.gremData);
    }
}

[System.Serializable]
public class ShopItem
{
    public GremData gremData;
    public float cost;
}