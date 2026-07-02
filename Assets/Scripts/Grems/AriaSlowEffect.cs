using UnityEngine;

public class AriaSlowEffect : MonoBehaviour
{
    private Enemy targetEnemy;
    private float originalSpeed;
    private float slowFactor;
    private float overlapCheckRadius = 4.0f; 
    private float lifeTimer = 0.2f; 

    public void Initialize(Enemy enemy, float reduction)
    {
        targetEnemy = enemy;
        originalSpeed = enemy.moveSpeed;
        slowFactor = reduction;

        
        targetEnemy.moveSpeed = originalSpeed * (1f - slowFactor);
    }

    private void Update()
    {
        
        
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f || targetEnemy == null)
        {
            CleanUp();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        
        if (other.GetComponent<JailbirdGrem>())
        {
            lifeTimer = 0.2f;
        }
    }

    private void CleanUp()
    {
        if (targetEnemy != null)
        {
            targetEnemy.moveSpeed = originalSpeed; 
        }
        Destroy(this);
    }
}