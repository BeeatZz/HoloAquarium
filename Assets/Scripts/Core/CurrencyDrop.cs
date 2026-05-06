using UnityEngine;
using DG.Tweening;

public class CurrencyDrop : MonoBehaviour
{
    [Header("Settings")]
    public float bobSpeed = 2f;
    public float bobAmplitude = 0.08f;
    public float spawnArcHeight = 0.5f;
    public float spawnDuration = 0.4f;
    public float spawnLandOffset = 0.6f;

    // Internal
    private float amount;
    private float lifetime;
    private float lifetimeTimer;
    private bool isCollectable;
    private bool collected;
    private Vector3 basePosition;

    public void Init(float dropAmount, float dropLifetime)
    {
        amount = dropAmount;
        lifetime = dropLifetime;
        lifetimeTimer = dropLifetime;
        isCollectable = false;
        collected = false;

        PlaySpawnAnimation();
    }

    private void Update()
    {
        if (!isCollectable || collected) return;

        // Idle bob
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(
            basePosition.x,
            basePosition.y + bob,
            basePosition.z
        );

        // Lifetime countdown
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            Expire();
        }
    }

    private void PlaySpawnAnimation()
    {
        Vector3 startPosition = transform.position;
        Vector3 landPosition = startPosition + new Vector3(
            Random.Range(-spawnLandOffset, spawnLandOffset),
            Random.Range(-spawnLandOffset, spawnLandOffset),
            0
        );

        Vector3 midPoint = Vector3.Lerp(startPosition, landPosition, 0.5f)
            + new Vector3(0, spawnArcHeight, 0);

        transform.DOPath(
            new Vector3[] { midPoint, landPosition },
            spawnDuration,
            PathType.CatmullRom
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            basePosition = transform.position;
            isCollectable = true;
            transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 5, 0.5f);
        });

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    public void Collect()
    {
        if (!isCollectable || collected) return;

        collected = true;
        transform.DOKill();

        // Convert UI position to world space
        Vector3 targetWorldPos = Camera.main.ScreenToWorldPoint(CurrencyManager.Instance.GetCounterScreenPos());
        targetWorldPos.z = 0;

        // Set a unified duration for perfect sync
        float travelTime = 0.4f;

        Sequence s = DOTween.Sequence();

        // Flight with your 1f pullback
        s.Append(transform.DOMove(targetWorldPos, travelTime).SetEase(Ease.InBack, 1f));

        // Shrink - matched to travelTime so it finishes exactly upon arrival
        s.Join(transform.DOScale(Vector3.one * 0.3f, travelTime).SetEase(Ease.InQuad));

        s.OnComplete(() =>
        {
            // 1. Add the logic/value
            CurrencyManager.Instance.Add(amount);

            // 2. Trigger the UI feedback exactly now
            // We use transform.DOPunchScale on the actual UI element
            CurrencyManager.Instance.counterIcon.DOKill(true); // Reset any current punch
            CurrencyManager.Instance.counterIcon.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), 0.15f, 10, 1f);

            // 3. Cleanup
            Destroy(gameObject);
        });
    }



    private void Expire()
    {
        collected = true;
        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.3f)
            .OnComplete(() => Destroy(gameObject));
    }
}