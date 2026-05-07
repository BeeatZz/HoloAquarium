using UnityEngine;
using DG.Tweening;

public class PeroEnemy : Enemy
{
    [Header("Pero Movement")]
    public float dashSpeedMultiplier = 2.5f;
    public float patternChangeInterval = 1.5f;
    public float zigZagIntensity = 5f;

    private float patternTimer;
    private Vector3 randomOffset;

    protected override void Start()
    {
        base.Start(); // Sets up Visual child and spawn animation
        patternTimer = patternChangeInterval;
    }

    protected override void Think()
    {
        // Challenge tracking: Move in crazy patterns
        HandleErraticMovement();
        
        base.Think(); // Handles FindTarget, MoveTowardTarget, and TryAttack
    }

    protected override void FindTarget()
    {
        // Target a specific role: Specialist grems first
        Gremurin[] allGrems = FindObjectsByType<Gremurin>(FindObjectsSortMode.None);
        Gremurin priorityTarget = null;
        float minDistance = float.MaxValue;

        // Pass 1: Look for Specialists
        foreach (Gremurin grem in allGrems)
        {
            if (grem.data != null && grem.data.gremRole == GremRole.Specialist)
            {
                float dist = Vector3.Distance(transform.position, grem.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    priorityTarget = grem;
                }
            }
        }

        // Pass 2: Fallback to nearest if no specialists exist
        if (priorityTarget != null)
        {
            currentTarget = priorityTarget;
        }
        else
        {
            base.FindTarget(); 
        }
    }

    private void HandleErraticMovement()
    {
        patternTimer -= Time.deltaTime;
        if (patternTimer <= 0)
        {
            // Create a "crazy" zigzag offset
            randomOffset = Random.insideUnitSphere * zigZagIntensity;
            randomOffset.z = 0;
            patternTimer = patternChangeInterval;
        }
    }

    protected override void MoveTowardTarget()
    {
        if (currentTarget == null) return;

        // Challenge tracking: fast movement with zigzagging
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        Vector3 erraticPath = direction + (randomOffset * 0.5f);
        
        float speed = moveSpeed * dashSpeedMultiplier;
        transform.position += erraticPath.normalized * speed * Time.deltaTime;

        // Ensure Pero stays in the box
        transform.position = LevelManager.Instance.ClampToPlayArea(transform.position);

        UpdateFacing(currentTarget.transform.position); //
    }

    public override void OnPlayerPunch(float damage)
    {
        // Challenge tracking: DASH away when punched to be harder to click twice
        base.OnPlayerPunch(damage); // Visual punch scale
        
        Vector2 escapeDir = Random.insideUnitCircle.normalized * 2f;
        transform.DOMove(transform.position + (Vector3)escapeDir, 0.3f)
            .SetEase(Ease.OutExpo)
            .OnUpdate(() => {
                transform.position = LevelManager.Instance.ClampToPlayArea(transform.position);
            });
    }
}