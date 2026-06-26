using UnityEngine;
using DG.Tweening;
using System;

public class PeroEnemy : Enemy
{
    [Header("Pero Movement")]
    public float dashSpeedMultiplier = 2.5f;
    public float patternChangeInterval = 1.5f;
    public float zigZagIntensity = 5f;

    private float patternTimer;
    private Vector3 randomOffset;
    private bool isKnockedBack; // Prevents animation/movement updates during punch hit

    protected override void Start()
    {
        base.Start();
        patternTimer = patternChangeInterval;
    }

    protected override void Think()
    {
        if (isKnockedBack) return; // Don't process normal pathing while flying backward

        HandleErraticMovement();
        base.Think();
    }

    protected override void FindTarget()
    {
        Gremurin[] allGrems = FindObjectsByType<Gremurin>(FindObjectsSortMode.None);
        Gremurin priorityTarget = null;
        float minDistance = float.MaxValue;

        foreach (Gremurin grem in allGrems)
        {
            if (grem.isDead) continue;
            if (grem.data != null && grem.data.role == GremRole.Specialist)
            {
                float dist = Vector3.Distance(transform.position, grem.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    priorityTarget = grem;
                }
            }
        }

        if (priorityTarget != null)
            targetGrem = priorityTarget;
        else
            base.FindTarget();
    }

    private void HandleErraticMovement()
    {
        patternTimer -= Time.deltaTime;
        if (patternTimer <= 0)
        {
            randomOffset = UnityEngine.Random.insideUnitSphere * zigZagIntensity;
            randomOffset.z = 0;
            patternTimer = patternChangeInterval;
        }
    }

    protected override void MoveTowardTarget()
    {
        if (targetGrem == null)
        {
            // Tell the base animator to stop moving if there are no targets
            UpdateMovingAnimation(false);
            return;
        }

        Vector3 direction = (targetGrem.transform.position - transform.position).normalized;
        Vector3 erraticPath = direction + (randomOffset * 0.5f);

        float speed = moveSpeed * dashSpeedMultiplier;
        transform.position += erraticPath.normalized * speed * Time.deltaTime;
        transform.position = LevelManager.Instance.ClampToPlayArea(transform.position);
        UpdateFacing(targetGrem.transform.position);

        // FIX: Explicitly turn on moving animation state
        UpdateMovingAnimation(true);
    }

    public override void OnPlayerPunch(float damage)
    {
        base.OnPlayerPunch(damage);

        isKnockedBack = true;
        UpdateMovingAnimation(false); // Stop running frames while flying back

        Vector2 escapeDir = UnityEngine.Random.insideUnitCircle.normalized * 2f;
        transform.DOMove(transform.position + (Vector3)escapeDir, 0.3f)
            .SetEase(Ease.OutExpo)
            .OnUpdate(() =>
            {
                transform.position = LevelManager.Instance.ClampToPlayArea(transform.position);
            })
            .OnComplete(() =>
            {
                isKnockedBack = false; // Resume normal hunting
            });
    }
}