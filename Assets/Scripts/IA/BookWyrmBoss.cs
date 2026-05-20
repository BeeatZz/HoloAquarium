using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BookWyrmBoss : Enemy
{
    public BookWyrmData data;

    public GameObject chargeWarningPrefab;
    public GameObject projectilePrefab;
    public GameObject rotatingRayPrefab;
    public GameObject tornadoPrefab;
    public GameObject pageSuckPrefab;
    public Transform firePoint;
    public Transform[] pageSuckSpawnPoints;

    public bool useStateMachine = true;
    public bool isVulnerable { get; private set; }
    public bool isPhase2 { get; private set; }
    public bool isPageSucking { get; private set; }
    public bool isAttacking { get; private set; }
    public int currentAttackCount { get; private set; }
    public float lastAttackTime { get; private set; }
    public float pageDestroyCount { get; private set; }
    public float pageDestroyTimer { get; private set; }
    public bool thresholdRetaliationTriggered { get; private set; }
    public float damageTakenInCurrentVulnerableWindow { get; private set; }
    public float vulnerableStartTime { get; private set; }

    public event Action OnHalfHealth;
    public event Action OnDeath;
    public event Action OnVulnerableStart;
    public event Action OnVulnerableEnd;
    public event Action OnVulnerableDamageThresholdReached;
    public event Action OnAttackCountReached;

    private bool halfHealthFired;
    private Coroutine currentAttackCoroutine;
    private Coroutine pageSuckCoroutine;
    private Vector3 wanderTarget;

    private void Awake()
    {
        GetComponent<BookWyrmStateMachine>().enabled = useStateMachine;
        GetComponent<BookWyrmBehaviorTree>().enabled = !useStateMachine;
    }

    protected override void Start()
    {
        base.Start();

        GameObject[] spawnObjs = GameObject.FindGameObjectsWithTag("PageSuckSpawn");
        pageSuckSpawnPoints = new Transform[spawnObjs.Length];

        for (int i = 0; i < spawnObjs.Length; i++)
        {
            pageSuckSpawnPoints[i] = spawnObjs[i].transform;
        }

        if (data != null)
        {
            maxHealth = data.maxHealth;
            moveSpeed = data.moveSpeed;
        }

        currentHealth = maxHealth;
        PickNewWanderTarget();
    }

    protected override void Think() { }

    protected override void Update()
    {
        base.Update();

        if (pageDestroyTimer > 0)
        {
            pageDestroyTimer -= Time.deltaTime;

            if (pageDestroyTimer <= 0)
                pageDestroyCount = 0;
        }
    }

    public override void TakeDamage(float amount)
    {
        if (isDead) return;
        if (!isVulnerable || damageTakenInCurrentVulnerableWindow >= 10f) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        damageTakenInCurrentVulnerableWindow += amount;

        if (sr != null)
        {
            sr.DOKill();
            sr.DOColor(Color.red, 0.05f)
                .OnComplete(() =>
                {
                    if (isVulnerable && damageTakenInCurrentVulnerableWindow < 10f)
                        sr.DOColor(Color.yellow, 0.15f);
                    else
                        sr.DOColor(Color.white, 0.15f);
                });
        }

        transform.DOPunchScale(Vector3.one * 0.2f, 0.1f, 5, 0.5f);

        float thresholdPct = data != null ? data.phase2TriggerThreshold : 0.5f;
        bool hitPhase2ThisFrame = !halfHealthFired && currentHealth <= maxHealth * thresholdPct;

        if (hitPhase2ThisFrame)
        {
            halfHealthFired = true;
            isPhase2 = true;
            OnHalfHealth?.Invoke();
        }

        if (IsVulnerableDamageThresholdExceeded() &&
            !hitPhase2ThisFrame &&
            !thresholdRetaliationTriggered)
        {
            thresholdRetaliationTriggered = true;
            OnVulnerableDamageThresholdReached?.Invoke();
        }

        if (currentHealth <= 0)
            Die();
    }

    public override void OnPlayerPunch(float punchDamage)
    {
        TakeDamage(punchDamage);
    }

    protected override void Die()
    {
        isDead = true;

        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
        if (pageSuckCoroutine != null) StopCoroutine(pageSuckCoroutine);

        OnDeath?.Invoke();
        base.Die();
    }

    public void DoWander(bool enraged = false)
    {
        float speed = enraged
            ? (data != null ? data.enragedMoveSpeed : 2.5f)
            : (data != null ? data.moveSpeed : 1.5f);

        transform.position = Vector3.MoveTowards(
            transform.position,
            wanderTarget,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, wanderTarget) < 0.1f)
            PickNewWanderTarget();

        UpdateFacing(wanderTarget);
    }

    // Attack system
    public void StartChargeAttack(Action onComplete = null)
    {
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);

        isAttacking = true;
        lastAttackTime = Time.time;

        currentAttackCoroutine = StartCoroutine(ChargeSequence(() =>
        {
            isAttacking = false;
            IncrementAttackCount();
            onComplete?.Invoke();
        }));
    }

    public void StartProjectileAttack(Action onComplete = null)
    {
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);

        isAttacking = true;
        lastAttackTime = Time.time;

        currentAttackCoroutine = StartCoroutine(ProjectileSequence(() =>
        {
            isAttacking = false;
            IncrementAttackCount();
            onComplete?.Invoke();
        }));
    }

    public void StartInkTornado(Action onComplete = null)
    {
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);

        isAttacking = true;
        lastAttackTime = Time.time;

        currentAttackCoroutine = StartCoroutine(InkTornadoSequence(() =>
        {
            isAttacking = false;
            IncrementAttackCount();
            onComplete?.Invoke();
        }));
    }

    public void StartRotatingRays(Action onComplete = null)
    {
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);

        isAttacking = true;
        lastAttackTime = Time.time;

        currentAttackCoroutine = StartCoroutine(RotatingRaySequence(() =>
        {
            isAttacking = false;
            onComplete?.Invoke();
        }));
    }

    public void StartPageSuck(Action onComplete = null)
    {
        if (pageSuckCoroutine != null) StopCoroutine(pageSuckCoroutine);

        isPageSucking = true;

        pageSuckCoroutine = StartCoroutine(PageSuckSequence(() =>
        {
            isPageSucking = false;
            onComplete?.Invoke();
        }));
    }

    public void StartAttackWander(float duration, Action onComplete = null)
    {
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);

        isAttacking = true;
        lastAttackTime = Time.time;

        currentAttackCoroutine = StartCoroutine(AttackWanderSequence(duration, () =>
        {
            isAttacking = false;
            onComplete?.Invoke();
        }));
    }

    // State
    public void EnterVulnerable()
    {
        isVulnerable = true;
        isAttacking = false;

        thresholdRetaliationTriggered = false;
        damageTakenInCurrentVulnerableWindow = 0f;
        vulnerableStartTime = Time.time;

        OnVulnerableStart?.Invoke();

        if (sr != null)
        {
            sr.DOKill();
            sr.DOColor(Color.yellow, 0.2f);
        }
    }

    public void ExitVulnerable()
    {
        if (!isVulnerable) return;

        isVulnerable = false;
        thresholdRetaliationTriggered = false;

        if (sr != null)
        {
            sr.DOKill();
            sr.DOColor(Color.white, 0.2f);
        }

        ClearAttackCounters();
        ResetDamageWindow();

        OnVulnerableEnd?.Invoke();
    }

    public void ExitVulnerableSilently()
    {
        isVulnerable = false;
        thresholdRetaliationTriggered = false;

        if (sr != null)
        {
            sr.DOKill();
            sr.DOColor(Color.white, 0.2f);
        }

        ClearAttackCounters();
        ResetDamageWindow();
    }

    public void FinishAttackWander()
    {
        isAttacking = false;

        if (currentAttackCoroutine != null)
            StopCoroutine(currentAttackCoroutine);
    }

    public void ClearAttackCounters()
    {
        currentAttackCount = 0;
    }

    public void ResetDamageWindow()
    {
        damageTakenInCurrentVulnerableWindow = 0f;
    }

    public void HealFromPage()
    {
        float heal = data != null ? data.healPerPage : 5f;

        currentHealth = Mathf.Clamp(currentHealth + heal, 0, maxHealth);

        if (sr != null)
        {
            sr.DOKill();
            sr.DOColor(Color.green, 0.1f)
                .OnComplete(() =>
                    sr.DOColor(isVulnerable ? Color.yellow : Color.white, 0.2f)
                );
        }
    }

    public void RegisterPageDestroyed()
    {
        pageDestroyCount++;

        float window = data != null ? data.pageDestroyTrackWindow : 3f;
        pageDestroyTimer = window;
    }

    // Helpers
    public bool ShouldEnterVulnerable()
    {
        int baseThreshold = data != null ? data.attacksBeforeVulnerable : 3;
        int threshold = isPhase2 ? baseThreshold * 2 : baseThreshold;

        return currentAttackCount >= threshold && !isVulnerable;
    }

    public bool AttackCooldownReady()
    {
        if (!IsInPlayArea()) return false;
        if (isAttacking) return false;
        if (isVulnerable) return false;
        if (isPageSucking) return false;

        float cd = isPhase2
            ? (data != null ? data.phase2AttackCooldown : 0.8f)
            : (data != null ? data.attackCooldown : 1.5f);

        return Time.time - lastAttackTime >= cd;
    }

    public bool IsVulnerableDamageThresholdExceeded()
    {
        return isVulnerable && damageTakenInCurrentVulnerableWindow >= 10f;
    }

    public string GetNextAttack()
    {
        if (data == null) return "Projectile";

        if (!isPhase2)
        {
            float totalPool = data.p1ChargeWeight + data.p1ProjectileWeight;
            if (totalPool <= 0f) return "Projectile";

            float roll = UnityEngine.Random.Range(0f, totalPool);
            return roll < data.p1ChargeWeight ? "Charge" : "Projectile";
        }

        float totalPool2 =
            data.p2ChargeWeight +
            data.p2ProjectileWeight +
            data.p2InkTornadoWeight;

        if (totalPool2 <= 0f) return "Projectile";

        float roll2 = UnityEngine.Random.Range(0f, totalPool2);

        if (roll2 < data.p2ChargeWeight) return "Charge";
        if (roll2 < data.p2ChargeWeight + data.p2ProjectileWeight) return "Projectile";

        return "Tornado";
    }

    public bool IsInPlayArea()
    {
        if (LevelManager.Instance == null) return true;

        Vector2 pos = transform.position;
        Vector2 min = LevelManager.Instance.playAreaMin;
        Vector2 max = LevelManager.Instance.playAreaMax;

        return pos.x >= min.x && pos.x <= max.x &&
               pos.y >= min.y && pos.y <= max.y;
    }

    private void IncrementAttackCount()
    {
        currentAttackCount++;

        int baseThreshold = data != null ? data.attacksBeforeVulnerable : 3;
        int threshold = isPhase2 ? baseThreshold * 2 : baseThreshold;

        if (currentAttackCount >= threshold)
            OnAttackCountReached?.Invoke();
    }

    private void PickNewWanderTarget()
    {
        if (LevelManager.Instance == null) return;

        Vector2 min = LevelManager.Instance.playAreaMin;
        Vector2 max = LevelManager.Instance.playAreaMax;

        wanderTarget = new Vector3(
            UnityEngine.Random.Range(min.x, max.x),
            UnityEngine.Random.Range(min.y, max.y),
            0
        );
    }

    public void SetAttacking(bool value)
    {
        isAttacking = value;
    }

    // Sequences
    private IEnumerator AttackWanderSequence(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
    }

    private IEnumerator ChargeSequence(Action onComplete)
    {
        Gremurin target = FindNearestGrem();
        if (target == null) { onComplete?.Invoke(); yield break; }

        float warningDur = data != null ? data.chargeWarningDuration : 1.2f;
        Vector3 chargeTarget = target.transform.position;
        GameObject warning = null;

        if (chargeWarningPrefab != null)
        {
            warning = Instantiate(chargeWarningPrefab, chargeTarget, Quaternion.identity);
            warning.transform.DOPunchScale(Vector3.one * 0.3f, warningDur, 5, 0.5f);
        }

        yield return new WaitForSeconds(warningDur);
        if (warning != null) Destroy(warning);

        float chargeSpd = data != null ? data.chargeSpeed : 8f;
        float elapsed = 0f;

        while (elapsed < 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, chargeTarget, chargeSpd * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        float hitRadius = data != null ? data.chargeHitRadius : 0.6f;
        float chargeDmg = data != null ? data.chargeDamage : 1f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius, LayerMask.GetMask("GremHitbox"));

        foreach (Collider2D hit in hits)
        {
            Gremurin g = hit.GetComponentInParent<Gremurin>();
            if (g != null && !g.isDead)
                g.TakeDamage(chargeDmg);
        }

        float postPause = data != null ? data.chargePostAttackPause : 0.3f;
        yield return new WaitForSeconds(postPause);

        onComplete?.Invoke();
    }

    private IEnumerator ProjectileSequence(Action onComplete)
    {
        Gremurin target = FindNearestGrem();
        if (target == null) { onComplete?.Invoke(); yield break; }

        if (projectilePrefab != null && firePoint != null)
        {
            Vector3 dir = (target.transform.position - firePoint.position).normalized;

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            BookWyrmProjectile bwProj = proj.GetComponent<BookWyrmProjectile>();

            float projSpd = data != null ? data.projectileSpeed : 5f;
            float projDmg = data != null ? data.projectileDamage : 1f;

            bwProj?.Init(dir, projSpd, projDmg);
        }

        yield return new WaitForSeconds(0.5f);
        onComplete?.Invoke();
    }

    private IEnumerator RotatingRaySequence(Action onComplete)
    {
        GameObject rays = null;

        if (rotatingRayPrefab != null)
        {
            rays = Instantiate(rotatingRayPrefab, transform.position, Quaternion.identity);
            rays.transform.SetParent(transform);

            RotatingRay rayScript = rays.GetComponent<RotatingRay>();

            float raySpd = data != null ? data.rayRotateSpeed : 60f;
            float disableDur = data != null ? data.cursorDisableDuration : 3f;

            rayScript?.Init(raySpd, disableDur);
        }

        float rayDur = data != null ? data.rayDuration : 5f;
        yield return new WaitForSeconds(rayDur);

        if (rays != null) Destroy(rays);
        onComplete?.Invoke();
    }

    private IEnumerator PageSuckSequence(Action onComplete)
    {
        pageDestroyCount = 0;
        pageDestroyTimer = 0;

        float duration = data != null ? data.pageSuckDuration : 8f;

        Coroutine spawnSub = StartCoroutine(SpawnPageSuckPages());

        yield return new WaitForSeconds(duration);

        if (spawnSub != null) StopCoroutine(spawnSub);

        pageDestroyCount = 0;

        onComplete?.Invoke();
    }

    private IEnumerator SpawnPageSuckPages()
    {
        if (pageSuckPrefab == null ||
            pageSuckSpawnPoints == null ||
            pageSuckSpawnPoints.Length == 0)
            yield break;

        float interval = data != null ? data.pageSuckSpawnInterval : 0.5f;
        float pageSpd = data != null ? data.pageSuckPageSpeed : 3f;

        float elapsed = 0f;

        while (isPageSucking)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= interval)
            {
                elapsed = 0f;

                Transform sp =
                    pageSuckSpawnPoints[UnityEngine.Random.Range(0, pageSuckSpawnPoints.Length)];

                GameObject page = Instantiate(pageSuckPrefab, sp.position, Quaternion.identity);

                PageSuckPage pageScript = page.GetComponent<PageSuckPage>();
                pageScript?.Init(transform, this, pageSpd);
            }

            yield return null;
        }
    }

    private IEnumerator InkTornadoSequence(Action onComplete)
    {
        if (tornadoPrefab != null)
        {
            GameObject tornado = Instantiate(tornadoPrefab, transform.position, Quaternion.identity);

            InkTornado tornadoScript = tornado.GetComponent<InkTornado>();

            float expSpd = data != null ? data.tornadoExpandSpeed : 1f;
            float maxRad = data != null ? data.tornadoMaxRadius : 3f;
            float dur = data != null ? data.tornadoDuration : 5f;
            float dmg = data != null ? data.tornadoDamage : 0.5f;

            tornadoScript?.Init(expSpd, maxRad, dur, dmg);

            yield return new WaitForSeconds(dur + 0.5f);
        }

        onComplete?.Invoke();
    }
}