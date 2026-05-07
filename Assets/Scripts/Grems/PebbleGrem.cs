using UnityEngine;

public class PebbleGrem : Gremurin
{
    [Header("Collector Settings")]
    public float collectRadius = 0.3f;
    public float detectionRadius = 3f;
    public float collectorMoveSpeed = 2.5f;
    public float intimidateRadius = 2f;
    public float intimidateCooldown = 0.5f;

    private CurrencyDrop targetDrop;
    private float intimidateTimer;

    protected override void Start()
    {
        base.Start();
        moveSpeed = collectorMoveSpeed;
    }

    protected override void HandleWander()
    {
        // Hunger still takes priority
        if (currentHunger < data.maxHunger * seekFoodHungerThreshold)
        {
            SeekFood();
            return;
        }

        HandleCollection();
        HandleIntimidate();
    }

    private void HandleCollection()
    {
        if (targetDrop == null)
            targetDrop = FindNearestDrop();

        if (targetDrop == null)
        {
            isMoving = false;
            return;
        }

        isMoving = true;

        Vector3 newPos = Vector3.MoveTowards(
            transform.position,
            targetDrop.transform.position,
            moveSpeed * Time.deltaTime
        );
        transform.position = LevelManager.Instance.ClampToPlayArea(newPos);
        UpdateFacing(targetDrop.transform.position);
        basePosition = transform.position;

        float dist = Vector3.Distance(transform.position, targetDrop.transform.position);
        if (dist < collectRadius)
        {
            targetDrop.Collect();
            targetDrop = null;
        }
    }

    private void HandleIntimidate()
    {
        intimidateTimer -= Time.deltaTime;
        if (intimidateTimer > 0) return;
        intimidateTimer = intimidateCooldown;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, intimidateRadius);
        foreach (Collider2D hit in hits)
        {
            PebbleEnemy pebble = hit.GetComponent<PebbleEnemy>();
            if (pebble != null)
                pebble.SetIntimidated(transform.position);
        }
    }

    private CurrencyDrop FindNearestDrop()
    {
        CurrencyDrop[] drops = FindObjectsByType<CurrencyDrop>(FindObjectsSortMode.None);
        CurrencyDrop nearest = null;
        float nearestDist = float.MaxValue;

        foreach (CurrencyDrop drop in drops)
        {
            float dist = Vector3.Distance(transform.position, drop.transform.position);
            if (dist < nearestDist && dist <= detectionRadius)
            {
                nearestDist = dist;
                nearest = drop;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, intimidateRadius);
    }
}