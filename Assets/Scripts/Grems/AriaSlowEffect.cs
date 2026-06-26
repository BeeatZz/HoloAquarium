using UnityEngine;

public class AriaSlowEffect : MonoBehaviour
{
    private Enemy targetEnemy;
    private float originalSpeed;
    private float slowFactor;
    private float overlapCheckRadius = 4.0f; // Matches the Grem's ariaRadius
    private float lifeTimer = 0.2f; // Quick decay if not refreshed

    public void Initialize(Enemy enemy, float reduction)
    {
        targetEnemy = enemy;
        originalSpeed = enemy.moveSpeed;
        slowFactor = reduction;

        // Apply the slow debuff immediately
        targetEnemy.moveSpeed = originalSpeed * (1f - slowFactor);
    }

    private void Update()
    {
        // Keep refreshing as long as a Jailbird Grem is nearby. 
        // If the Grem stops singing or the enemy walks out, this timer ticks down.
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f || targetEnemy == null)
        {
            CleanUp();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Refresh the debuff duration if still touching the zone
        if (other.GetComponent<JailbirdGrem>())
        {
            lifeTimer = 0.2f;
        }
    }

    private void CleanUp()
    {
        if (targetEnemy != null)
        {
            targetEnemy.moveSpeed = originalSpeed; // Restore normal speed
        }
        Destroy(this);
    }
}