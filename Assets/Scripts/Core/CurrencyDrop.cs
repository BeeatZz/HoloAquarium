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

    private void OnMouseDown()
    {
        if (!isCollectable || collected) return;
        Collect();
    }

    private void Collect()
    {
        collected = true;

        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.15f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                CurrencyManager.Instance.Add(amount);
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