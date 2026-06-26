using UnityEngine;
using DG.Tweening;

public class NoveliteGremProjectile : MonoBehaviour
{
    public bool isBlue; // True = Buff, False = Ground Hazard Warning
    public float travelTime = 1.8f;
    public float jumpPower = 3.0f;

    [Header("Hazard Prefab Reference")]
    public GameObject redHazardWarningPrefab; // Drag an object with a warning flash behavior here
    public GameObject blueBuffVfxPrefab;

    private Vector3 targetFloorPosition;

    public void Launch(Transform targetGrem)
    {
        // Cache the exact floor vector where the Grem was standing at launch
        targetFloorPosition = targetGrem.position;

        // 1. If it's a dangerous RED projectile, drop a static warning circle right there
        if (!isBlue && redHazardWarningPrefab != null)
        {
            GameObject warning = Instantiate(redHazardWarningPrefab, targetFloorPosition, Quaternion.identity);

            // If your warning prefab uses a script similar to your StompEffect, initialize it:
            // warning.GetComponent<YourWarningScript>().SetupWarning(travelTime);
        }

        // 2. Lob the projectile towards that specific coordinate vector (not tracking the moving transform)
        transform.DOJump(targetFloorPosition, jumpPower, 1, travelTime)
            .SetEase(Ease.Linear)
            .OnComplete(ApplyImpact);
    }

    private void ApplyImpact()
    {
        // Check what is sitting inside the blast radius on impact
        float checkRadius = 1.2f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetFloorPosition, checkRadius);

        foreach (var hit in hits)
        {
            Gremurin grem = hit.GetComponentInParent<Gremurin>() ?? hit.GetComponent<Gremurin>();
            if (grem == null || grem.isDead) continue;

            if (isBlue)
            {
                // Empower: Apply rapid production / speed buff
                grem.moveSpeed *= 1.5f;
                if (blueBuffVfxPrefab != null) Instantiate(blueBuffVfxPrefab, grem.transform.position, Quaternion.identity);
            }
            else
            {
                // Damage: Caught inside the unevaded red hazard zone!
                // If the player failed to pull them out of the warning area, apply red ink burden
                if (!grem.isPickedUp)
                {
                    grem.TakeDamage(20f); // Instant chunk damage
                    if (!grem.gameObject.GetComponent<RedInkHazard>())
                    {
                        grem.gameObject.AddComponent<RedInkHazard>();
                    }
                }
            }
        }

        // Spawn local splash visual particles here if needed
        Destroy(gameObject);
    }
}