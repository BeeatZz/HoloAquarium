using UnityEngine;

[CreateAssetMenu(fileName = "New BookWyrmData", menuName = "Gremurin/BookWyrmData")]
public class BookWyrmData : ScriptableObject
{
    public float maxHealth = 100f;
    [Range(0f, 1f)] public float phase2TriggerThreshold = 0.5f; 
    public float moveSpeed = 1.5f;
    public float enragedMoveSpeed = 2.5f;
    public int attacksBeforeVulnerable = 3;
    public float vulnerableDuration = 5f;
    public float attackCooldown = 1.5f;
    public float phase2AttackCooldown = 0.8f;
    public float chargeSpeed = 8f;
    public float chargeWarningDuration = 1.2f;
    public float chargePostAttackPause = 1.0f; 
    public float chargeDamage = 1f;
    public float chargeHitRadius = 0.6f;
    public float projectileSpeed = 5f;
    public float projectileDamage = 1f;
    public float rayRotateSpeed = 60f;
    public float rayDuration = 5f;
    public float cursorDisableDuration = 3f;
    public float tornadoExpandSpeed = 1f;
    public float tornadoMaxRadius = 3f;
    public float tornadoDuration = 5f;
    public float tornadoDamage = 0.5f;
    public float pageSuckDuration = 8f;
    public float healPerPage = 5f;
    public float pageDestroyRateThreshold = 2f;
    public float pageDestroyTrackWindow = 3f;
    public float pageSuckPageSpeed = 3f;
    public float pageSuckSpawnInterval = 0.5f;

    public float p1ChargeWeight = 50f;
    public float p1ProjectileWeight = 50f;

    public float p2ChargeWeight = 30f;
    public float p2ProjectileWeight = 40f; 
    public float p2InkTornadoWeight = 30f;
    public float p2PageSuckWeight = 0f;
}