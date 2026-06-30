using System;
using UnityEngine;
using DG.Tweening;

public class Gremurin : MonoBehaviour
{
    [Header("Stats")]
    public GremData data;

    [Header("Runtime State")]
    public float currentHunger;
    public float currentHealth;
    public bool isDead;
    public bool isPickedUp;

    [Header("Movement")]
    public float wanderRadius = 1.5f;
    public float wanderPauseMin = 2f;
    public float wanderPauseMax = 5f;
    public float moveSpeed = 1.5f;
    public float seekFoodHungerThreshold = 0.4f;

    [Header("Idle Bob")]
    public float bobSpeed = 2f;
    public float bobAmplitude = 0.05f;

    [Header("Nerissa Mechanics")]
    public bool isCharmed = false;
    public int clicksToWakeUp = 5;
    public float lastUncharmedTime = -10f;
    public float damageCooldown = 0.5f;
    private int currentClicks = 0;
    private Gremurin charmTarget;
    private float lastDamageTime;

    protected Vector3 basePosition;
    protected Vector3 targetPosition;
    protected float wanderTimer;
    protected bool isMoving;
    protected FoodItem targetFood;
    protected SpriteRenderer sr;

    // Animation reference
    protected Animator animator;

    // Dynamic virtual property so DropSpawner can grab dynamic rates from subclasses
    public virtual float CurrentCurrencyOutputRate
    {
        get { return data != null ? data.currencyOutputRate : 15f; }
    }

    protected virtual void Start()
    {
        if (data == null) return;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.sprite != null) sr.sprite = data.sprite;

        // Looks for the Animator on this GameObject or its visual child
        animator = GetComponentInChildren<Animator>();

        basePosition = transform.position;
        targetPosition = transform.position;
        currentHunger = data.maxHunger;
        currentHealth = data.maxHealth;
        wanderRadius = data.wanderRadius;
        moveSpeed = data.moveSpeed;

        ScheduleNextWander();

