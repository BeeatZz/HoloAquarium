using UnityEngine;
using System;
using System.Collections.Generic;

public class BookWyrmBehaviorTree : MonoBehaviour
{
    [Header("References")]
    public BookWyrmBoss boss;

    private BTNode root;
    private bool isRunningAttack;

    private BTNode activePhase1Attack = null;
    private BTNode activePhase2Attack = null;

    private void Start()
    {
        boss.OnVulnerableEnd += ResetTreeAttackState;

        boss.OnHalfHealth += () =>
        {
            ResetTreeAttackState();
            boss.StartPageSuck();
        };

        boss.OnDeath += () => enabled = false;
        root = BuildTree();
    }

    private void Update()
    {
        if (boss.isDead) return;
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
        BTNode shouldVulnerable = new BTCondition(() => boss.ShouldEnterVulnerable());
        BTNode attackSequenceRunningOrReady = new BTCondition(() => isRunningAttack || boss.AttackCooldownReady());

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

        BTNode charge = new BTAction("Charge", () =>
        {
            if (!isRunningAttack)
            {
                isRunningAttack = true;
                boss.StartChargeAttack(() => {
                    isRunningAttack = false;
                    activePhase1Attack = null;
                    activePhase2Attack = null;
                });
            }
            return BTResult.Running;
        });

        BTNode projectile = new BTAction("Projectile", () =>
        {
            if (!isRunningAttack)
            {
                isRunningAttack = true;
                boss.StartProjectileAttack(() => {
                    isRunningAttack = false;
                    activePhase1Attack = null;
                    activePhase2Attack = null;
                });
            }
            return BTResult.Running;
        });

        BTNode inkTornado = new BTAction("InkTornado", () =>
        {
            if (!isRunningAttack)
            {
                isRunningAttack = true;
                boss.StartInkTornado(() => {
                    isRunningAttack = false;
                    activePhase1Attack = null;
                    activePhase2Attack = null;
                });
            }
            return BTResult.Running;
        });

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

        BTNode enterVulnerableSequence = new BTSequence(new List<BTNode>
        {
            shouldVulnerable,
            enterVulnerable
        });

        BTNode vulnerableBranchWithDamageReactor = new BTReactor(
            new BTAction("HelplessVulnerableIdle", () =>
            {
                return BTResult.Running;
            }),
            new BTAction("ForcePhaseCounterRetaliation", () => {
                boss.ExitVulnerable();
                boss.ClearAttackCounters();
                boss.ResetDamageWindow();

                if (boss.isPhase2)
                {
                    boss.StartPageSuck();
                    ResetTreeAttackState();
                    return BTResult.Success;
                }
                else
                {
                    if (!isRunningAttack)
                    {
                        isRunningAttack = true;
                        boss.StartRotatingRays(() => ResetTreeAttackState());
                    }
                    return BTResult.Running;
                }
            }),
            () => boss.IsVulnerableDamageThresholdExceeded()
        );

        // FIX: Replaced explicit inline random distribution with a call to the central weights machine
        BTNode phase1Attacks = new BTAction("Phase1Selector", () =>
        {
            if (activePhase1Attack != null)
            {
                BTResult res = activePhase1Attack.Tick();
                if (res != BTResult.Running) activePhase1Attack = null;
                return res;
            }

            float wanderRoll = UnityEngine.Random.value;
            if (wanderRoll < 0.2f)
            {
                activePhase1Attack = attackWanderAction;
            }
            else
            {
                string choice = boss.GetNextAttack();
                activePhase1Attack = (choice == "Charge") ? charge : projectile;
            }

            return activePhase1Attack.Tick();
        });

        // FIX: Seamlessly leverages the central dynamic weight evaluation mapping for Phase 2
        BTNode phase2Attacks = new BTAction("Phase2Selector", () =>
        {
            if (activePhase2Attack != null)
            {
                BTResult res = activePhase2Attack.Tick();
                if (res != BTResult.Running) activePhase2Attack = null;
                return res;
            }

            float wanderRoll = UnityEngine.Random.value;
            if (wanderRoll < 0.2f)
            {
                activePhase2Attack = attackWanderAction;
            }
            else
            {
                string choice = boss.GetNextAttack();
                if (choice == "Tornado") activePhase2Attack = inkTornado;
                else if (choice == "Projectile") activePhase2Attack = projectile;
                else activePhase2Attack = charge; // Default fallback sequence
            }

            return activePhase2Attack.Tick();
        });

        BTNode vulnerableBranch = new BTSequence(new List<BTNode>
        {
            isVulnerable,
            vulnerableBranchWithDamageReactor
        });

        return new BTSelector(new List<BTNode>
        {
            new BTSequence(new List<BTNode> { isDead }),
            vulnerableBranch,
            enterVulnerableSequence,
            new BTSequence(new List<BTNode>
            {
                isPhase2,
                new BTSelector(new List<BTNode>
                {
                    new BTSequence(new List<BTNode> { attackSequenceRunningOrReady, phase2Attacks }),
                    enragedWander
                })
            }),
            new BTSelector(new List<BTNode>
            {
                new BTSequence(new List<BTNode> { attackSequenceRunningOrReady, phase1Attacks }),
                wander
            })
        });
    }
}