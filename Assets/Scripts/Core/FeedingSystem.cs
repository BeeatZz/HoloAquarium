using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class FeedingSystem : MonoBehaviour
{
    public static FeedingSystem Instance { get; private set; }

    [Header("Settings")]
    public GameObject foodPrefab;
    public float foodCost = 5f;
    public float foodHungerRestoreAmount = 40f;
    public float foodLifetime = 10f;
    public float foodDetectionRadius = 1f;

    [Header("State")]
    public bool feedingModeActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

 
    public void ToggleFeedingMode(bool active)
    {
        feedingModeActive = active;
    }

    public void TryPlaceFoodAt(Vector2 position)
    {
        if (foodPrefab == null) return;
        if (!CurrencyManager.Instance.CanAfford(foodCost)) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mouseScreen);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);

            // Don't place food if clicking a currency drop or enemy
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.GetComponent<CurrencyDrop>() != null) return;
            if (hit != null && hit.GetComponent<Enemy>() != null) return;

            CurrencyManager.Instance.Spend(foodCost);
            SpawnFood(worldPoint);
        }
    }

    private void SpawnFood(Vector3 position)
    {
        GameObject food = Instantiate(foodPrefab, position, Quaternion.identity);
        FoodItem foodItem = food.GetComponent<FoodItem>();

        if (foodItem != null)
        {
            foodItem.Init(foodHungerRestoreAmount, foodLifetime, foodDetectionRadius);
        }
    }
}