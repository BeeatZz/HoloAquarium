using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class HubGremurin : MonoBehaviour
{
    [Header("Data")]
    private GremData _data;
    public GremData data
    {
        get => _data;
        set
        {
            _data = value;
            UpdateGremVisuals();
        }
    }

    [Header("Wander Settings")]
    public float wanderRadius = 6f;
    public float wanderPauseMin = 2f;
    public float wanderPauseMax = 5f;

    [Header("Idle Bob")]
    public bool bobOnlyWhenIdle = true; // Set false if you want them to bounce while walking too!
    public float bobSpeed = 2f;
    public float bobAmplitude = 0.05f;

    private NavMeshAgent agent;
    private float wanderTimer;
    private bool isWaiting;
    private SpriteRenderer spriteRenderer;
    private Transform cameraTransform;

    // Tracks the sprite's starting local position so it doesn't drift away
    private Vector3 initialSpriteLocalPos;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Cache the original local position of the sprite child
        if (spriteRenderer != null)
        {
            initialSpriteLocalPos = spriteRenderer.transform.localPosition;
        }
    }

    private void Start()
    {
        UpdateGremVisuals();
        ScheduleNextMove();
    }

    private void Update()
    {
        if (isWaiting)
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
                PickNewDestination();
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            isWaiting = true;
            ScheduleNextMove();
        }

        HandleIdleBob();
    }

    private void LateUpdate()
    {
        BillboardToCamera();
    }

    private void HandleIdleBob()
    {
        if (spriteRenderer == null) return;

        // Bob if we are idling, OR if bobOnlyWhenIdle is turned off
        if (isWaiting || !bobOnlyWhenIdle)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            spriteRenderer.transform.localPosition = new Vector3(
                initialSpriteLocalPos.x,
                initialSpriteLocalPos.y + bob,
                initialSpriteLocalPos.z
            );
        }
        else
        {
            // Instantly snap back to standard height when walking
            spriteRenderer.transform.localPosition = initialSpriteLocalPos;
        }
    }

    private void UpdateGremVisuals()
    {
        if (_data != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = _data.sprite;
        }
    }

    private void BillboardToCamera()
    {
        if (spriteRenderer == null || cameraTransform == null) return;

        // Remember current local position (which now includes our bobbing math)
        Vector3 localPos = spriteRenderer.transform.localPosition;

        spriteRenderer.transform.rotation = cameraTransform.rotation;

        // Reapply local position so billboarding doesn't override the bobbing
        spriteRenderer.transform.localPosition = localPos;
    }

    private void ScheduleNextMove()
    {
        wanderTimer = UnityEngine.Random.Range(wanderPauseMin, wanderPauseMax);
    }

    private void PickNewDestination()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            isWaiting = false;
        }
        else
        {
            ScheduleNextMove();
        }
    }
}