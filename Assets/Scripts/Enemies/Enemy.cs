using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 3f;
    public float moveSpeed = 1.5f;
    public float damage = 1f;
    public float attackCooldown = 1.5f;

    [Header("Runtime")]
    public float currentHealth;
    public bool isDead;

    protected float attackTimer;
    protected Gremurin targetGrem;
    protected CurrencyDrop targetDrop;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        attackTimer = attackCooldown;

        // Spawn punch in
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    protected virtual void Update()
    {
        if (isDead) return;

        attackTimer -= Time.deltaTime;
        Think();
    }

    // Override in subclasses to define behaviour
    protected virtual void Think()
    {
        FindTarget();
        MoveTowardTarget();
        TryAttack();
    }

    protected virtual void FindTarget()
    {
        // Default: target nearest grem
        targetGrem = FindNearestGrem();
    }

    protected virtual void MoveTowardTarget()
    {
        if (targetGrem == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetGrem.transform.position,
            moveSpeed * Time.deltaTime
        );
    }

    protected virtual void TryAttack()
    {
        if (attackTimer > 0) return;
        if (targetGrem == null) return;

        float dist = Vector3.Distance(transform.position, targetGrem.transform.position);
        if (dist < 0.4f)
        {
            targetGrem.TakeDamage(damage);
            attackTimer = attackCooldown;
        }
    }

    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        // Hit flash
        transform.DOPunchScale(Vector3.one * 0.2f, 0.1f, 5, 0.5f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;

        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }

    protected Gremurin FindNearestGrem()
    {
        Gremurin[] grems = FindObjectsByType<Gremurin>(FindObjectsSortMode.None);
        Gremurin nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Gremurin g in grems)
        {
            if (g.isDead) continue;
            float dist = Vector3.Distance(transform.position, g.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = g;
            }
        }

        return nearest;
    }

    // Called by player click
    public virtual void OnPlayerPunch(float punchDamage)
    {
        TakeDamage(punchDamage);
    }

    
}