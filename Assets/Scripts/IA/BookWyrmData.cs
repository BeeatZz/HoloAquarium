using UnityEngine;

[CreateAssetMenu(fileName = "New BookWyrmData", menuName = "Gremurin/BookWyrmData")]
public class BookWyrmData : ScriptableObject
{
    [Header("Core Stats")]
    public float maxHealth = 100f;
    [Range(0f, 1f)] public float phase2TriggerThreshold = 0.5f; // Split point for the fight
    public float moveSpeed = 1.5f;
    public float enragedMoveSpeed = 2.5f;

    [Header("State Flow & Timing")]
    public int attacksBeforeVulnerable = 3;
    public float vulnerableDuration = 5f;
    public float attackCooldown = 1.5f;
    public float phase2AttackCooldown = 0.8f;

    [Header("Attack: Charge")]
    public float chargeSpeed = 8f;
    public float chargeWarningDuration = 1.2f;
    public float chargePostAttackPause = 1.0f; // Stun/Reset window after charging
    public float chargeDamage = 1f;
    public float chargeHitRadius = 0.6f;

    [Header("Attack: Projectile")]
    public float projectileSpeed = 5f;
    public float projectileDamage = 1f;

    [Header("Attack: Rotating Rays")]
    public float rayRotateSpeed = 60f;
    public float rayDuration = 5f;
    public float cursorDisableDuration = 3f;

    [Header("Attack: Ink Tornado")]
    public float tornadoExpandSpeed = 1f;
    public float tornadoMaxRadius = 3f;
    public float tornadoDuration = 5f;
    public float tornadoDamage = 0.5f;

    [Header("Mechanic: Page Suck")]
    public float pageSuckDuration = 8f;
    public float healPerPage = 5f;
    public float pageDestroyRateThreshold = 2f;
    public float pageDestroyTrackWindow = 3f;
    public float pageSuckPageSpeed = 3f;
    public float pageSuckSpawnInterval = 0.5f;

    [Header("Phase 1 Weights (Relative)")]
    public float p1ChargeWeight = 50f;
    public float p1ProjectileWeight = 50f;

    [Header("Phase 2 Weights (Relative)")]
    public float p2ChargeWeight = 40f;
    public float p2ProjectileWeight = 20f; 
    public float p2InkTornadoWeight = 20f;
    public float p2PageSuckWeight = 30f;
}