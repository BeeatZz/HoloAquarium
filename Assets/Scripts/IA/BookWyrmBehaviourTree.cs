using System;
using System.Collections.Generic;
using UnityEngine;

public class BookWyrmBehaviorTree : MonoBehaviour
{
    public BookWyrmBoss boss;
    private BTNode root;
    private bool isRunningAttack;
    private BTNode activePhase1Attack;
    private BTNode activePhase2Attack;

    private void Start()
    {
        boss.OnVulnerableEnd += ResetTreeAttackState;

        boss.OnHalfHealth += () =>
        {
            boss.ExitVulnerableSilently();

            ResetTreeAttackState();

            boss.StartPageSuck(() =>
            {
                ResetTreeAttackState();
            });
        };

        boss.OnDeath += () => enabled = false;

        root = BuildTree();
    }

    private void Update()
    {
        if (boss.isDead)
        {
            return;
        }

        root.Tick();
    }

    private void ResetTreeAttackState()
    {
        isRunningAttack = false;

        activePhase1Attack = null;
        activePhase2Attack = null;

        boss.ResetDamageWindow();
    }

    private BTNode BuildTree()
    {
        BTNode isDead = new BTCondition(() => boss.isDead);
        BTNode isPhase2 = new BTCondition(() => boss.isPhase2);
        BTNode isVulnerable = new BTCondition(() => boss.isVulnerable);

        BTNode shouldVulnerable = new BTCondition(() =>
            boss.ShouldEnterVulnerable()
        );

        BTNode attackSequenceRunningOrReady = new BTCondition(() =>
            boss.isAttacking || boss.AttackCooldownReady()
        );

        BTNode wander = new BTAction("Wander", () =>
        {
            boss.DoWander(false);
            return BTResult.Running;
        });

        BTNode enragedWander = new BTAction("EnragedWander", () =>
        {
            boss.DoWander(true);
            return BTResult.Running;
        });

        BTNode enterVulnerable = new BTAction("EnterVulnerable", () =>
        {
            if (!boss.isVulnerable)
            {
                boss.EnterVulnerable();
            }

            return BTResult.Success;
        });

        // Attack nodes

        BTNode charge = new BTAction("Charge", () =>
        {
            if (!isRunningAttack)
            {
                isRunningAttack = true;

                boss.StartChargeAttack(() =>
                {
                    ResetTreeAttackState();
                });
            }

            return BTResult.Running;
        });

        BTNode projectile = new BTAction("Projectile", () =>
        {
            if (!isRunningAttack)
            {
                isRunningAttack = true;

                boss.StartProjectileAttack(() =>
                {
                    ResetTreeAttackState();
                });
            }

            return BTResult.Running;
        });

        BTNode inkTornado = new BTAction("InkTornado", () =>
        {
            if (!isRunningAttack)
            {
                isRunningAttack = true;

                boss.StartInkTornado(() =>
                {
                    ResetTreeAttackState();
                });
            }

            return BTResult.Running;
        });

        // Attack wandering phase between attacks

        float attackWanderTimer = 0f;

        BTNode attackWanderAction = new BTAction("AttackWanderPool", () =>
        {
            if (!isRunningAttack)
            {
                isRunningAttack = true;
                attackWanderTimer = Time.time + 1.5f;
            }

            if (Time.time < attackWanderTimer)
            {
                boss.DoWander(boss.isPhase2);
                return BTResult.Running;
            }

            isRunningAttack = false;

            boss.FinishAttackWander();

            activePhase1Attack = null;
            activePhase2Attack = null;

            return BTResult.Success;
        });

        // Vulnerable entry sequence

        BTNode enterVulnerableSequence = new BTSequence(
            new List<BTNode>
            {
                shouldVulnerable,
                enterVulnerable
            }
        );

        // Vulnerable damage reaction logic

        BTNode vulnerableBranchWithDamageReactor = new BTReactor(
            new BTAction("HelplessVulnerableIdle", () =>
            {
                return BTResult.Running;
            }),

            new BTAction("ForcePhaseCounterRetaliation", () =>
            {
                boss.ExitVulnerable();

                ResetTreeAttackState();

                if (boss.isPhase2)
                {
                    boss.StartPageSuck(() =>
                    {
                        ResetTreeAttackState();
                    });

                    return BTResult.Success;
                }

                boss.StartRotatingRays(() =>
                {
                    ResetTreeAttackState();
                });

                return BTResult.Success;
            }),

            () => boss.thresholdRetaliationTriggered || !boss.isVulnerable
        );

        // Phase 1 attack selector

        BTNode phase1Attacks = new BTAction("Phase1Selector", () =>
        {
            if (boss.isAttacking && activePhase1Attack == null)
            {
                return BTResult.Running;
            }

            if (activePhase1Attack != null)
            {
                BTResult result = activePhase1Attack.Tick();

                if (result != BTResult.Running)
                {
                    activePhase1Attack = null;
                }

                return result;
            }

            float wanderRoll = UnityEngine.Random.value;

            if (wanderRoll < 0.2f)
            {
                activePhase1Attack = attackWanderAction;
            }
            else
            {
                string choice = boss.GetNextAttack();

                activePhase1Attack =
                    (choice == "Charge")
                        ? charge
                        : projectile;
            }

            return activePhase1Attack.Tick();
        });

        // Phase 2 attack selector

        BTNode phase2Attacks = new BTAction("Phase2Selector", () =>
        {
            if ((boss.isAttacking || boss.isPageSucking) && activePhase2Attack == null)
            {
                return BTResult.Running;
            }

            if (activePhase2Attack != null)
            {
                BTResult result = activePhase2Attack.Tick();

                if (result != BTResult.Running)
                {
                    activePhase2Attack = null;
                }

                return result;
            }

            float wanderRoll = UnityEngine.Random.value;

            if (wanderRoll < 0.2f)
            {
                activePhase2Attack = attackWanderAction;
            }
            else
            {
                string choice = boss.GetNextAttack();

                if (choice == "Tornado")
                {
                    activePhase2Attack = inkTornado;
                }
                else if (choice == "Projectile")
                {
                    activePhase2Attack = projectile;
                }
                else
                {
                    activePhase2Attack = charge;
                }
            }

            return activePhase2Attack.Tick();
        });

        // Vulnerable branch

        BTNode vulnerableBranch = new BTSequence(
            new List<BTNode>
            {
                isVulnerable,
                vulnerableBranchWithDamageReactor
            }
        );

        // Root behavior tree

        return new BTSelector(
            new List<BTNode>
            {
                new BTSequence(
                    new List<BTNode>
                    {
                        isDead
                    }
                ),

                vulnerableBranch,

                enterVulnerableSequence,

                new BTSequence(
                    new List<BTNode>
                    {
                        isPhase2,

                        new BTSelector(
                            new List<BTNode>
                            {
                                new BTSequence(
                                    new List<BTNode>
                                    {
                                        attackSequenceRunningOrReady,
                                        phase2Attacks
                                    }
                                ),

                                enragedWander
                            }
                        )
                    }
                ),

                new BTSelector(
                    new List<BTNode>
                    {
                        new BTSequence(
                            new List<BTNode>
                            {
                                attackSequenceRunningOrReady,
                                phase1Attacks
                            }
                        ),

                        wander
                    }
                )
            }
        );
    }
}