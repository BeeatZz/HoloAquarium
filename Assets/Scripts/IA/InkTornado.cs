using UnityEngine;
using System.Collections;

public class InkTornado : MonoBehaviour
{
    private float expandSpeed;
    private float maxRadius;
    private float duration;
    private float damage;
    private float currentRadius;

    public void Init(float speed, float max, float dur, float dmg)
    {
        expandSpeed = speed;
        maxRadius = max;
        duration = dur;
        damage = dmg;
        currentRadius = 0f;
        StartCoroutine(TornadoSequence());
    }

    private IEnumerator TornadoSequence()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentRadius = Mathf.Lerp(0, maxRadius, elapsed / duration);

            transform.localScale = Vector3.one * currentRadius * 2f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position, currentRadius,
                LayerMask.GetMask("GremHitbox")
            );

            foreach (Collider2D hit in hits)
            {
                Gremurin grem = hit.GetComponentInParent<Gremurin>();
                if (grem != null && !grem.isDead)
                    grem.TakeDamage(damage * Time.deltaTime);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, currentRadius);
    }
}