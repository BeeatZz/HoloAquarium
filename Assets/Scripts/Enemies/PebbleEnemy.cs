using UnityEngine;
using DG.Tweening;

public class PebbleEnemy : Enemy
{
    [Header("Pebble Settings")]
    public float dropStealRange = 0.4f;

    protected override void FindTarget()
    {
        // Prefer currency drops over grems
        CurrencyDrop nearestDrop = FindNearestDrop();

        if (nearestDrop != null)
        {
            targetDrop = nearestDrop;
            targetGrem = null;
        }
        else
        {
            targetDrop = null;
            targetGrem = FindNearestGrem();
        }
    }

    protected override void MoveTowardTarget()
    {
        if (targetDrop != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetDrop.transform.position,
                moveSpeed * Time.deltaTime
            );
        }
        else if (targetGrem != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetGrem.transform.position,
                moveSpeed * Time.deltaTime
            );
        }
    }

    protected override void TryAttack()
    {
        if (attackTimer > 0) return;

        // Steal drop if in range
        if (targetDrop != null)
        {
            float dist = Vector3.Distance(transform.position, targetDrop.transform.position);
            if (dist < dropStealRange)
            {
                StealDrop(targetDrop);
                attackTimer = attackCooldown;
                return;
            }
        }

        // Otherwise attack grem
        if (targetGrem != null)
        {
            float dist = Vector3.Distance(transform.position, targetGrem.transform.position);
            if (dist < 0.4f)
            {
                targetGrem.TakeDamage(damage);
                attackTimer = attackCooldown;
            }
        }
    }

    private void StealDrop(CurrencyDrop drop)
    {
        if (drop == null) return;

        // Animate toward self then destroy drop
        drop.transform.DOKill();
        drop.transform.DOMove(transform.position, 0.2f)
            .OnComplete(() =>
            {
                if (drop != null)
                    Destroy(drop.gameObject);
            });

        // Pebble does a little happy bounce
        transform.DOPunchScale(Vector3.one * 0.4f, 0.3f, 5, 0.5f);
    }

    private CurrencyDrop FindNearestDrop()
    {
        CurrencyDrop[] drops = FindObjectsByType<CurrencyDrop>(FindObjectsSortMode.None);
        CurrencyDrop nearest = null;
        float nearestDist = float.MaxValue;

        foreach (CurrencyDrop drop in drops)
        {
            float dist = Vector3.Distance(transform.position, drop.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = drop;
            }
        }

        return nearest;
    }
}