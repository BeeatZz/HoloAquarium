using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour
{

    [Header("Settings")]
    public bool facingLeftByDefault = false;

    [Header("Stats")]
    public float maxHealth = 3f;
    public float moveSpeed = 1.5f;
    public float damage = 1f;
    public float attackCooldown = 1.5f;
    public float bossDamageValue = 10f;

    [Header("Runtime")]
    public float currentHealth;
    public bool isDead;

    protected float attackTimer;
    protected Gremurin targetGrem;
    protected CurrencyDrop targetDrop;
    protected Transform visual;
    protected SpriteRenderer sr;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        attackTimer = attackCooldown;

        sr = GetComponent<SpriteRenderer>();

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    protected virtual void Update()
    {
        if (isDead) return;
        attackTimer -= Time.deltaTime;
        Think();
    }

    protected virtual void Think()
    {
        FindTarget();
        MoveTowardTarget();
        TryAttack();
    }

    protected virtual void FindTarget() => targetGrem = FindNearestGrem();

    protected virtual void MoveTowardTarget()
    {
        if (targetGrem == null) return;
        transform.position = Vector3.MoveTowards(transform.position, targetGrem.transform.position, moveSpeed * Time.deltaTime);
        UpdateFacing(targetGrem.transform.position);
    }

    protected virtual void TryAttack()
    {
        if (attackTimer > 0 || targetGrem == null) return;
        if (Vector3.Distance(transform.position, targetGrem.transform.position) < 0.4f)
        {
            targetGrem.TakeDamage(damage);
            attackTimer = attackCooldown;
        }
    }

    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;

        transform.DOKill();
        transform.DOPunchScale(new Vector3(0.2f, -0.2f, 0), 0.15f, 10, 1);

        sr.DOKill();
        sr.color = Color.white;
        sr.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);

        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        if (BossHealthBar.Instance != null)
        {
            BossHealthBar.Instance.TakeDamage(bossDamageValue);
        }
        isDead = true;
        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
    }

    protected void UpdateFacing(Vector3 targetPos)
    {
        if (sr == null) return;

        float diff = targetPos.x - transform.position.x;
        if (Mathf.Abs(diff) > 0.01f)
        {
            bool shouldFlip = diff < 0;
            sr.flipX = facingLeftByDefault ? !shouldFlip : shouldFlip;
        }
    }

    protected Gremurin FindNearestGrem()
    {
        Gremurin[] grems = FindObjectsByType<Gremurin>(FindObjectsSortMode.None);
        Gremurin nearest = null;
        float nearestDist = float.MaxValue;
        foreach (Gremurin g in grems)
        {
            if (g.isDead) continue;
            float d = Vector3.Distance(transform.position, g.transform.position);
            if (d < nearestDist) { nearestDist = d; nearest = g; }
        }
        return nearest;
    }

    public virtual void OnPlayerPunch(float punchDamage) => TakeDamage(punchDamage);
}