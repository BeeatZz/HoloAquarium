using UnityEngine;

public class JailbirdGrem : Gremurin
{
    [Header("Ravens Aria Settings")]
    public float ariaRadius = 4.0f;
    public float itemPullForce = 1.5f;

    [Range(0f, 1f)]
    public float enemySpeedReduction = 0.5f; // Slows enemies down by 50%
    public float ariaDamagePerSecond = 10f;

    protected override void HandleWander()
    {
        // Survival first: if starving, go get food
        if (currentHunger < data.maxHunger * seekFoodHungerThreshold)
        {
            SeekFood();
            return;
        }

        // Otherwise, she anchors down, sings, and plays her idle bobbing animation
        isMoving = false;
        HandleUniversalAria();
        HandleIdleBob();
    }

    private void HandleUniversalAria()
    {
        // Find everything within the song's radius
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, ariaRadius);

        foreach (var obj in objects)
        {
            // 1. Magnetize Items (Food & Currency Drops)
            CurrencyDrop drop = obj.GetComponent<CurrencyDrop>();
            FoodItem food = obj.GetComponent<FoodItem>();

            if (drop != null || food != null)
            {
                obj.transform.position = Vector3.MoveTowards(
                    obj.transform.position,
                    transform.position,
                    itemPullForce * Time.deltaTime
                );
                continue; // Move to next object
            }

            // 2. Combat: Catch ANY class that inherits from your base "Enemy" script
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Apply constant damage over time while they are in the radius
                enemy.TakeDamage(ariaDamagePerSecond * Time.deltaTime);

                // Dynamically suppress their speed while inside the zone
                // This will slow down regular walk speeds AND sudden enrage/swoop speed spikes
                if (!enemy.gameObject.GetComponent<AriaSlowEffect>())
                {
                    // Attach a temporary component to manage their speed state cleanly
                    var slowDebuff = enemy.gameObject.AddComponent<AriaSlowEffect>();
                    slowDebuff.Initialize(enemy, enemySpeedReduction);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.3f); // Translucent Purple
        Gizmos.DrawWireSphere(transform.position, ariaRadius);
    }
}