        // --- PLAY SPAWN SOUND ---
        data.audioPack?.PlaySpawn();
    }

    protected virtual void Update()
    {
        if (isDead || isPickedUp) return;

        if (isCharmed)
        {
            HandleCharmBehavior();
            UpdateAnimationStates(); // Update layout while charmed
            return;
        }

        // Clean, flattened update cycle execution
        HandleHunger();
        HandleWander();
        HandleIdleBob();

        // Push movement and state data to the animator every frame
        UpdateAnimationStates();
    }

    protected virtual void HandleHunger()
    {
        currentHunger -= data.hungerRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, data.maxHunger);
    }

    protected virtual void HandleWander()
    {
        if (currentHunger < data.maxHunger * seekFoodHungerThreshold) { SeekFood(); return; }
        targetFood = null;

        if (isMoving)
        {
            Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.position = LevelManager.Instance.ClampToPlayArea(newPos);
            UpdateFacing(targetPosition);

            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                basePosition = targetPosition;
                isMoving = false;
                ScheduleNextWander();
            }
        }
        else
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f) PickWanderTarget();
        }
    }

    protected virtual void HandleIdleBob()
    {
        if (!isMoving)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = LevelManager.Instance.ClampToPlayArea(new Vector3(basePosition.x, basePosition.y + bob, basePosition.z));
        }
    }

    protected virtual void HandleCharmBehavior()
    {
        if (charmTarget == null || charmTarget.isDead) charmTarget = FindNearestOtherGrem();
        if (charmTarget != null)
        {
            isMoving = true;

            // Charmed movement speed adjustment preserved
            Vector3 newPos = Vector3.MoveTowards(transform.position, charmTarget.transform.position, (moveSpeed * 0.5f) * Time.deltaTime);
            transform.position = LevelManager.Instance.ClampToPlayArea(newPos);
            UpdateFacing(charmTarget.transform.position);

            if (Vector3.Distance(transform.position, charmTarget.transform.position) < 0.4f)
            {
                charmTarget.TakeDamage(2f);
                if (animator != null) animator.SetTrigger("Attack");

                // --- PLAY CHARM ATTACK CRY ---
                data.audioPack?.PlayAttack();
            }
        }
        else
        {
            isMoving = false;
        }
    }

    public void BeCharmed(Color charmColor)
    {
        if (isCharmed || IsInsideBarrier()) return;
        isCharmed = true;
        currentClicks = 0;
        sr.DOColor(charmColor, 0.5f);
    }

    public void OnSpamClicked()
    {
        if (!isCharmed) return;
        currentClicks++;

        transform.DOKill();
        transform.DOScale(Vector3.one * 1.15f, 0.05f).OnComplete(() => transform.DOScale(Vector3.one, 0.05f));

        if (currentClicks >= clicksToWakeUp) WakeUp();
    }

    public void WakeUp()
    {
        isCharmed = false;
        lastUncharmedTime = Time.time;
        sr.DOColor(Color.white, 0.2f);

        transform.DOKill();
        transform.DOScale(Vector3.one * 1.5f, 0.1f).SetEase(Ease.OutBack).OnComplete(() => transform.DOScale(Vector3.one, 0.2f));

        basePosition = transform.position;
        ScheduleNextWander();
    }

    private bool IsInsideBarrier() => Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Barrier")) != null;

    public virtual void TakeDamage(float amount)
    {
        if (isDead || Time.time < lastDamageTime + damageCooldown) return;
        lastDamageTime = Time.time;
        currentHealth -= amount;

        // --- PLAY HURT SOUND ---
        data.audioPack?.PlayHit();

        if (sr != null)
        {
            sr.DOKill();
            Color ret = isCharmed ? new Color(0.7f, 0.4f, 1f) : Color.white;
            sr.DOColor(Color.red, 0.05f).OnComplete(() => sr.DOColor(ret, 0.15f));
        }

        transform.DOKill();
        transform.DOScale(Vector3.one * 1.25f, 0.05f).OnComplete(() => transform.DOScale(Vector3.one, 0.1f));

        if (currentHealth <= 0) Die();
    }

    private Gremurin FindNearestOtherGrem()
    {
        Gremurin[] all = UnityEngine.Object.FindObjectsByType<Gremurin>(FindObjectsSortMode.None);
        Gremurin closest = null; float minDist = float.MaxValue;
        foreach (var g in all)
        {
            if (g == this || g.isDead) continue;
            float d = Vector3.Distance(transform.position, g.transform.position);
            if (d < minDist) { minDist = d; closest = g; }
        }
        return closest;
    }

    protected virtual void SeekFood()
    {
        if (targetFood == null) targetFood = FindNearestFood();
        if (targetFood == null)
        {
            isMoving = false;
            return;
        }
        isMoving = true;
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetFood.transform.position, moveSpeed * 1.5f * Time.deltaTime);
        transform.position = LevelManager.Instance.ClampToPlayArea(newPos);
        UpdateFacing(targetFood.transform.position);
        basePosition = transform.position;
    }

    protected void UpdateFacing(Vector3 targetPos) { if (sr == null) return; sr.flipX = (targetPos.x - transform.position.x) < 0; }

    protected FoodItem FindNearestFood()
    {
        FoodItem[] foods = UnityEngine.Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        FoodItem nearest = null; float nDist = float.MaxValue;
        foreach (var f in foods) { float d = Vector3.Distance(transform.position, f.transform.position); if (d < nDist) { nDist = d; nearest = f; } }
        return nearest;
    }

    protected void PickWanderTarget()
    {
        Vector2 rand = UnityEngine.Random.insideUnitCircle * wanderRadius;
        targetPosition = LevelManager.Instance.ClampToPlayArea(basePosition + (Vector3)rand);
        isMoving = true;
    }

    protected void ScheduleNextWander() => wanderTimer = UnityEngine.Random.Range(wanderPauseMin, wanderPauseMax);

    protected void UpdateAnimationStates()
    {
        if (animator == null) return;

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsCharmed", isCharmed);
        animator.SetBool("IsPickedUp", isPickedUp);
    }

    public void SetBasePosition(Vector3 nb) { basePosition = nb; targetPosition = nb; isMoving = false; }

    public void OnPickedUp()
    {
        isPickedUp = true;
        isMoving = false;
        UpdateAnimationStates();
    }

    public void OnReleased()
    {
        isPickedUp = false;
        UpdateAnimationStates();
        ScheduleNextWander();
    }

    public virtual void Feed(float amt)
    {
        currentHunger = Mathf.Clamp(currentHunger + amt, 0, data.maxHunger);

        // --- PLAY EAT SOUND PACK ---
        data.audioPack?.PlayHit(); // Or add a specialized audio method if needed!
    }

    protected virtual void Die()
    {
        isDead = true;

        // --- PLAY DEATH CRY ---
        data.audioPack?.PlayDeath();

        if (animator != null) animator.SetBool("IsMoving", false);
        if (LevelManager.Instance != null) LevelManager.Instance.RegisterGremDeath();
        UnityEngine.Object.Destroy(gameObject);
    }
}