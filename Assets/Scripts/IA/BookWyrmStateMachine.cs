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

    private void Start()
    {
        // Phase transition event
        boss.OnHalfHealth += () =>
        {
            if (currentState == State.Vulnerable || currentState == State.VulnerableRetaliation)
            {
                // Force exit vulnerability clean if we phase shift mid-stun
                boss.ExitVulnerable();
                boss.ClearAttackCounters();
                boss.ResetDamageWindow();
            }

            boss.StartPageSuck();

            // Allow the state machine/tree to resolve the correct phase state dynamically
            TransitionTo(ResolveNextState());
        };

        boss.OnDeath += () => enabled = false;

        // Triggers naturally when attack counter maxes out
        boss.OnAttackCountReached += () =>
        {
            if (currentState != State.Vulnerable && currentState != State.VulnerableRetaliation)
                TransitionTo(State.Vulnerable);
        };

        // Triggers immediately when the 10 damage cap is reached inside the damage window
        boss.OnVulnerableDamageThresholdReached += () =>
        {
            if (currentState == State.Vulnerable)
            {
                TransitionTo(State.VulnerableRetaliation);
            }
        };

        // The boss's internal timer ended naturally without the player hitting the damage threshold
        boss.OnVulnerableEnd += () =>
        {
            // Only process natural timeouts if we are actively in the Vulnerable state.
            if (currentState == State.Vulnerable)
            {
                boss.ClearAttackCounters();
                boss.ResetDamageWindow();
                TransitionTo(ResolveNextState());
            }
        };

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
                boss.DoWander(boss.isPhase2);
                break;

            case State.AttackCooldown:
                UpdateCooldown();
                break;

            case State.Vulnerable:
                // Stand completely still while vulnerable
                break;

            case State.VulnerableRetaliation:
                if (boss.isPhase2)
                {
                    boss.DoWander(true);
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
        // FIX: Instead of forcing State.Wander directly, resolve the next logical state
        // This ensures decorator rules can be calculated cleanly.
        if (boss.AttackCooldownReady())
            TransitionTo(ResolveNextState());
    }

    private State ResolveNextState()
    {
        // If your Behavior Tree is managing decorators, always check if a special override is needed first
        if (boss.ShouldEnterVulnerable())
        {
            return State.Vulnerable;
        }

        float attackWanderRoll = UnityEngine.Random.value;

        if (attackWanderRoll < 0.2f)
            return State.AttackWander;

        string chosenAttack = boss.GetNextAttack();

        switch (chosenAttack)
        {
            case "Charge":
                return State.Charging;

            case "Projectile":
                return State.ProjectileAttack;

            case "Tornado":
                return State.InkTornado;

            default:
                return boss.isPhase2 ? State.EnragedWander : State.Wander;
        }
    }

    private void TransitionTo(State next)
    {
        OnExitState(currentState);
        currentState = next;
        OnEnterState(currentState);
    }

    private void OnEnterState(State state)
    {
        switch (state)
        {
            case State.Vulnerable:
                boss.EnterVulnerable();
                break;

            case State.Charging:
                boss.StartChargeAttack(() =>
                {
                    TransitionTo(State.AttackCooldown);
                });
                break;

            case State.ProjectileAttack:
                boss.StartProjectileAttack(() =>
                {
                    TransitionTo(State.AttackCooldown);
                });
                break;

            case State.InkTornado:
                boss.StartInkTornado(() =>
                {
                    TransitionTo(State.AttackCooldown);
                });
                break;

            case State.AttackWander:
                boss.StartAttackWander(1.5f, () =>
                {
                    TransitionTo(State.AttackCooldown);
                });
                break;

            case State.VulnerableRetaliation:
                // Clean up vulnerable state
                boss.ExitVulnerable();

                // Explicitly lock the state machine
                boss.SetAttacking(true);

                if (boss.isPhase2)
                {
                    boss.StartPageSuck(() =>
                    {
                        boss.SetAttacking(false);
                        TransitionTo(State.AttackCooldown);
                    });
                }
                else
                {
                    boss.StartRotatingRays(() =>
                    {
                        boss.SetAttacking(false);
                        TransitionTo(State.AttackCooldown);
                    });
                }
                break;
        }
    }

    private void OnExitState(State state)
    {
    }
}