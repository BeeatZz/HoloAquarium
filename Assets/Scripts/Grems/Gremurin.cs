using System;
using UnityEngine;

public class Gremurin : MonoBehaviour
{
    private SpriteRenderer sr;
    [Header("Stats")]
    public GremData data;

    [Header("Runtime State")]
    public float currentHunger;
    public float currentHealth;
    public bool isDead;

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

    // Internal
    private Vector3 basePosition;
    private Vector3 targetPosition;
    private float wanderTimer;
    private bool isMoving;
    private FoodItem targetFood;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (data == null)
        {
            Debug.LogError($"Gremurin {gameObject.name} has no GremData assigned.");
            return;
        }

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

    private void Update()
    {
        if (isDead) return;

        HandleHunger();
        HandleWander();
        HandleIdleBob();
    }

    private void UpdateFacing(Vector3 targetPos)
    {
        if (sr == null) return;
        float diff = targetPos.x - transform.position.x;
        if (Mathf.Abs(diff) > 0.01f)
            sr.flipX = diff < 0;
    }
    private void HandleHunger()
    {
        currentHunger -= data.hungerRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, data.maxHunger);
    }

    private void HandleWander()
    {
        if (currentHunger < data.maxHunger * seekFoodHungerThreshold)
        {
            SeekFood();
            return;
        }

        targetFood = null;

        if (isMoving)
        {
            UpdateFacing(targetPosition);
            Vector3 newPos = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            transform.position = LevelManager.Instance.ClampToPlayArea(newPos);

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
            {
                PickWanderTarget();
            }
        }
    }

    private void SeekFood()
    {
        if (targetFood == null)
            targetFood = FindNearestFood();

        if (targetFood == null) return;

        isMoving = true;
        UpdateFacing(targetFood.transform.position);
        Vector3 newPos = Vector3.MoveTowards(
            transform.position,
            targetFood.transform.position,
            moveSpeed * 1.5f * Time.deltaTime
        );
        transform.position = LevelManager.Instance.ClampToPlayArea(newPos);
        basePosition = transform.position;
    }

    private FoodItem FindNearestFood()
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

    private void HandleIdleBob()
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

    private void PickWanderTarget()
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * wanderRadius;
        Vector3 candidate = basePosition + new Vector3(randomOffset.x, randomOffset.y, 0);
        targetPosition = LevelManager.Instance.ClampToPlayArea(candidate);
        isMoving = true;
    }
    public void SetBasePosition(Vector3 newBase)
    {
        basePosition = newBase;
        targetPosition = newBase;
        isMoving = false;
    }

    private void ScheduleNextWander()
    {
        wanderTimer = UnityEngine.Random.Range(wanderPauseMin, wanderPauseMax);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Feed(float amount)
    {
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, data.maxHunger);
    }

    private void Die()
    {
        isDead = true;

        if (LevelManager.Instance != null)
            LevelManager.Instance.RegisterGremDeath();

        // TODO: play death animation then destroy
        Destroy(gameObject);
    }
}