using UnityEngine;
using DG.Tweening;

public class GeowEnemy : Enemy
{
    [Header("Geow Settings")]
    public float meteorWarningDuration = 1.2f;
    public float meteorDamage = 2f;
    public float meteorRadius = 0.6f;
    public GameObject meteorWarningPrefab;

    private bool isSummoning;

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
    }

    protected override void Think()
    {
        if (isSummoning) return;
        base.Think();

        if (attackTimer <= 0 && !isSummoning)
        {
            Gremurin target = FindNearestGrem();
            if (target != null)
            {
                StartCoroutine(SummonMeteor(target.transform.position));
                attackTimer = attackCooldown;
            }
        }
    }

    private System.Collections.IEnumerator SummonMeteor(Vector3 targetPos)
    {
        isSummoning = true;

        if (meteorWarningPrefab != null)
        {
            GameObject warning = Instantiate(meteorWarningPrefab, targetPos, Quaternion.identity);

            warning.transform.localScale = Vector3.zero;
            warning.transform.DOScale(Vector3.one * meteorRadius * 2f, meteorWarningDuration * 0.5f)
                .SetEase(Ease.OutBack);

            yield return new WaitForSeconds(meteorWarningDuration);

            Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, meteorRadius);
            foreach (Collider2D hit in hits)
            {
                Gremurin grem = hit.GetComponent<Gremurin>();
                if (grem != null && !grem.isDead)
                    grem.TakeDamage(meteorDamage);
            }

            warning.transform.DOPunchScale(Vector3.one * 0.5f, 0.2f, 5, 0.5f)
                .OnComplete(() => Destroy(warning));
        }
        else
        {
            yield return new WaitForSeconds(meteorWarningDuration);

            int hitboxLayer = LayerMask.GetMask("GremHitbox");
            Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, meteorRadius, hitboxLayer);
            foreach (Collider2D hit in hits)
            {
                Gremurin grem = hit.GetComponentInParent<Gremurin>();
                if (grem != null && !grem.isDead)
                    grem.TakeDamage(meteorDamage);
            }
        }

        isSummoning = false;
    }
}