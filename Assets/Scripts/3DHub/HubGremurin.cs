using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;


using Random = UnityEngine.Random;
using Pointer = UnityEngine.InputSystem.Pointer;

[RequireComponent(typeof(NavMeshAgent))]
public class HubGremurin : MonoBehaviour
{
    private enum GremState { Wander, Held, Charging, Thrown, Landing, Vibing, Sleeping }
    private GremState state = GremState.Wander;

    [Header("Data")]
    [SerializeField] private GremData _data;
    public GremData data
    {
        get => _data;
        set
        {
            _data = value;
            UpdateGremVisuals();
            ApplyDataStats();
            ApplyOverrideAnimations();
        }
    }

    [Header("Wander Settings")]
    public float wanderRadius = 6f;
    public float wanderPauseMin = 2f;
    public float wanderPauseMax = 5f;
    public float tankMaxRadius = 10f;

    [Header("Idle Bob")]
    public bool bobOnlyWhenIdle = true;
    public float bobSpeed = 2f;
    public float bobAmplitude = 0.05f;
    public float bobSpeedVariance = 0.4f;
    public float bobAmplitudeVariance = 0.02f;
    public float bobTimeOffsetMax = 10f;

    [Header("Idle Stretch (rare variation)")]
    public float stretchCheckInterval = 6f;
    [Range(0f, 1f)] public float stretchChance = 0.15f;
    public float stretchAmount = 0.15f;
    public float stretchDuration = 0.4f;

    [Header("Pickup / Charge / Throw")]
    public float dragHeight = 1.5f;
    public float minThrowSpeed = 2f;
    public float maxThrowSpeed = 14f;
    public float timeToMaxCharge = 1.5f;
    public float gravity = 9.8f;
    public LayerMask groundLayer;
    public float pickupLeniencyRadius = 0.4f;
    public float throwCollisionRadius = 0.3f;

    [Header("Throw Intensity Visuals")]
    public float maxSpinSpeed = 720f;
    public float minSquashAmount = 0.15f;
    public float maxSquashAmount = 0.6f;
    public float maxChargeCrouch = 0.3f;
    public float squashDuration = 0.25f;

