using UnityEngine;
using DG.Tweening;

public class FoodItem : MonoBehaviour
{
    [Header("Idle Settings")]
    public float bobSpeed = 1.5f;
    public float bobAmplitude = 0.04f;

    private float hungerRestoreAmount;
    private float lifetime;
    private float detectionRadius;
    private bool consumed;
    private bool readyToEat;
    private Vector3 basePosition;

    public void Init(float restore, float foodLifetime, float radius)
    {
        hungerRestoreAmount = restore;
        lifetime = foodLifetime;
        detectionRadius = radius;
        consumed = false;
        readyToEat = false;

        PlaySpawnAnimation();
        Invoke(nameof(Expire), lifetime);
    }

    private void Update()
    {
        if (consumed || !readyToEat) return;

        HandleIdleBob();
        CheckForHungryGrem();
    }

    private void HandleIdleBob()
    {
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(
            basePosition.x,
            basePosition.y + bob,
            basePosition.z
        );
    }

    private void CheckForHungryGrem()
    {
        // Larger consume radius since grems actively walk to food
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius + 0.2f);

        foreach (Collider2D hit in hits)
        {
            Gremurin grem = hit.GetComponent<Gremurin>();
            if (grem != null && !grem.isDead && grem.currentHunger < grem.data.maxHunger * 0.9f)
            {
                Consume(grem);
                return;
            }
        }
    }

    private void Consume(Gremurin grem)
    {
        consumed = true;
        CancelInvoke();

        grem.Feed(hungerRestoreAmount);

        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }

    private void PlaySpawnAnimation()
    {
        basePosition = transform.position;
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.25f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => readyToEat = true); // only start detecting after animation
    }

    private void Expire()
    {
        if (consumed) return;
        consumed = true;

        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.3f)
            .OnComplete(() => Destroy(gameObject));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}