using UnityEngine;
using DG.Tweening;

public class NoveliteGremProjectile : MonoBehaviour
{
    public bool isBlue; 
    public float travelTime = 1.8f;
    public float jumpPower = 3.0f;

    [Header("Hazard Prefab Reference")]
    public GameObject redHazardWarningPrefab; 
    public GameObject blueBuffVfxPrefab;

    private Vector3 targetFloorPosition;

    public void Launch(Transform targetGrem)
    {
        
        targetFloorPosition = targetGrem.position;

        
        if (!isBlue && redHazardWarningPrefab != null)
        {
            GameObject warning = Instantiate(redHazardWarningPrefab, targetFloorPosition, Quaternion.identity);

            
            
        }

        
        transform.DOJump(targetFloorPosition, jumpPower, 1, travelTime)
            .SetEase(Ease.Linear)
            .OnComplete(ApplyImpact);
    }

    private void ApplyImpact()
    {
        
        float checkRadius = 1.2f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetFloorPosition, checkRadius);

        foreach (var hit in hits)
        {
            Gremurin grem = hit.GetComponentInParent<Gremurin>() ?? hit.GetComponent<Gremurin>();
            if (grem == null || grem.isDead) continue;

            if (isBlue)
            {
                
                grem.moveSpeed *= 1.5f;
                if (blueBuffVfxPrefab != null) Instantiate(blueBuffVfxPrefab, grem.transform.position, Quaternion.identity);
            }
            else
            {
                
                
                if (!grem.isPickedUp)
                {
                    grem.TakeDamage(20f); 
                    if (!grem.gameObject.GetComponent<RedInkHazard>())
                    {
                        grem.gameObject.AddComponent<RedInkHazard>();
                    }
                }
            }
        }

        
        Destroy(gameObject);
    }
}