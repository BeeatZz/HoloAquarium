using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance { get; private set; }

    [Header("Settings")]
    public float punchDamage = 1f;
    public float holdThreshold = 0.2f;
    public float dragFollowSpeed = 12f;
    public float dragScale = 1.25f;

    private Gremurin draggedGrem;
    private Gremurin pressedGrem;
    private Vector3 dragOffset;
    private float holdTimer;
    private bool isDragging;

    private void Awake() { if (Instance != null) Destroy(gameObject); else Instance = this; }

    private void Update()
    {
        bool is3D = LevelManager.Instance != null && LevelManager.Instance.is3DPerspectiveMode;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (is3D) HandlePress3D(); else HandlePress();
        }

        if (Mouse.current.leftButton.isPressed && pressedGrem != null)
        {
            if (is3D) HandleHold3D(); else HandleHold();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame) HandleRelease();
    }

    private void HandlePress()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(mouseScreen);

        if (GremInfoPopup.Instance != null && GremInfoPopup.Instance.IsClickInsidePopup(mouseScreen)) return;

        int currencyLayer = LayerMask.GetMask("Currency");
        Collider2D coinHit = Physics2D.OverlapPoint(worldPoint, currencyLayer);
        if (coinHit != null && coinHit.TryGetComponent(out CurrencyDrop drop)) { drop.Collect(); return; }

        Collider2D generalHit = Physics2D.OverlapPoint(worldPoint);
        if (generalHit != null)
        {
            if (generalHit.TryGetComponent(out Enemy enemy)) { enemy.OnPlayerPunch(punchDamage); return; }
            if (generalHit.TryGetComponent(out GremEgg egg)) { egg.Hatch(); return; }
        }

        if (IsWithinPlayArea(worldPoint))
        {
            if (FeedingSystem.Instance != null && FeedingSystem.Instance.feedingModeActive) { FeedingSystem.Instance.TryPlaceFoodAt(worldPoint); return; }
            else if (ProtectionSystem.Instance != null && ProtectionSystem.Instance.protectionModeActive) { ProtectionSystem.Instance.TryPlaceBarrierAt(worldPoint); return; }
        }

        int pickupLayer = LayerMask.GetMask("GremPickup");
        Collider2D gremHit = Physics2D.OverlapPoint(worldPoint, pickupLayer);
        if (gremHit != null && gremHit.TryGetComponent(out Gremurin grem) && !grem.isDead)
        {
            if (grem.isCharmed) grem.OnSpamClicked();
            pressedGrem = grem;
            holdTimer = 0f;
            isDragging = false;
            Vector3 wp3 = (Vector3)worldPoint; wp3.z = 0;
            dragOffset = grem.transform.position - wp3;
            return;
        }
        GremInfoPopup.Instance?.Hide();
    }

    private void HandleHold()
    {
        if (pressedGrem == null || pressedGrem.isDead) return;
        holdTimer += Time.deltaTime;
        if (!isDragging && holdTimer >= holdThreshold)
        {
            isDragging = true;
            draggedGrem = pressedGrem;
            draggedGrem.OnPickedUp();
            GremInfoPopup.Instance?.Hide();
            draggedGrem.transform.DOKill();
            draggedGrem.transform.DOScale(Vector3.one * dragScale, 0.15f).SetEase(Ease.OutBack);
        }
        if (isDragging && draggedGrem != null)
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); wp.z = 0;
            Vector3 target = LevelManager.Instance.ClampToPlayArea(wp + dragOffset);
            draggedGrem.transform.position = Vector3.Lerp(draggedGrem.transform.position, target, dragFollowSpeed * Time.deltaTime);
            draggedGrem.SetBasePosition(draggedGrem.transform.position);
        }
    }

    private void HandlePress3D()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mouseScreen);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (GremInfoPopup.Instance != null && GremInfoPopup.Instance.IsClickInsidePopup(mouseScreen)) return;

        Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
        Vector3 worldPoint = Vector3.zero;
        if (groundPlane.Raycast(ray, out float dist)) worldPoint = ray.GetPoint(dist);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Currency") && hit.collider.TryGetComponent(out CurrencyDrop drop)) { drop.Collect(); return; }
            if (hit.collider.TryGetComponent(out Enemy enemy)) { enemy.OnPlayerPunch(punchDamage); return; }
            if (hit.collider.TryGetComponent(out GremEgg egg)) { egg.Hatch(); return; }

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("GremPickup") && hit.collider.TryGetComponent(out Gremurin grem) && !grem.isDead)
            {
                if (grem.isCharmed) grem.OnSpamClicked();
                pressedGrem = grem;
                holdTimer = 0f;
                isDragging = false;
                dragOffset = grem.transform.position - worldPoint;
                return;
            }
        }

        if (IsWithinPlayArea(worldPoint))
        {
            if (FeedingSystem.Instance != null && FeedingSystem.Instance.feedingModeActive) FeedingSystem.Instance.TryPlaceFoodAt(worldPoint);
            else if (ProtectionSystem.Instance != null && ProtectionSystem.Instance.protectionModeActive) ProtectionSystem.Instance.TryPlaceBarrierAt(worldPoint);
        }
        GremInfoPopup.Instance?.Hide();
    }

    private void HandleHold3D()
    {
        if (pressedGrem == null || pressedGrem.isDead) return;
        holdTimer += Time.deltaTime;

        if (!isDragging && holdTimer >= holdThreshold)
        {
            isDragging = true;
            draggedGrem = pressedGrem;
            draggedGrem.OnPickedUp();
            draggedGrem.transform.DOKill();
            draggedGrem.transform.DOScale(Vector3.one * dragScale, 0.15f);
        }

        if (isDragging && draggedGrem != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
            if (groundPlane.Raycast(ray, out float dist))
            {
                Vector3 wp = ray.GetPoint(dist);
                Vector3 target = LevelManager.Instance.ClampToPlayArea(wp + dragOffset);
                draggedGrem.transform.position = Vector3.Lerp(draggedGrem.transform.position, target, dragFollowSpeed * Time.deltaTime);
                draggedGrem.SetBasePosition(draggedGrem.transform.position);
            }
        }
    }

    private void HandleRelease()
    {
        if (pressedGrem != null && !isDragging)
        {
            bool inCooldown = (Time.time - pressedGrem.lastUncharmedTime) < 2.0f;
            if (GremInfoPopup.Instance != null && !pressedGrem.isCharmed && !inCooldown)
            {
                if (GremInfoPopup.Instance.IsShowing(pressedGrem)) GremInfoPopup.Instance.Hide();
                else GremInfoPopup.Instance.Show(pressedGrem);
            }
        }
        if (draggedGrem != null) { draggedGrem.OnReleased(); draggedGrem.transform.DOKill(); draggedGrem.transform.DOScale(Vector3.one, 0.15f); }
        pressedGrem = null; draggedGrem = null; isDragging = false;
    }

    private bool IsWithinPlayArea(Vector2 wp) => wp.x >= LevelManager.Instance.playAreaMin.x && wp.x <= LevelManager.Instance.playAreaMax.x && wp.y >= LevelManager.Instance.playAreaMin.y && wp.y <= LevelManager.Instance.playAreaMax.y;
}