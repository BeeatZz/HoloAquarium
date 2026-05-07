using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance { get; private set; }

    [Header("Settings")]
    public float punchDamage = 1f;
    public float holdThreshold = 0.2f;

    [Header("Drag Settings")]
    public float dragFollowSpeed = 12f;
    public float dragScale = 1.25f;

    // Drag state
    private Gremurin draggedGrem;
    private Gremurin pressedGrem;
    private Vector3 dragOffset;
    private float holdTimer;
    private bool isDragging;

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

        if (Mouse.current.leftButton.isPressed && pressedGrem != null)
            HandleHold();

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            HandleRelease();
    }

    private bool IsClickOnPopup()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        return GremInfoPopup.Instance != null &&
               GremInfoPopup.Instance.IsClickInsidePopup(mouseScreen);
    }

    private void HandlePress()
    {
        if (IsClickOnPopup()) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(mouseScreen);

        Collider2D hit = Physics2D.OverlapPoint(worldPoint);

        if (hit == null)
        {
            GremInfoPopup.Instance?.Hide();
            if (FeedingSystem.Instance != null && FeedingSystem.Instance.feedingModeActive)
            {
                // Only place food within play area
                if (IsWithinPlayArea(worldPoint))
                    FeedingSystem.Instance.TryPlaceFoodAt(worldPoint);
            }
            return;
        }

        CurrencyDrop drop = hit.GetComponent<CurrencyDrop>();
        if (drop != null) { drop.Collect(); return; }

        Enemy enemy = hit.GetComponent<Enemy>();
        if (enemy != null) { PunchEnemy(enemy); return; }

        GremEgg egg = hit.GetComponent<GremEgg>();
        if (egg != null) { egg.Hatch(); return; }

        Gremurin grem = hit.GetComponent<Gremurin>();
        if (grem != null && !grem.isDead)
        {
            // Only pick up grems within play area
            if (!IsWithinPlayArea(worldPoint)) return;

            pressedGrem = grem;
            holdTimer = 0f;
            isDragging = false;

            Vector3 wp3 = Camera.main.ScreenToWorldPoint(mouseScreen);
            wp3.z = 0;
            dragOffset = grem.transform.position - wp3;
            return;
        }

        GremInfoPopup.Instance?.Hide();
    }
    private bool IsPointerOverUI()
    {
        var pointerEventData = new UnityEngine.EventSystems.PointerEventData(
            UnityEngine.EventSystems.EventSystem.current
        );
        pointerEventData.position = Mouse.current.position.ReadValue();

        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerEventData, results);

        return results.Count > 0;
    }
    private void HandleHold()
    {
        if (pressedGrem == null || pressedGrem.isDead)
        {
            pressedGrem = null;
            return;
        }

        holdTimer += Time.deltaTime;

        // Transition from Press to Drag
        if (!isDragging && holdTimer >= holdThreshold)
        {
            isDragging = true;
            draggedGrem = pressedGrem;

            // Tell the Gremurin it is being held so it stops its own AI logic
            draggedGrem.OnPickedUp();

            GremInfoPopup.Instance?.Hide();

            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 currentWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            currentWorld.z = 0;
            dragOffset = draggedGrem.transform.position - currentWorld;

            draggedGrem.transform.DOKill();
            draggedGrem.transform.DOScale(Vector3.one * dragScale, 0.15f)
                .SetEase(Ease.OutBack);
        }

        if (isDragging && draggedGrem != null)
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mouseScreen);
            worldPoint.z = 0;

            Vector3 targetPos = LevelManager.Instance.ClampToPlayArea(worldPoint + dragOffset);

            draggedGrem.transform.position = Vector3.Lerp(
                draggedGrem.transform.position,
                targetPos,
                dragFollowSpeed * Time.deltaTime
            );

            // Keep the Gremurin's internal position in sync with the drag
            draggedGrem.SetBasePosition(draggedGrem.transform.position);
        }
    }

    private void HandleRelease()
    {
        if (pressedGrem != null && !isDragging)
        {
            if (GremInfoPopup.Instance != null)
            {
                if (GremInfoPopup.Instance.IsShowing(pressedGrem))
                    GremInfoPopup.Instance.Hide();
                else
                    GremInfoPopup.Instance.Show(pressedGrem);
            }
        }

        if (draggedGrem != null)
        {
            // Tell the Gremurin it was dropped so it can resume its AI
            draggedGrem.OnReleased();

            draggedGrem.transform.DOKill();
            draggedGrem.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack);
        }

        pressedGrem = null;
        draggedGrem = null;
        isDragging = false;
        holdTimer = 0f;
    }
    private bool IsWithinPlayArea(Vector2 worldPoint)
    {
        Vector2 min = LevelManager.Instance.playAreaMin;
        Vector2 max = LevelManager.Instance.playAreaMax;
        return worldPoint.x >= min.x && worldPoint.x <= max.x &&
               worldPoint.y >= min.y && worldPoint.y <= max.y;
    }
    public void PunchEnemy(Enemy enemy)
    {
        enemy.OnPlayerPunch(punchDamage);
    }
}