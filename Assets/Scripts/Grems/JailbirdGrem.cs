using UnityEngine;
using DG.Tweening;

public class JailbirdGrem : Gremurin
{
    [Header("Ravens Aria Settings")]
    public float ariaRadius = 4.0f;
    public float itemPullForce = 1.5f;
    [Range(0f, 1f)]
    public float enemySpeedReduction = 0.5f;
    public float ariaDamagePerSecond = 10f;

    [Header("Visuals")]
    public Transform ariaVisual; // Drag the child object with the Sprite Renderer here

    protected override void Start()
    {
        base.Start();

        // Initialize visual size
        UpdateVisualScale();

        // Start the pulse animation
        // We pulse between 95% and 100% of the radius size for a subtle breathing effect
        if (ariaVisual != null)
        {
            ariaVisual.DOScale(ariaVisual.localScale * 0.95f, 1.0f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }

    private void OnValidate()
    {
        // This allows you to see the circle size update in the editor while you move the slider
        UpdateVisualScale();
    }

    private void UpdateVisualScale()
    {
        if (ariaVisual != null)
        {
            // Diameter = Radius * 2
            float diameter = ariaRadius * 2f;
            ariaVisual.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

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

                AriaSlowEffect slowDebuff = enemy.GetComponent<AriaSlowEffect>();
                if (slowDebuff == null)
                {
                    slowDebuff = enemy.gameObject.AddComponent<AriaSlowEffect>();
                    slowDebuff.Initialize(enemy, enemySpeedReduction);
                }
                else
                {
                    slowDebuff.Refresh();
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