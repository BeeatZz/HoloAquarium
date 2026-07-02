using UnityEngine;

public class AriaSlowEffect : MonoBehaviour
{
    private Enemy targetEnemy;
    private float originalSpeed;
    private float slowFactor;
    private bool isInitialized = false;
    private float lifeTimer = 0.2f;

    public void Initialize(Enemy enemy, float reduction)
    {
        // Only run initialization logic once
        if (isInitialized) return;

        targetEnemy = enemy;
        originalSpeed = enemy.moveSpeed;
        slowFactor = reduction;

        // Apply the slow
        targetEnemy.moveSpeed = originalSpeed * (1f - slowFactor);
        isInitialized = true;
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f || targetEnemy == null)
        {
            CleanUp();
        }
    }

    // Called by JailbirdGrem to refresh the timer
    public void Refresh()
    {
        lifeTimer = 0.2f;
    }

    private void CleanUp()
    {
        if (targetEnemy != null)
        {
            targetEnemy.moveSpeed = originalSpeed; // Restore original speed
        }
        Destroy(this);
    }
}