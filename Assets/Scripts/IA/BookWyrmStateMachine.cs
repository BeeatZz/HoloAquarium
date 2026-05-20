using System;
using UnityEngine;

public class BookWyrmStateMachine : MonoBehaviour
{
    public enum State
    {
        Wander,
        Charging,
        ProjectileAttack,
        AttackCooldown,
        Vulnerable,
        VulnerableRetaliation,
        EnragedWander,
        InkTornado,
        AttackWander
    }

    public BookWyrmBoss boss;
    public State currentState;

    private float cooldownEnterTime;
    private bool pendingPhaseShift;
    private bool retaliationInProgress;
    private float retaliationStartTime;

    private void Start()
    {
        // Phase transition (half health)
        boss.OnHalfHealth += () =>
        {
            if (retaliationInProgress)
            {
                pendingPhaseShift = true;
            }
            else if (currentState == State.Vulnerable)
            {
                pendingPhaseShift = true;
                boss.ExitVulnerableSilently();
                TransitionTo(State.AttackCooldown);
            }
            else
            {
                ApplyPhaseShift();
            }
        };

        // Damage threshold reached during vulnerable state
        boss.OnVulnerableDamageThresholdReached += () =>
        {
            if (currentState == State.Vulnerable && !retaliationInProgress)
            {
                TransitionTo(State.VulnerableRetaliation);
            }
        };

        // Natural vulnerable end
        boss.OnVulnerableEnd += () =>
        {
            if (currentState == State.Vulnerable)
            {
                TransitionTo(State.AttackCooldown);
            }
        };

        boss.OnDeath += () => enabled = false;

        TransitionTo(State.Wander);
    }

    private void Update()
    {
        if (boss.isDead) return;

        switch (currentState)
        {
            case State.Wander:
                UpdateWander();
                break;

            case State.EnragedWander:
                UpdateEnragedWander();
                break;

            case State.AttackWander:
                break;

            case State.AttackCooldown:
                UpdateCooldown();
                break;

            case State.Vulnerable:
                float vulnDuration = boss.data != null ? boss.data.vulnerableDuration : 5f;

                if (vulnDuration <= 0f)
                {
                    vulnDuration = 5f;
                }

                if (Time.time >= boss.vulnerableStartTime + vulnDuration)
                {
                    boss.ExitVulnerable();
                }
                break;

            case State.VulnerableRetaliation:
                if (Time.time - retaliationStartTime > 15f)
                {
                    retaliationInProgress = false;
                    TransitionTo(State.AttackCooldown);
                }
                break;
        }
    }

    private void UpdateWander()
    {
        boss.DoWander(false);

        if (boss.ShouldEnterVulnerable())
        {
            TransitionTo(State.Vulnerable);
            return;
        }

        if (boss.AttackCooldownReady())
            TransitionTo(ResolveNextState());
    }

    private void UpdateEnragedWander()
    {
        boss.DoWander(true);

        if (boss.ShouldEnterVulnerable())
        {
            TransitionTo(State.Vulnerable);
            return;
        }

        if (boss.AttackCooldownReady())
            TransitionTo(ResolveNextState());
    }

    private void UpdateCooldown()
    {
        if (boss.ShouldEnterVulnerable())
        {
            TransitionTo(State.Vulnerable);
            return;
        }

        if (boss.AttackCooldownReady())
        {
            TransitionTo(ResolveNextState());
            return;
        }

        if (Time.time - cooldownEnterTime > 3f)
        {
            boss.SetAttacking(false);
            retaliationInProgress = false;
            TransitionTo(boss.isPhase2 ? State.EnragedWander : State.Wander);
        }
    }

    private State ResolveNextState()
    {
        if (boss.ShouldEnterVulnerable())
            return State.Vulnerable;

        float attackWanderRoll = UnityEngine.Random.value;
        if (attackWanderRoll < 0.2f)
            return State.AttackWander;

        string chosenAttack = boss.GetNextAttack();

        switch (chosenAttack)
        {
            case "Charge": return State.Charging;
            case "Projectile": return State.ProjectileAttack;
            case "Tornado": return State.InkTornado;
            case "PageSuck": return State.Charging;

            default:
                return boss.isPhase2 ? State.EnragedWander : State.Wander;
        }
    }

    private void TransitionTo(State next)
    {
        State oldState = currentState;
        currentState = next;

        OnExitState(oldState);
        OnEnterState(currentState);

        if (next == State.AttackCooldown)
        {
            cooldownEnterTime = Time.time;

            if (pendingPhaseShift)
            {
                pendingPhaseShift = false;
                ApplyPhaseShift();
            }
        }
    }

    private void OnEnterState(State state)
    {
        switch (state)
        {
            case State.Vulnerable:
                boss.EnterVulnerable();
                break;

            case State.Charging:
                boss.StartChargeAttack(() => OnAttackComplete());
                break;

            case State.ProjectileAttack:
                boss.StartProjectileAttack(() => OnAttackComplete());
                break;

            case State.InkTornado:
                boss.StartInkTornado(() => OnAttackComplete());
                break;

            case State.AttackWander:
                boss.StartAttackWander(1.5f, () => TransitionTo(State.AttackCooldown));
                break;

            case State.VulnerableRetaliation:
                retaliationInProgress = true;
                retaliationStartTime = Time.time;

                boss.ExitVulnerableSilently();
                boss.SetAttacking(true);

                if (boss.isPhase2)
                {
                    boss.StartPageSuck(() =>
                    {
                        retaliationInProgress = false;
                        TransitionTo(State.AttackCooldown);
                    });
                }
                else
                {
                    boss.StartRotatingRays(() =>
                    {
                        retaliationInProgress = false;
                        TransitionTo(State.AttackCooldown);
                    });
                }
                break;
        }
    }

    private void OnExitState(State state)
    {
        switch (state)
        {
            case State.VulnerableRetaliation:
            case State.Charging:
            case State.ProjectileAttack:
            case State.InkTornado:
            case State.AttackWander:
                boss.SetAttacking(false);
                break;
        }
    }

    private void OnAttackComplete()
    {
        if (currentState == State.Vulnerable || currentState == State.VulnerableRetaliation)
            return;

        if (boss.ShouldEnterVulnerable())
        {
            TransitionTo(State.Vulnerable);
            return;
        }

        TransitionTo(State.AttackCooldown);
    }

    private void ApplyPhaseShift()
    {
        boss.StartPageSuck(() =>
        {
            TransitionTo(ResolveNextState());
        });
    }
}