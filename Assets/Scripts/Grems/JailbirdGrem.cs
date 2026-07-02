using UnityEngine;

public class JailbirdGrem : Gremurin
{
    [Header("Ravens Aria Settings")]
    public float ariaRadius = 4.0f;
    public float itemPullForce = 1.5f;

    [Range(0f, 1f)]
    public float enemySpeedReduction = 0.5f; 
    public float ariaDamagePerSecond = 10f;

    protected override void HandleWander()
    {
        
        if (currentHunger < data.maxHunger * seekFoodHungerThreshold)
        {
            SeekFood();
            return;
        }

        
        isMoving = false;
        HandleUniversalAria();
        HandleIdleBob();
    }

    private void HandleUniversalAria()
    {
        
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, ariaRadius);

        foreach (var obj in objects)
        {
            
            CurrencyDrop drop = obj.GetComponent<CurrencyDrop>();
            FoodItem food = obj.GetComponent<FoodItem>();

            if (drop != null || food != null)
            {
                obj.transform.position = Vector3.MoveTowards(
                    obj.transform.position,
                    transform.position,
                    itemPullForce * Time.deltaTime
                );
                continue; 
            }

            
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                
                enemy.TakeDamage(ariaDamagePerSecond * Time.deltaTime);

                
                
                if (!enemy.gameObject.GetComponent<AriaSlowEffect>())
                {
                    
                    var slowDebuff = enemy.gameObject.AddComponent<AriaSlowEffect>();
                    slowDebuff.Initialize(enemy, enemySpeedReduction);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.3f); 
        Gizmos.DrawWireSphere(transform.position, ariaRadius);
    }
}