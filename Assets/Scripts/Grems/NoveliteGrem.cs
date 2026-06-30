using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class NoveliteGrem : Gremurin
{
    [Header("Novelite Settings")]
    public int currentStacks = 0;
    public float timeToRead = 2.0f;
    public float projectileCooldown = 4.0f;
    public float spamWindow = 3.0f;
    public int requiredClicks = 10;

    [Header("Combat/Effects")]
    public GameObject blueProjectilePrefab;
    public GameObject redProjectilePrefab;
    public float explosionRadius = 3.5f;
    public float explosionDamage = 50f;
    public float explosionDelay = 1.5f;

    private float stateTimer;
    private bool isSpamWindowOpen = false;
    private int currentClickCount = 0;
    private float spamTimer;

    protected override void HandleWander()
    {
        if (currentHunger < data.maxHunger * seekFoodHungerThreshold)
        {
            SeekFood();
            return;
        }

        if (currentStacks < 3)
        {
            // State: Reading / Stacking
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
            {
                PerformRead();
            }
            else { base.HandleWander(); } // Only wander if not "reading"
        }
        else if (isSpamWindowOpen)
        {
            // State: Waiting for player input
            HandleSpamWindow();
        }
    }

    private void PerformRead()
    {
        // Stop moving to read
        isMoving = false;
        transform.DOShakeScale(timeToRead, 0.1f);

        // After reading, increment stack
        currentStacks++;
        stateTimer = (currentStacks == 3) ? 0 : projectileCooldown / (currentStacks * 0.5f);

        if (currentStacks == 3) StartSpamWindow();
    }

    private void StartSpamWindow()
    {
        isSpamWindowOpen = true;
        spamTimer = spamWindow;
        currentClickCount = 0;
        // Visual indicator (e.g., tint red or pulse)
        sr.DOColor(Color.magenta, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    private void HandleSpamWindow()
    {
        spamTimer -= Time.deltaTime;
        if (spamTimer <= 0)
        {
            isSpamWindowOpen = false;
            sr.DOKill();
            sr.color = Color.white;
            currentStacks = 0; // Reset
        }
    }

    // Call this via an EventTrigger or standard Unity Input
    private void OnMouseDown()
    {
        if (isSpamWindowOpen)
        {
            currentClickCount++;
            transform.DOPunchScale(Vector3.one * 0.1f, 0.1f);

            // --- PLAY SPAM CLICK VARIATION SOUND ---
            data.audioPack?.PlayAttack();

            if (currentClickCount >= requiredClicks) TriggerAttack();
        }
    }

    private void TriggerAttack()
    {
        isSpamWindowOpen = false;
        sr.DOKill();
        sr.DOColor(Color.black, explosionDelay);

        // Attack Delay
        DOVirtual.DelayedCall(explosionDelay, () => {
            // Explosion Logic
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            // --- PLAY MASSIVE SPECIAL EXPLOSION SOUND ---
            data.audioPack?.PlaySpecial();

            foreach (var hit in hits)
            {
                if (hit.gameObject == this.gameObject) continue;

                // Check if it's a friendly unit
                Gremurin grem = hit.GetComponentInParent<Gremurin>() ?? hit.GetComponent<Gremurin>();
                if (grem != null)
                {
                    grem.TakeDamage(explosionDamage);
                    continue;
                }

                // Check if it's an enemy unit
                Enemy enemy = hit.GetComponentInParent<Enemy>() ?? hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage);
                }
            }
            // Reset
            currentStacks = 0;
            sr.color = Color.white;
        });
    }

    // Logic for firing projectiles (called in an Update loop or Cooldown timer)
    private void FireProjectile()
    {
        if (currentStacks == 0 || currentStacks == 3) return;

        // Find random ally
        Gremurin[] allies = FindObjectsByType<Gremurin>(FindObjectsSortMode.None);
        List<Gremurin> validTargets = new List<Gremurin>();
        foreach (var a in allies) if (a != this && !a.isDead) validTargets.Add(a);

        if (validTargets.Count > 0)
        {
            GameObject prefab = (UnityEngine.Random.value > 0.5f) ? blueProjectilePrefab : redProjectilePrefab;
            GameObject proj = Instantiate(prefab, transform.position, Quaternion.identity);
            proj.GetComponent<NoveliteGremProjectile>().Launch(validTargets[UnityEngine.Random.Range(0, validTargets.Count)].transform);
        }
    }
}