using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance { get; private set; }

    [Header("Settings")]
    public float punchDamage = 1f;

    // Drag state
    private Gremurin draggedGrem;
    private Vector3 dragOffset;

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
        if (Mouse.current.leftButton.wasPressedThisFrame)
            HandlePress();

        if (Mouse.current.leftButton.isPressed && draggedGrem != null)
            HandleDrag();

        if (Mouse.current.leftButton.wasReleasedThisFrame && draggedGrem != null)
            HandleRelease();
    }

    private void HandlePress()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(mouseScreen);

        Collider2D hit = Physics2D.OverlapPoint(worldPoint);

        if (hit == null)
        {
            if (FeedingSystem.Instance != null && FeedingSystem.Instance.feedingModeActive)
                FeedingSystem.Instance.TryPlaceFoodAt(worldPoint);
            return;
        }

        // Priority 1: collect currency drop
        CurrencyDrop drop = hit.GetComponent<CurrencyDrop>();
        if (drop != null)
        {
            drop.Collect();
            return;
        }

        // Priority 2: punch enemy
        Enemy enemy = hit.GetComponent<Enemy>();
        if (enemy != null)
        {
            PunchEnemy(enemy);
            return;
        }

        // Priority 3: start dragging grem
        Gremurin grem = hit.GetComponent<Gremurin>();
        if (grem != null && !grem.isDead)
        {
            draggedGrem = grem;
            dragOffset = grem.transform.position - (Vector3)worldPoint;
            return;
        }
    }

    private void HandleDrag()
    {
        if (draggedGrem == null || draggedGrem.isDead)
        {
            draggedGrem = null;
            return;
        }

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mouseScreen);
        worldPoint.z = 0;

        Vector3 newPos = worldPoint + dragOffset;
        draggedGrem.transform.position = LevelManager.Instance.ClampToPlayArea(newPos);
        draggedGrem.SetBasePosition(draggedGrem.transform.position);
    }

    private void HandleRelease()
    {
        draggedGrem = null;
    }

    public void PunchEnemy(Enemy enemy)
    {
        enemy.OnPlayerPunch(punchDamage);
    }
}