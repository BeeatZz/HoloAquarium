using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;

public enum RuffianRole { Guard, Ward }

public class RuffianEnemy : Enemy
{
    [Header("Ruffian Identity")]
    public RuffianRole role;
    public Sprite ruffianSprite;

    [Header("Link State")]
    public RuffianEnemy partner;
    public bool isLinked = false;
    private bool isLinking = false;
    private bool isInvulnerable = false;

    [Header("Enrage Settings")]
    public float enrageSpeedMultiplier = 1.8f;
    public Color stunColor = Color.gray;
    public Color enrageColor = Color.red;

    protected override void Start() 
    {
        base.Start();
        if (ruffianSprite != null)
            GetComponent<SpriteRenderer>().sprite = ruffianSprite;

        isInvulnerable = false;
    }

    protected override void Think()
    {
        if (!isLinked && !isLinking && IsInPlayArea())
        {
            FindAvailablePartner();
        }

        if (isLinking) return;

        base.Think();
    }

    private bool IsInPlayArea()
    {
        Vector2 pos = transform.position;
        Vector2 min = LevelManager.Instance.playAreaMin;
        Vector2 max = LevelManager.Instance.playAreaMax;
        return pos.x > min.x && pos.x < max.x && pos.y > min.y && pos.y < max.y;
    }

    private void FindAvailablePartner()
    {
        RuffianEnemy[] allRuffians = UnityEngine.Object.FindObjectsByType<RuffianEnemy>(FindObjectsSortMode.None);

        foreach (RuffianEnemy potential in allRuffians)
        {
            if (potential != this && !potential.isLinked && !potential.isLinking && potential.IsInPlayArea())
            {
                if ((this.role == RuffianRole.Guard && potential.role == RuffianRole.Ward) ||
                    (this.role == RuffianRole.Ward && potential.role == RuffianRole.Guard))
                {
                    StartCoroutine(EstablishLink(potential));
                    return;
                }
            }
        }
    }

    private System.Collections.IEnumerator EstablishLink(RuffianEnemy target)
    {
        isLinking = true;
        target.isLinking = true;

        float myOldSpeed = this.moveSpeed;
        float targetOldSpeed = target.moveSpeed;
        this.moveSpeed = 0;
        target.moveSpeed = 0;

        transform.DOShakePosition(0.5f, 0.1f);
        target.transform.DOShakePosition(0.5f, 0.1f);

        yield return new WaitForSeconds(1.0f);

        this.partner = target;
        target.partner = this;
        this.isLinked = true;
        target.isLinked = true;

        if (this.role == RuffianRole.Ward) SetInvulnerable(true);
        if (target.role == RuffianRole.Ward) target.SetInvulnerable(true);

        this.moveSpeed = myOldSpeed;
        target.moveSpeed = targetOldSpeed;

        isLinking = false;
        target.isLinking = false;
    }

    public void SetInvulnerable(bool state) => isInvulnerable = state;

    public override void TakeDamage(float damage)
    {
        if (role == RuffianRole.Ward && isInvulnerable) return;
        base.TakeDamage(damage);
    }

    protected override void Die() 
    {
        if (partner != null) partner.OnPartnerDeath();
        base.Die();
    }

    public void OnPartnerDeath()
    {
        partner = null;
        isLinked = false;
        isInvulnerable = false;
        StartCoroutine(EnrageSequence());
    }

    private System.Collections.IEnumerator EnrageSequence()
    {
        float originalSpeed = moveSpeed;
        moveSpeed = 0;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.DOColor(stunColor, 0.2f).SetLoops(5, LoopType.Yoyo);

        yield return new WaitForSeconds(1.0f);

        sr.DOColor(enrageColor, 0.5f);
        moveSpeed = originalSpeed * enrageSpeedMultiplier;
        transform.DOScale(transform.localScale * 1.2f, 0.5f);
    }
}