using UnityEngine;
using DG.Tweening;

public class PebbleEnemy : Enemy
{
    [Header("Pebble Settings")]
    public float dropStealRange = 0.4f;

    [Header("Hop Animation")]
    public float hopSpeed = 8f;
    public float hopHeight = 0.12f;
    public float hopSquishAmount = 0.15f;

    [Header("Intimidation")]
    public float intimidateDuration = 2f;
    public float fleeSpeed = 3f;

    private Vector3 baseScale;
    private float hopTimer;
    private bool isIntimidated;
    private float intimidateTimer;
    private Vector3 fleeFromPos;

    protected override void Start()
    {
        base.Start();
        baseScale = visual != null ? visual.localScale : transform.localScale;
    }

    protected override void Update()
    {
        base.Update();
        HandleHop();
    }

    protected override void Think()
    {
        if (isIntimidated)
        {
            intimidateTimer -= Time.deltaTime;
            if (intimidateTimer <= 0)
                isIntimidated = false;
            else
            {
                Flee();
                return;
            }
        }

        base.Think();
    }

    private void Flee()
    {
        Vector3 fleeDir = (transform.position - fleeFromPos).normalized;
        Vector3 newPos = transform.position + fleeDir * fleeSpeed * Time.deltaTime;
        transform.position = LevelManager.Instance.ClampToPlayArea(newPos);
        UpdateFacing(transform.position + fleeDir);
    }

    public void SetIntimidated(Vector3 sourcePos)
    {
        isIntimidated = true;
        intimidateTimer = intimidateDuration;
        fleeFromPos = sourcePos;
    }

    private void HandleHop()
    {
        if (isDead || visual == null) return;

        hopTimer += Time.deltaTime * hopSpeed;
        float hop = Mathf.Abs(Mathf.Sin(hopTimer));

        visual.localPosition = new Vector3(0, hop * hopHeight, 0);

        float squishY = 1f + hop * hopSquishAmount;
        float squishX = 1f - hop * hopSquishAmount * 0.5f;
        visual.localScale = new Vector3(
            baseScale.x * squishX,
            baseScale.y * squishY,
            baseScale.z
        );
    }

    protected override void FindTarget()
    {
        if (isIntimidated) return;

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
        if (isIntimidated) return;

        if (targetDrop != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetDrop.transform.position,
                moveSpeed * Time.deltaTime
            );
            UpdateFacing(targetDrop.transform.position);
        }
        else if (targetGrem != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetGrem.transform.position,
                moveSpeed * Time.deltaTime
            );
            UpdateFacing(targetGrem.transform.position);
        }
    }

    protected override void TryAttack()
    {
        if (isIntimidated) return;
        if (attackTimer > 0) return;

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

        drop.transform.DOKill();
        drop.transform.DOMove(transform.position, 0.2f)
            .OnComplete(() =>
            {
                if (drop != null)
                    Destroy(drop.gameObject);
            });

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