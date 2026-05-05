using UnityEngine;
using DG.Tweening;

public class PebbleEnemy : Enemy
{
    [Header("Pebble Settings")]
    public float dropStealRange = 0.4f;
    protected CurrencyDrop targetDrop;

    protected override void Start()
    {
        base.Start();
        StartJuice();
    }

    // This makes it "Breathe/Hop" using only scale
    private void StartJuice()
    {
        transform.localScale = Vector3.one;
        // Stretch up and thin out, then squash down
        transform.DOScale(new Vector3(0.85f, 1.2f, 1f), 0.2f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount); // Does the punch
        if (!isDead) DOVirtual.DelayedCall(0.16f, StartJuice); // Restarts juice after punch
    }

    protected override void Think()
    {
        FindTarget();
        MoveTowardTarget();
        TryAttack();
    }

    protected override void FindTarget()
    {
        targetDrop = FindNearestDrop();
        if (targetDrop == null) base.FindTarget();
    }

    protected override void MoveTowardTarget()
    {
        Vector3 dest = targetDrop != null ? targetDrop.transform.position : (targetGrem != null ? targetGrem.transform.position : Vector3.zero);
        if (dest != Vector3.zero)
        {
            transform.position = Vector3.MoveTowards(transform.position, dest, moveSpeed * Time.deltaTime);
            UpdateFacing(dest);
        }
    }

    protected override void TryAttack()
    {
        if (attackTimer > 0) return;
        if (targetDrop != null && Vector3.Distance(transform.position, targetDrop.transform.position) < dropStealRange)
        {
            StealDrop(targetDrop);
            attackTimer = attackCooldown;
        }
        else base.TryAttack();
    }

    private void StealDrop(CurrencyDrop drop)
    {
        if (drop == null) return;
        drop.transform.DOMove(transform.position, 0.2f).OnComplete(() => { if (drop) Destroy(drop.gameObject); });
        transform.DOPunchScale(Vector3.one * 0.3f, 0.2f);
    }

    private CurrencyDrop FindNearestDrop()
    {
        CurrencyDrop[] drops = FindObjectsByType<CurrencyDrop>(FindObjectsSortMode.None);
        CurrencyDrop nearest = null;
        float minDist = float.MaxValue;
        foreach (var d in drops)
        {
            float dist = Vector3.Distance(transform.position, d.transform.position);
            if (dist < minDist) { minDist = dist; nearest = d; }
        }
        return nearest;
    }
}