using UnityEngine;
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceFood();
        }
    }

    private void TryPlaceFood()
    {
        if (foodPrefab == null) return;
        if (!CurrencyManager.Instance.CanAfford(foodCost)) return;

        // Raycast to find placement point on the level plane
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);

            // Don't place food if clicking a currency drop
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit.GetComponent<CurrencyDrop>() != null) return;

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