    [Header("Trajectory Preview")]
    public int trajectoryPoints = 20;
    public float trajectoryTimeStep = 0.05f;
    public Material trajectoryMaterial;
    public float trajectoryWidth = 0.05f;
    public Color trajectoryColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("Landing Particles (dust puff)")]
    public int landingParticleCount = 10;
    public float landingParticleSpeed = 1.2f;
    public float landingParticleLifetime = 0.6f;
    public float landingParticleSize = 0.06f;
    public Color landingParticleColor = new Color(0.75f, 0.68f, 0.55f, 0.6f);

    [Header("Vibing (speaker proximity)")]
    public float hopBeatSubdivision = 1f;
    public float vibeHopAmplitude = 0.15f;
    public float vibeSleepinessRelief = 0.05f;
    [SerializeField] private bool nearSpeaker;
    [SerializeField] private bool likesCurrentSong;
    private bool favoriteDiscovered;
    private float currentBpm = 120f;

    [Header("Sleepiness")]
    public float sleepyRate = 0.02f;
    public float sleepThreshold = 1f;
    public float sleepDuration = 8f;
    public float sleepBobSpeed = 0.6f;
    public float sleepBobAmplitude = 0.02f;
    private float sleepiness;
    private bool headingHome;
    private float sleepTimer;

    [Header("Scene Light References (Auto-Assigned at Start)")]
    public List<Light> roomMainLights = new List<Light>();
    public List<Light> stageSpotlights = new List<Light>();

    private NavMeshAgent agent;
    private float wanderTimer;
    private bool isWaiting;
    private SpriteRenderer spriteRenderer;
    private Transform cameraTransform;
    private Vector3 initialSpriteLocalPos;
    private Vector3 initialSpriteLocalScale;
    private Vector3 homePosition;
    private Vector3 defaultScale;

    private float instanceBobSpeed;
    private float instanceBobAmplitude;
    private float instanceTimeOffset;
    private float stretchTimer;
    private bool isStretching;

    private Camera cam;
    private float chargeTime;
    private float throwIntensity;
    private Vector3 aimPoint;
    private Vector3 throwVelocity;
    private float currentSpin;

    private LineRenderer trajectoryLine;
    private ParticleSystem landingParticles;
    private Coroutine secretShowCoroutine;
    private float initialRoomLightIntensity;

    
    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        cam = Camera.main;
        defaultScale = transform.localScale;
        if (cam != null) cameraTransform = cam.transform;

        if (spriteRenderer != null)
        {
            initialSpriteLocalPos = spriteRenderer.transform.localPosition;
            initialSpriteLocalScale = spriteRenderer.transform.localScale;
        }
        else
        {
            initialSpriteLocalScale = Vector3.one;
        }

        instanceBobSpeed = bobSpeed + Random.Range(-bobSpeedVariance, bobSpeedVariance);
        instanceBobAmplitude = bobAmplitude + Random.Range(-bobAmplitudeVariance, bobAmplitudeVariance);
        instanceTimeOffset = Random.Range(0f, bobTimeOffsetMax);
        stretchTimer = stretchCheckInterval;

        SetupTrajectoryLine();
        SetupLandingParticles();

        

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ApplyDataStats()
    {
        if (_data == null)
        {
            Debug.LogWarning($"[{gameObject.name}] ApplyDataStats: GremData is NULL!", this);
            return;
        }

        wanderRadius = _data.wanderRadius;
        wanderPauseMin = _data.wanderPauseMin;
        wanderPauseMax = _data.wanderPauseMax;

        if (agent != null)
            agent.speed = _data.moveSpeed;
    }
    private void ApplyOverrideAnimations()
    {
        if (_data == null) return;

        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        
        if (animator != null && _data.hubAnimatorOverride != null)
        {
            animator.runtimeAnimatorController = _data.hubAnimatorOverride;
        }
        else if (animator == null)
        {
            Debug.LogError($"[{gameObject.name}] Failed to swap animations: No Animator component found in children!", this);
        }
    }
    private void OnEnable()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.OnTrackChanged += HandleTrackChanged;
            Debug.Log($"[{gameObject.name}] Subscribed to MusicManager.OnTrackChanged.", this);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        homePosition = transform.position;
        UpdateGremVisuals();
        ScheduleNextMove();

        
        roomMainLights.Clear();
        GameObject[] lightObjs = GameObject.FindGameObjectsWithTag("MainLight");
        foreach (GameObject obj in lightObjs)
        {
            Light l = obj.GetComponent<Light>();
            if (l != null) roomMainLights.Add(l);
        }

        
        stageSpotlights.Clear();
        GameObject[] spotObjects = GameObject.FindGameObjectsWithTag("StageSpotlight");
        foreach (GameObject obj in spotObjects)
        {
            Light l = obj.GetComponent<Light>();
            if (l != null) stageSpotlights.Add(l);
        }

        if (stageSpotlights.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] Runtime Spotlight Discovery failed: No objects found with tag 'StageSpotlight'!", this);
        }

        if (MusicManager.Instance != null && MusicManager.Instance.CurrentTrack != null)
        {
            HandleTrackChanged(MusicManager.Instance.CurrentTrack);
        }
    }

    private void HandleTrackChanged(TrackData track)
    {
        if (_data == null || track == null)
        {
            likesCurrentSong = false;
            return;
        }

        likesCurrentSong = (_data.favoriteTrack == track);
        currentBpm = Mathf.Max(1f, track.bpm);

        Debug.Log($"[{gameObject.name}] Song update: '{track.trackName}'. Likes it? {likesCurrentSong}", this);

        if (likesCurrentSong && nearSpeaker && (state == GremState.Wander || headingHome))
        {
            headingHome = false;
            agent.isStopped = true;
            state = GremState.Vibing;

            TriggerSecretShowIfEligible();
        }
    }

    private void Update()
    {
        switch (state)
        {
            case GremState.Wander:
                HandleWander();
                HandleIdleBob();
                CheckForPickup();
                break;

            case GremState.Held:
                HandleHeld();
                break;

            case GremState.Charging:
                HandleCharging();
                break;

            case GremState.Vibing:
                HandleVibeHop();
                CheckForPickup();
                break;

            case GremState.Sleeping:
                HandleSleeping();
                CheckForPickup();
                break;
        }

        UpdateAnimationStates();
    }

    private void LateUpdate()
    {
        BillboardToCamera();
    }

    private void OnDisable()
    {
        GrabManager.Release(this);
        if (trajectoryLine != null) trajectoryLine.enabled = false;

        if (MusicManager.Instance != null)
            MusicManager.Instance.OnTrackChanged -= HandleTrackChanged;
    }

    

    private void UpdateAnimationStates()
    {
        if (animator == null) return;

        
        bool isMoving = agent.enabled && agent.hasPath && agent.remainingDistance > agent.stoppingDistance;

        animator.SetBool("IsMoving", isMoving);
        
        if (isMoving)
        {
            Debug.Log($"[{gameObject.name}] script is sending IsMoving = TRUE to the animator.", this);
        }

        
        animator.SetBool("IsPickedUp", (state == GremState.Held || state == GremState.Charging));
        animator.SetBool("IsCharmed", false); 
    }

    

    private void HandleWander()
    {
        sleepiness += sleepyRate * Time.deltaTime;

        if (isWaiting)
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                if (sleepiness >= sleepThreshold)
                    GoHome();
                else
                    PickNewDestination();
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            if (headingHome)
                EnterSleep();
            else
            {
                isWaiting = true;
                ScheduleNextMove();
            }
        }
    }

    private void GoHome()
    {
        headingHome = true;
        agent.SetDestination(homePosition);
        isWaiting = false;
    }

    private void ScheduleNextMove()
    {
        wanderTimer = Random.Range(wanderPauseMin, wanderPauseMax);
    }

    private void PickNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        Vector3 candidate = transform.position + randomDirection;

        Vector3 offsetFromHome = candidate - homePosition;
        if (offsetFromHome.magnitude > tankMaxRadius)
        {
            offsetFromHome = offsetFromHome.normalized * tankMaxRadius;
            candidate = homePosition + offsetFromHome;
        }

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            isWaiting = false;
        }
        else
        {
            ScheduleNextMove();
        }
    }

    

    private void EnterSleep()
    {
        state = GremState.Sleeping;
        headingHome = false;
        sleepTimer = sleepDuration;
    }

    private void HandleSleeping()
    {
        sleepTimer -= Time.deltaTime;

        if (spriteRenderer != null)
        {
            float bob = Mathf.Sin(Time.time * sleepBobSpeed) * sleepBobAmplitude;
            spriteRenderer.transform.localPosition = initialSpriteLocalPos + new Vector3(0, bob, 0);
        }

        if (sleepTimer <= 0f)
            WakeUp();
    }

    private void WakeUp()
    {
        sleepiness = 0f;
        state = GremState.Wander;
        isWaiting = true;
        ScheduleNextMove();
    }

    

    private void HandleIdleBob()
    {
        if (spriteRenderer == null || isStretching) return;

        if (isWaiting || !bobOnlyWhenIdle)
        {
            float bob = Mathf.Sin((Time.time + instanceTimeOffset) * instanceBobSpeed) * instanceBobAmplitude;
            spriteRenderer.transform.localPosition = initialSpriteLocalPos + new Vector3(0, bob, 0);
            HandleStretchRoll();
        }
        else
        {
            spriteRenderer.transform.localPosition = initialSpriteLocalPos;
        }
    }

    private void HandleStretchRoll()
    {
        stretchTimer -= Time.deltaTime;
        if (stretchTimer <= 0f)
        {
            stretchTimer = stretchCheckInterval;
            if (Random.value < stretchChance)
                StartCoroutine(StretchPulse());
        }
    }

    private IEnumerator StretchPulse()
    {
        if (spriteRenderer == null) yield break;
        isStretching = true;
        Vector3 baseScale = initialSpriteLocalScale;
        float t = 0f;

        while (t < stretchDuration * 0.5f)
        {
            t += Time.deltaTime;
            float p = t / (stretchDuration * 0.5f);
            float stretch = Mathf.Lerp(1f, 1f + stretchAmount, p);
            float squash = Mathf.Lerp(1f, 1f - stretchAmount * 0.5f, p);
            spriteRenderer.transform.localScale = new Vector3(baseScale.x * squash, baseScale.y * stretch, baseScale.z * squash);
            yield return null;
        }

        t = 0f;
        while (t < stretchDuration * 0.5f)
        {
            t += Time.deltaTime;
            float p = t / (stretchDuration * 0.5f);

            spriteRenderer.transform.localScale = Vector3.Lerp(
                new Vector3(baseScale.x * (1f - stretchAmount * 0.5f), baseScale.y * (1f + stretchAmount), baseScale.z * (1f - stretchAmount * 0.5f)),
                baseScale, p);
            yield return null;
        }

        spriteRenderer.transform.localScale = initialSpriteLocalScale;
        isStretching = false;
    }

    

    private void CheckForPickup()
    {
        if (Pointer.current == null || cam == null) return;
        if (!Pointer.current.press.wasPressedThisFrame) return;
        if (GrabManager.CurrentlyHeld != null) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.SphereCast(ray, pickupLeniencyRadius, out RaycastHit hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore)
            && hit.collider.gameObject == gameObject)
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        if (!GrabManager.TryClaim(this)) return;

        Debug.Log($"[{gameObject.name}] Picked up! Interrupting current state.", this);
        state = GremState.Held;
        agent.enabled = false;
        transform.localScale = defaultScale;

        if (spriteRenderer != null)
            spriteRenderer.transform.localScale = initialSpriteLocalScale;

        sleepiness = 0f;
        headingHome = false;
    }

    private void HandleHeld()
    {
        Vector3 targetPos = GetHeldCarryPosition();
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 15f);

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            StartCharging();
    }

    private void StartCharging()
    {
        state = GremState.Charging;
        chargeTime = 0f;
        throwIntensity = 0f;
    }

    private void HandleCharging()
    {
        if (Pointer.current == null) return;

        if (Pointer.current.press.isPressed)
        {
            chargeTime += Time.deltaTime;
            throwIntensity = Mathf.Clamp01(chargeTime / timeToMaxCharge);

            Vector3 targetPos = GetHeldCarryPosition();
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 15f);

            if (spriteRenderer != null)
            {
                float crouch = throwIntensity * maxChargeCrouch;
                spriteRenderer.transform.localScale = new Vector3(
                    initialSpriteLocalScale.x * (1f + crouch * 0.5f),
                    initialSpriteLocalScale.y * (1f - crouch),
                    initialSpriteLocalScale.z * (1f + crouch * 0.5f)
                );
            }

            UpdateTrajectoryPreview();
        }
        else
        {
            ReleaseThrow();
        }
    }

    private Vector3 GetHeldCarryPosition()
    {
        if (cam == null) return transform.position;
        Vector3 carryOffset = cam.transform.forward * 1.5f + cam.transform.up * -0.3f;
        return cam.transform.position + carryOffset;
    }

    private void UpdateTrajectoryPreview()
    {
        if (trajectoryLine == null) return;

        Vector3 direction = cam.transform.forward;
        float speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, throwIntensity);
        Vector3 simVelocity = direction * speed;
        Vector3 simPos = transform.position;

        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = trajectoryPoints;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            trajectoryLine.SetPosition(i, simPos);
            simVelocity.y -= gravity * trajectoryTimeStep;
            Vector3 nextSimPos = simPos + (simVelocity * trajectoryTimeStep);

            Vector3 sweepDir = nextSimPos - simPos;
            if (sweepDir.sqrMagnitude > 0.001f && Physics.SphereCast(simPos, throwCollisionRadius, sweepDir.normalized, out RaycastHit hit, sweepDir.magnitude, groundLayer))
            {
                trajectoryLine.positionCount = i + 2;
                trajectoryLine.SetPosition(i + 1, hit.point);
                return;
            }
            simPos = nextSimPos;
        }
    }

    

    private void ReleaseThrow()
    {
        GrabManager.Release(this);
        if (trajectoryLine != null) trajectoryLine.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.transform.localScale = initialSpriteLocalScale;

        throwIntensity = Mathf.Max(throwIntensity, 0.05f);
        float speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, throwIntensity);

        throwVelocity = cam.transform.forward * speed;
        currentSpin = Mathf.Lerp(-maxSpinSpeed, maxSpinSpeed, Random.value) * throwIntensity;

        state = GremState.Thrown;
        StartCoroutine(ThrowRoutine());
    }

    private IEnumerator ThrowRoutine()
    {
        Vector3 velocity = throwVelocity;
        Vector3 pos = transform.position;

        while (true)
        {
            if (Physics.CheckSphere(pos, throwCollisionRadius, groundLayer))
            {
                if (Physics.Raycast(pos + Vector3.up * 0.5f, Vector3.down, out RaycastHit groundHit, 1f, groundLayer))
                {
                    transform.position = groundHit.point;
                    yield return StartCoroutine(LandRoutine());
                    yield break;
                }
            }

            velocity.y -= gravity * Time.deltaTime;
            Vector3 step = velocity * Time.deltaTime;
            float stepDistance = step.magnitude;

            if (spriteRenderer != null)
            {
                float z = spriteRenderer.transform.localEulerAngles.z;
                z += currentSpin * Time.deltaTime;
                spriteRenderer.transform.localEulerAngles = new Vector3(0, 0, z);
            }

            bool hitDetected = false;
            RaycastHit hit = default;

            if (stepDistance > 0.001f)
            {
                hitDetected = Physics.SphereCast(pos, throwCollisionRadius, step.normalized, out hit, stepDistance, groundLayer);
            }

            if (hitDetected)
            {
                transform.position = hit.point;
                yield return StartCoroutine(LandRoutine());
                yield break;
            }

            pos += step;
            transform.position = pos;

            if (Physics.Raycast(pos + Vector3.up * 0.1f, Vector3.down, out RaycastHit downHit, 0.2f + throwCollisionRadius, groundLayer))
            {
                transform.position = downHit.point;
                yield return StartCoroutine(LandRoutine());
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator LandRoutine()
    {
        state = GremState.Landing;

        if (spriteRenderer != null)
            spriteRenderer.transform.localEulerAngles = Vector3.zero;

        if (landingParticles != null)
        {
            landingParticles.transform.position = transform.position;
            landingParticles.Emit(landingParticleCount);
        }

        float squashAmount = Mathf.Lerp(minSquashAmount, maxSquashAmount, throwIntensity);
        float t = 0f;
        Vector3 baseScale = spriteRenderer != null ? spriteRenderer.transform.localScale : initialSpriteLocalScale;

        while (t < squashDuration * 0.5f)
        {
            t += Time.deltaTime;
            float p = t / (squashDuration * 0.5f);
            float squash = Mathf.Lerp(1f, 1f - squashAmount, p);
            float stretch = Mathf.Lerp(1f, 1f + squashAmount * 0.5f, p);

            if (spriteRenderer != null)
                spriteRenderer.transform.localScale = new Vector3(baseScale.x * stretch, baseScale.y * squash, baseScale.z * stretch);

            yield return null;
        }

        t = 0f;
        while (t < squashDuration * 0.5f)
        {
            t += Time.deltaTime;
            float p = t / (squashDuration * 0.5f);

            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localScale = Vector3.Lerp(
                    new Vector3(baseScale.x * (1f + squashAmount * 0.5f), baseScale.y * (1f - squashAmount), baseScale.z * (1f + squashAmount * 0.5f)),
                    initialSpriteLocalScale, p);
            }
            yield return null;
        }

        if (spriteRenderer != null) spriteRenderer.transform.localScale = initialSpriteLocalScale;
        transform.localScale = defaultScale;

        agent.enabled = true;
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
            agent.Warp(navHit.position);

        state = GremState.Wander;
        isWaiting = true;
        ScheduleNextMove();

        if (nearSpeaker && likesCurrentSong)
        {
            headingHome = false;
            agent.isStopped = true;
            state = GremState.Vibing;

            TriggerSecretShowIfEligible();
        }
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Speaker"))
        {
            nearSpeaker = true;
            if (likesCurrentSong && (state == GremState.Wander || headingHome))
            {
                headingHome = false;
                agent.isStopped = true;
                state = GremState.Vibing;

                TriggerSecretShowIfEligible();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Speaker"))
        {
            nearSpeaker = false;
            if (state == GremState.Vibing)
            {
                agent.isStopped = false;
                state = GremState.Wander;
                isWaiting = true;
                ScheduleNextMove();
            }
        }
    }

    private void HandleVibeHop()
    {
        if (!nearSpeaker || !likesCurrentSong)
        {
            agent.isStopped = false;
            state = GremState.Wander;
            isWaiting = true;
            ScheduleNextMove();
            return;
        }

        sleepiness = Mathf.Max(0f, sleepiness - vibeSleepinessRelief * Time.deltaTime);
        if (spriteRenderer == null) return;

        float beatsPerSecond = currentBpm / 60f;
        float angularFrequency = Mathf.PI * beatsPerSecond * hopBeatSubdivision;
        float hop = Mathf.Abs(Mathf.Sin(Time.time * angularFrequency)) * vibeHopAmplitude;
        spriteRenderer.transform.localPosition = initialSpriteLocalPos + new Vector3(0, hop, 0);
    }

    

    private void TriggerSecretShowIfEligible()
    {
        if (!favoriteDiscovered)
        {
            favoriteDiscovered = true;
            if (secretShowCoroutine == null && gameObject.activeInHierarchy)
            {
                secretShowCoroutine = StartCoroutine(PlaySecretSpotlightShow());
            }
        }
    }

    

    private IEnumerator PlaySecretSpotlightShow()
    {
        
        if (_data == null || roomMainLights.Count == 0 || stageSpotlights.Count == 0) yield break;

        
        float[] initialRoomIntensities = new float[roomMainLights.Count];
        for (int i = 0; i < roomMainLights.Count; i++)
        {
            initialRoomIntensities[i] = roomMainLights[i] != null ? roomMainLights[i].intensity : 0f;
        }

        
        Vector3 focusCenterPosition = transform.position;
        if (!_data.trackGremurin && !string.IsNullOrEmpty(_data.sceneAnchorTag))
        {
            GameObject anchor = GameObject.FindWithTag(_data.sceneAnchorTag);
            if (anchor != null) focusCenterPosition = anchor.transform.position;
        }

        
        float[] baseSpotIntensities = new float[stageSpotlights.Count];
        Vector3[] spotOrigins = new Vector3[stageSpotlights.Count];
        float[] spotSpeedMultipliers = new float[stageSpotlights.Count];

        for (int i = 0; i < stageSpotlights.Count; i++)
        {
            if (stageSpotlights[i] == null) continue;
            stageSpotlights[i].enabled = true;
            stageSpotlights[i].color = _data.spotlightColor;
            baseSpotIntensities[i] = stageSpotlights[i].intensity;
            spotOrigins[i] = stageSpotlights[i].transform.position;
            spotSpeedMultipliers[i] = Random.Range(0.7f, 1.4f);
        }

        float timer = 0f;
        bool secretTriggered = false;

        
        while (timer < _data.showTotalDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / _data.showTotalDuration;

            
            float dimFactor = Mathf.Sin(progress * Mathf.PI);
            for (int i = 0; i < roomMainLights.Count; i++)
            {
                if (roomMainLights[i] == null) continue;
                float target = initialRoomIntensities[i] * _data.danceShowIntensity;
                roomMainLights[i].intensity = Mathf.Lerp(initialRoomIntensities[i], target, dimFactor);
            }

            
            float pulseWave = Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * (currentBpm / 60f)));
            if (_data.trackGremurin) focusCenterPosition = transform.position;

            for (int i = 0; i < stageSpotlights.Count; i++)
            {
                if (stageSpotlights[i] == null) continue;

                
                float evaluatedSpeed = Time.time * _data.spotlightOrbitSpeed * spotSpeedMultipliers[i];
                float seedX = evaluatedSpeed + (i * 243.19f);
                float seedY = evaluatedSpeed + (i * 711.83f) + 1200f;

                float noiseX = (Mathf.PerlinNoise(seedX, 0f) * 2f) - 1f;
                float noiseY = (Mathf.PerlinNoise(0f, seedY) * 2f) - 1f;

                Vector3 dynamicTarget = focusCenterPosition;
                dynamicTarget.x += noiseX * _data.spotlightRadiusScale;
                dynamicTarget.z += noiseY * _data.spotlightRadiusScale;

                
                Vector3 lookDirection = dynamicTarget - spotOrigins[i];
                if (lookDirection.sqrMagnitude > 0.01f)
                {
                    stageSpotlights[i].transform.rotation = Quaternion.Lerp(
                        stageSpotlights[i].transform.rotation,
                        Quaternion.LookRotation(lookDirection),
                        Time.deltaTime * 8f
                    );
                }

                
                stageSpotlights[i].intensity = baseSpotIntensities[i] * Mathf.Lerp(0.5f, 1.5f, pulseWave);
            }

            if (!secretTriggered && timer >= _data.secretActivationDelay)
            {
                secretTriggered = true;
                if (_data.secretEffect != null)
                    _data.secretEffect.TriggerSecret(this, MusicManager.Instance.CurrentTrack);
            }
            yield return null;
        }

        
        float restoreTimer = 0f;
        while (restoreTimer < 0.3f)
        {
            restoreTimer += Time.deltaTime;
            float t = restoreTimer / 0.3f;
            for (int i = 0; i < roomMainLights.Count; i++)
            {
                if (roomMainLights[i] != null)
                    roomMainLights[i].intensity = Mathf.Lerp(roomMainLights[i].intensity, initialRoomIntensities[i], t);
            }
            yield return null;
        }

        
        for (int i = 0; i < stageSpotlights.Count; i++)
        {
            if (stageSpotlights[i] != null)
            {
                stageSpotlights[i].intensity = baseSpotIntensities[i];
                stageSpotlights[i].enabled = false;
            }
        }
        secretShowCoroutine = null;
    }

    

    private void UpdateGremVisuals()
    {
        if (_data != null && spriteRenderer != null)
            spriteRenderer.sprite = _data.sprite;
    }

    private void BillboardToCamera()
    {
        if (spriteRenderer == null || cameraTransform == null) return;
        Vector3 localPos = spriteRenderer.transform.localPosition;
        spriteRenderer.transform.rotation = cameraTransform.rotation;
        spriteRenderer.transform.localPosition = localPos;
    }

    private void SetupTrajectoryLine()
    {
        GameObject lineObj = new GameObject("TrajectoryLine");
        trajectoryLine = lineObj.AddComponent<LineRenderer>();
        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.startWidth = trajectoryWidth;
        trajectoryLine.endWidth = trajectoryWidth;
        trajectoryLine.material = trajectoryMaterial != null ? trajectoryMaterial : new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = new Color(trajectoryColor.r, trajectoryColor.g, trajectoryColor.b, 0f);
        trajectoryLine.enabled = false;
    }

    private void SetupLandingParticles()
    {
        GameObject psObj = new GameObject("LandingDustPuff");
        landingParticles = psObj.AddComponent<ParticleSystem>();

        var main = landingParticles.main;
        main.startLifetime = landingParticleLifetime;
        main.startSpeed = landingParticleSpeed;
        main.startSize = landingParticleSize;
        main.startColor = landingParticleColor;
        main.gravityModifier = 0.1f;
        main.loop = false;
        main.playOnAwake = false;

        var emission = landingParticles.emission;
        emission.enabled = false;

        var shape = landingParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.15f;

        var vel = landingParticles.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.radial = new ParticleSystem.MinMaxCurve(-0.3f);

        var renderer = landingParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
    }
}