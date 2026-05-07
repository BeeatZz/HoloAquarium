using System;
using UnityEngine;
using DG.Tweening;


public class Gremurin : MonoBehaviour
{
    [Header("Stats")]
    public GremData data;

    [Header("Separation")]
    public float separationRadius = 0.4f;
    public float separationStrength = 0.5f;

    [Header("Runtime State")]
    public float currentHunger;
    public float currentHealth;
    public bool isDead;
    public bool isPickedUp;

    [Header("Wander Settings")]
    public float wanderRadius = 1.5f;
    public float wanderPauseMin = 2f;
    public float wanderPauseMax = 5f;
    public float moveSpeed = 1.5f;

    [Header("Hunger Settings")]
    public float seekFoodHungerThreshold = 0.4f;

    [Header("Idle Bob")]
    public float bobSpeed = 2f;
    public float bobAmplitude = 0.05f;

    protected Vector3 basePosition;
    protected Vector3 targetPosition;
    protected float wanderTimer;
    protected bool isMoving;
    protected FoodItem targetFood;
    protected SpriteRenderer sr;

    protected virtual void Start()
    {
        if (data == null)
        {
            Debug.LogError($"Gremurin {gameObject.name} has no GremData assigned.");
            return;
        }

        sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.sprite != null)
            sr.sprite = data.sprite;

        basePosition = transform.position;
        targetPosition = transform.position;
        currentHunger = data.maxHunger;
        currentHealth = data.maxHealth;
        wanderRadius = data.wanderRadius;
        wanderPauseMin = data.wanderPauseMin;
        wanderPauseMax = data.wanderPauseMax;
        moveSpeed = data.moveSpeed;

        ScheduleNextWander();
    }

    protected virtual void Update()
    {
        if (isDead || isPickedUp) return;

        HandleHunger();
        HandleWander();
        HandleIdleBob();
    }

    protected virtual void HandleHunger()
    {
        currentHunger -= data.hungerRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, data.maxHunger);
    }

    protected virtual void HandleWander()
    {
        if (currentHunger < data.maxHunger * seekFoodHungerThreshold)
        {
            SeekFood();
            return;
        }

        targetFood = null;

        if (isMoving)
        {
            Vector3 newPos = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
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
            if (wanderTimer <= 0f)
                PickWanderTarget();
        }
    }

    protected virtual void HandleIdleBob()
    {
        if (!isMoving)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            Vector3 bobbedPos = new Vector3(
                basePosition.x,
                basePosition.y + bob,
                basePosition.z
            );
            transform.position = LevelManager.Instance.ClampToPlayArea(bobbedPos);
        }
    }

    protected virtual void SeekFood()
    {
        if (targetFood == null)
            targetFood = FindNearestFood();

        if (targetFood == null) return;

        isMoving = true;
        Vector3 newPos = Vector3.MoveTowards(
            transform.position,
            targetFood.transform.position,
            moveSpeed * 1.5f * Time.deltaTime
        );
        transform.position = LevelManager.Instance.ClampToPlayArea(newPos);
        UpdateFacing(targetFood.transform.position);
        basePosition = transform.position;
    }

    protected void UpdateFacing(Vector3 targetPos)
    {
        if (sr == null) return;
        float diff = targetPos.x - transform.position.x;
        if (Mathf.Abs(diff) > 0.01f)
            sr.flipX = diff < 0;
    }

    protected FoodItem FindNearestFood()
    {
        FoodItem[] foods = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        FoodItem nearest = null;
        float nearestDist = float.MaxValue;

        foreach (FoodItem food in foods)
        {
            float dist = Vector3.Distance(transform.position, food.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = food;
            }
        }

        return nearest;
    }

    protected void PickWanderTarget()
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * wanderRadius;
        Vector3 candidate = basePosition + new Vector3(randomOffset.x, randomOffset.y, 0);
        targetPosition = LevelManager.Instance.ClampToPlayArea(candidate);
        isMoving = true;
    }

    protected void ScheduleNextWander()
    {
        wanderTimer = UnityEngine.Random.Range(wanderPauseMin, wanderPauseMax);
    }

    public void SetBasePosition(Vector3 newBase)
    {
        basePosition = newBase;
        targetPosition = newBase;
        isMoving = false;
    }

    public void OnPickedUp()
    {
        isPickedUp = true;
        isMoving = false;
    }

    public void OnReleased()
    {
        isPickedUp = false;
        ScheduleNextWander();
    }

    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;

        if (sr != null)
        {
            sr.DOKill();
            sr.DOColor(Color.red, 0.05f).SetEase(Ease.OutQuad)
                .OnComplete(() => sr.DOColor(Color.white, 0.15f));
        }

        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 5, 0.5f);
        transform.DOShakePosition(0.2f, 0.08f, 15, 90f);

        if (currentHealth <= 0)
            Die();
    }

    public virtual void Feed(float amount)
    {
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, data.maxHunger);
    }

    protected virtual void Die()
    {
        isDead = true;

        if (LevelManager.Instance != null)
            LevelManager.Instance.RegisterGremDeath();

        Destroy(gameObject);
    }
}