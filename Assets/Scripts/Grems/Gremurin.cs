using UnityEngine;

public class Gremurin : MonoBehaviour
{
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

    [Header("Idle Bob")]
    public float bobSpeed = 2f;
    public float bobAmplitude = 0.05f;

    // Internal
    private Vector3 basePosition;
    private Vector3 targetPosition;
    private float wanderTimer;
    private bool isMoving;

    private void Start()
    {
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

    private void HandleHunger()
    {
        currentHunger -= data.hungerRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, data.maxHunger);
    }

    private void HandleWander()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

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

    private void HandleIdleBob()
    {
        if (!isMoving)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = new Vector3(
                basePosition.x,
                basePosition.y + bob,
                basePosition.z
            );
        }
    }

    private void PickWanderTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        targetPosition = basePosition + new Vector3(randomOffset.x, randomOffset.y, 0);
        isMoving = true;
    }

    private void ScheduleNextWander()
    {
        wanderTimer = Random.Range(wanderPauseMin, wanderPauseMax);
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