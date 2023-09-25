using System;
using System.Collections.Generic;
using FSG.MeshAnimator;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Data;
using Main.Scripts.Actions.Health;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Effects;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
using Pathfinding;
using UnityEngine;

namespace Main.Scripts.Enemies
{
    public class EnemyLogicDelegate :
        Damageable,
        Healable,
        Affectable,
        ObjectWithGettingKnockBack,
        ObjectWithGettingStun,
        EffectsManager.EventListener,
        ActiveSkillsManager.EventListener
    {
        private static readonly int IS_MOVING_ANIM = Animator.StringToHash("isMoving");
        private static readonly int ATTACK_ANIM = Animator.StringToHash("Attack");
        private static readonly float HEALTH_THRESHOLD = 0.01f;

        private EnemyConfig config;
        private DataHolder dataHolder;
        private EventListener eventListener;
        private NetworkObject objectContext = default!;

        private Transform transform;
        private NetworkRigidbody3D rigidbody3D;
        private RichAI richAI;
        private MeshAnimatorBase meshAnimator;
        private HealthBar healthBar;

        private EnemiesHelper enemiesHelper = default!;
        private NavigationManager navigationManager = default!;

        private ActiveSkillsManager activeSkillsManager;
        private PassiveSkillsManager passiveSkillsManager;
        private EffectsManager effectsManager;
        private HealthChangeDisplayManager? healthChangeDisplayManager;

        private int lastAnimationTriggerId;

        private EnemyAnimationState currentAnimationState;

        private float sqrAttackDistance;
        private Vector3 lookDirection;
        private int movementDeltaTicks;

        private List<KnockBackActionData> knockBackActions = new();
        private List<StunActionData> stunActions = new();
        private List<DamageActionData> damageActions = new();
        private List<HealActionData> healActions = new();
        private List<EffectsCombination> effectActions = new();
        private bool shouldDespawn;

        public readonly GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.EffectsRemoveFinishedPhase,
            GameLoopPhase.StrategyPhase,
            GameLoopPhase.SkillActivationPhase,
            GameLoopPhase.SkillSpawnPhase,
            GameLoopPhase.SkillUpdatePhase,
            GameLoopPhase.EffectsApplyPhase,
            GameLoopPhase.EffectsUpdatePhase,
            GameLoopPhase.ApplyActionsPhase,
            GameLoopPhase.DespawnPhase,
            GameLoopPhase.PhysicsUpdatePhase,
            GameLoopPhase.PhysicsUnitsLookPhase,
            GameLoopPhase.NavigationPhase,
            GameLoopPhase.VisualStateUpdatePhase
        };

        public EnemyLogicDelegate(
            ref EnemyConfig config,
            DataHolder dataHolder,
            EventListener eventListener
        )
        {
            this.config = config;
            this.dataHolder = dataHolder;
            this.eventListener = eventListener;

            rigidbody3D = dataHolder.GetCachedComponent<NetworkRigidbody3D>();
            richAI = dataHolder.GetCachedComponent<RichAI>();
            meshAnimator = dataHolder.GetCachedComponent<MeshAnimatorBase>();
            transform = dataHolder.GetCachedComponent<Transform>();

            richAI.updatePosition = false;

            healthBar = config.HealthBar;

            effectsManager = new EffectsManager(
                dataHolder: dataHolder,
                eventListener: this,
                effectsTarget: this
            );
            activeSkillsManager = new ActiveSkillsManager(
                config: ref config.ActiveSkillsConfig,
                dataHolder: dataHolder,
                eventListener: this,
                transform
            );
            passiveSkillsManager = new PassiveSkillsManager(
                config: ref config.PassiveSkillsConfig,
                affectable: this,
                transform: transform
            );

            if (config.ShowHealthChangeDisplay)
            {
                healthChangeDisplayManager = new HealthChangeDisplayManager(
                    config: ref config.HealthChangeDisplayConfig,
                    dataHolder: dataHolder
                );
            }

            sqrAttackDistance = config.AttackDistance * config.AttackDistance;
        }

        public static void OnValidate(GameObject gameObject, ref EnemyConfig config)
        {
            PassiveSkillsManager.OnValidate(gameObject, ref config.PassiveSkillsConfig);
            ActiveSkillsManager.OnValidate(ref config.ActiveSkillsConfig);
        }

        public void Spawned(NetworkObject objectContext)
        {
            this.objectContext = objectContext;
            ResetState();

            enemiesHelper = dataHolder.GetCachedComponent<EnemiesHelper>();
            navigationManager = dataHolder.GetCachedComponent<NavigationManager>();
            richAI.enabled = objectContext.HasStateAuthority;
            if (this.objectContext.HasStateAuthority)
            {
                richAI.Teleport(transform.position);
            }
            
            effectsManager.Spawned(objectContext);
            activeSkillsManager.Spawned(objectContext);
            passiveSkillsManager.Spawned(objectContext);
            healthChangeDisplayManager?.Spawned(objectContext);
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            effectsManager.Despawned(runner, hasState);
            activeSkillsManager.Despawned(runner, hasState);
            healthChangeDisplayManager?.Despawned(runner, hasState);

            objectContext = default!;
            enemiesHelper = default!;
        }

        private void ResetState()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            
            knockBackActions.Clear();
            stunActions.Clear();
            damageActions.Clear();
            healActions.Clear();
            effectActions.Clear();
            shouldDespawn = false;
            lookDirection = Vector3.zero;
            movementDeltaTicks = 0;

            enemyData.maxHealth = config.DefaultMaxHealth;
            enemyData.speed = config.DefaultSpeed;

            effectsManager.ResetState();
            passiveSkillsManager.Init(); //reset after reset effectsManager

            enemyData.health = enemyData.maxHealth;
            healthBar.SetMaxHealth((uint)Math.Max(0, enemyData.maxHealth));

            enemyData.isDead = false;
            currentAnimationState = EnemyAnimationState.None;
        }

        public void Render()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();

            switch (currentAnimationState)
            {
                case EnemyAnimationState.Walking:
                    var velocity = rigidbody3D.VelocityInterpolated.magnitude / config.DefaultSpeed;
                    meshAnimator.speed = velocity;
                    break;
            }

            healthBar.SetMaxHealth((uint)enemyData.maxHealth);
            healthBar.SetHealth((uint)enemyData.health);
            
            activeSkillsManager.Render();

            healthChangeDisplayManager?.Render();
        }

        public void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.EffectsRemoveFinishedPhase:
                    effectsManager.RemoveFinishedEffects();
                    break;
                case GameLoopPhase.StrategyPhase:
                    OnStrategyPhase();
                    break;
                case GameLoopPhase.SkillActivationPhase:
                case GameLoopPhase.SkillUpdatePhase:
                case GameLoopPhase.SkillSpawnPhase:
                    activeSkillsManager.OnGameLoopPhase(phase);
                    passiveSkillsManager.OnGameLoopPhase(phase);
                    break;
                case GameLoopPhase.EffectsApplyPhase:
                    ApplyEffects();
                    break;
                case GameLoopPhase.EffectsUpdatePhase:
                    effectsManager.UpdateEffects();
                    break;
                case GameLoopPhase.ApplyActionsPhase:
                    OnApplyActionsPhase();
                    break;
                case GameLoopPhase.DespawnPhase:
                    OnDespawnPhase();
                    break;
                case GameLoopPhase.PhysicsUpdatePhase:
                    OnPhysicsUpdatePhase();
                    break;
                case GameLoopPhase.PhysicsUnitsLookPhase:
                    OnPhysicsUnitsLookPhase();
                    break;
                case GameLoopPhase.NavigationPhase:
                    OnNavigationPhase();
                    break;
                case GameLoopPhase.VisualStateUpdatePhase:
                    OnVisualStateUpdatePhase();
                    activeSkillsManager.OnGameLoopPhase(phase);
                    passiveSkillsManager.OnGameLoopPhase(phase);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        private void OnStrategyPhase()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();

            UpdateTickLogic(ref enemyData);
        }

        private void OnApplyActionsPhase()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            
            ApplyKnockBackActions(ref enemyData);
            ApplyStunActions(ref enemyData);
            
            ApplyHealActions(ref enemyData);
            ApplyDamageActions(ref enemyData);

            CheckIsDead(ref enemyData);
        }

        private void OnNavigationPhase()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            
            if (!objectContext.HasStateAuthority || enemyData.isDead) return;

            richAI.destination = enemyData.navigationTarget;
        }

        private void OnPhysicsUpdatePhase()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();

            //todo в последний тик атаки не просчитывается стратегия, но срабатывает логика передвижения (меняется значение IsAttacking())
            if (!objectContext.HasStateAuthority || enemyData.isDead) return;

            if (CanMoveByController(ref enemyData))
            {
                if (navigationManager.IsSimulateOnCurrentTick(objectContext, out var deltaTicks))
                {
                    richAI.MovementUpdate(objectContext.Runner.DeltaTime * deltaTicks, out var nextPosition, out var nextRotation);
                    lookDirection = nextPosition - transform.position;
                    richAI.FinalizeMovement(nextPosition, nextRotation);
                    movementDeltaTicks = deltaTicks + 1; //сглаживаем возможное увеличение deltaTicks
                }

                if (movementDeltaTicks > 0)
                {
                    transform.position += (richAI.position - transform.position) / movementDeltaTicks;
                    movementDeltaTicks--;
                }
            }
            else if (movementDeltaTicks > 0)
            {
                movementDeltaTicks = 0;
                richAI.FinalizeMovement(transform.position, transform.rotation);
            }
        }

        private void OnPhysicsUnitsLookPhase()
        {
            transform.LookAt(transform.position + lookDirection);
        }

        private void OnDespawnPhase()
        {
            if (shouldDespawn)
            {
                objectContext.Runner.Despawn(objectContext);
            }

            shouldDespawn = false;
        }

        private void OnVisualStateUpdatePhase()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();

            UpdateAnimationState(ref enemyData);

            healthChangeDisplayManager?.OnAfterPhysicsSteps();
        }

        /**
         * Must be called only one time
         */
        private bool CheckIsDead(ref EnemyData enemyData)
        {
            if (enemyData.isDead)
            {
                if (objectContext.HasStateAuthority)
                {
                    eventListener.OnEnemyDead();
                    shouldDespawn = true;
                }

                return true;
            }

            return false;
        }

        public void OnUpdatedStatModifiers(StatType statType)
        {
            ref var enemyData = ref dataHolder.GetEnemyData();

            switch (statType)
            {
                case StatType.Speed:
                    enemyData.speed = effectsManager.GetModifiedValue(statType, config.DefaultSpeed);
                    break;
                case StatType.MaxHealth:
                    var newMaxHealth = effectsManager.GetModifiedValue(statType, config.DefaultMaxHealth);
                    if ((int)newMaxHealth == (int)enemyData.maxHealth)
                    {
                        healthBar.SetMaxHealth((uint)Math.Max(0, newMaxHealth));
                    }

                    enemyData.maxHealth = newMaxHealth;
                    break;
                case StatType.Damage:
                    break;
                case StatType.ReservedDoNotUse:
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }

        public void OnActiveSkillStateChanged(ActiveSkillType type, ActiveSkillState state)
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            switch (state)
            {
                case ActiveSkillState.NotAttacking:
                    break;
                case ActiveSkillState.Casting:
                    enemyData.animationTriggerId++;
                    break;
                case ActiveSkillState.Attacking:
                    enemyData.animationTriggerId++;
                    break;
                case ActiveSkillState.WaitingForPoint:
                    break;
                case ActiveSkillState.WaitingForTarget:
                    break;
                case ActiveSkillState.Finished:
                    break;
                case ActiveSkillState.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void UpdateTickLogic(ref EnemyData enemyData)
        {
            if (!objectContext.HasStateAuthority || enemyData.isDead || !CanMoveByController(ref enemyData)) return;

            var targetRef =
                enemiesHelper.FindPlayerTarget(objectContext.Runner, transform.position, out var targetPosition);
            if (targetRef != null)
            {
                enemyData.targetPlayerRef = targetRef.Value;
                var sqrDistanceToTarget = Vector3.SqrMagnitude(transform.position - targetPosition);

                if (sqrDistanceToTarget > sqrAttackDistance)
                {
                    UpdateDestination(ref enemyData, targetPosition);
                }
                else
                {
                    UpdateDestination(ref enemyData, null);
                    lookDirection = targetPosition - transform.position;
                    FireWeapon();
                }
            }
            else
            {
                UpdateDestination(ref enemyData, null);
            }
        }

        private void UpdateDestination(ref EnemyData enemyData, Vector3? destination)
        {
            enemyData.navigationTarget = destination ?? transform.position;
        }

        private void FireWeapon()
        {
            activeSkillsManager.AddActivateSkill(ActiveSkillType.PRIMARY);
        }

        private bool IsAttacking()
        {
            return activeSkillsManager.GetCurrentSkillState() == ActiveSkillState.Attacking;
        }

        public float GetMaxHealth()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            return enemyData.maxHealth;
        }

        public float GetCurrentHealth()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            return enemyData.health;
        }

        public void AddHeal(ref HealActionData data)
        {
            healActions.Add(data);
        }

        private void ApplyHealActions(ref EnemyData enemyData)
        {
            if (enemyData.isDead)
            {
                healActions.Clear();
                return;
            }

            for (var i = 0; i < healActions.Count; i++)
            {
                var actionData = healActions[i];
                ApplyHeal(ref enemyData, ref actionData);
            }

            healActions.Clear();
        }

        private void ApplyHeal(ref EnemyData enemyData, ref HealActionData actionData)
        {
            enemyData.health = Math.Min(enemyData.health + actionData.healValue, enemyData.maxHealth);
            if (healthChangeDisplayManager != null)
            {
                healthChangeDisplayManager.ApplyHeal(actionData.healValue);
            }
        }

        public void AddDamage(ref DamageActionData data)
        {
            damageActions.Add(data);
        }

        private void ApplyDamageActions(ref EnemyData enemyData)
        {
            if (enemyData.isDead)
            {
                damageActions.Clear();
                return;
            }

            for (var i = 0; i < damageActions.Count; i++)
            {
                var actionData = damageActions[i];
                ApplyDamage(ref enemyData, ref actionData);
            }

            damageActions.Clear();
        }

        private void ApplyDamage(ref EnemyData enemyData, ref DamageActionData actionData)
        {
            if (enemyData.health - actionData.damageValue < HEALTH_THRESHOLD)
            {
                enemyData.health = 0;
                enemyData.isDead = true;
            }
            else
            {
                enemyData.health -= actionData.damageValue;
            }

            if (healthChangeDisplayManager != null)
            {
                healthChangeDisplayManager.ApplyDamage(actionData.damageValue);
            }
        }

        public void AddKnockBack(ref KnockBackActionData data)
        {
            knockBackActions.Add(data);
        }

        private void ApplyKnockBackActions(ref EnemyData enemyData)
        {
            if (enemyData.isDead)
            {
                knockBackActions.Clear();
                return;
            }

            for (var i = 0; i < knockBackActions.Count; i++)
            {
                var actionData = knockBackActions[i];
                ApplyKnockBack(ref actionData);
            }

            knockBackActions.Clear();
        }

        private void ApplyKnockBack(ref KnockBackActionData actionData)
        {
            rigidbody3D.AddForce(actionData.force);
        }

        public void AddStun(ref StunActionData data)
        {
            stunActions.Add(data);
        }

        private void ApplyStunActions(ref EnemyData enemyData)
        {
            if (enemyData.isDead)
            {
                stunActions.Clear();
                return;
            }

            for (var i = 0; i < stunActions.Count; i++)
            {
                var actionData = stunActions[i];
                ApplyStun(ref enemyData, ref actionData);
            }

            stunActions.Clear();
        }

        private void ApplyStun(ref EnemyData enemyData, ref StunActionData actionData)
        {
            enemyData.stunTimer = TickTimer.CreateFromSeconds(objectContext.Runner, actionData.durationSec);
        }

        public void AddEffects(EffectsCombination effectsCombination)
        {
            effectActions.Add(effectsCombination);
        }

        private void ApplyEffects()
        {
            // if (enemyData.isDead)
            // {
            //     effectActions.Clear();
            //     return;
            // } //todo далить, если не падает

            foreach (var effectsCombination in effectActions)
            {
                effectsManager.AddEffects(effectsCombination.Effects);
            }

            effectActions.Clear();
        }

        private void UpdateAnimationState(ref EnemyData enemyData)
        {
            var newAnimationState = GetActualAnimationState(ref enemyData);

            if (lastAnimationTriggerId < enemyData.animationTriggerId)
            {
                switch (newAnimationState)
                {
                    case EnemyAnimationState.Attacking:
                        Debug.Log("Start Attacking animation");
                        meshAnimator.Play(2);
                        meshAnimator.speed = 1f;
                        break;
                }
            }

            lastAnimationTriggerId = enemyData.animationTriggerId;

            if (currentAnimationState != newAnimationState)
            {
                switch (newAnimationState)
                {
                    case EnemyAnimationState.Walking:
                        Debug.Log("Start Walking animation");
                        meshAnimator.Play(1);
                        break;
                    case EnemyAnimationState.Idle:
                        Debug.Log("Start Idle animation");
                        meshAnimator.Play(0);
                        meshAnimator.speed = 1f;
                        break;
                }
            }

            currentAnimationState = newAnimationState;
        }

        private EnemyAnimationState GetActualAnimationState(ref EnemyData enemyData)
        {
            if (activeSkillsManager.GetCurrentSkillState() == ActiveSkillState.Attacking)
            {
                return EnemyAnimationState.Attacking;
            }

            return CanMoveByController(ref enemyData) ? EnemyAnimationState.Walking : EnemyAnimationState.Idle;
        }

        private bool CanMoveByController(ref EnemyData enemyData)
        {
            return enemyData.stunTimer.ExpiredOrNotRunning(objectContext.Runner)
                   && !IsAttacking()
                   && !rigidbody3D.IsForceRunning();
        }

        public interface DataHolder :
            EffectsManager.DataHolder,
            ActiveSkillsManager.DataHolder,
            HealthChangeDisplayManager.DataHolder
        {
            public ref EnemyData GetEnemyData();
        }

        public interface EventListener
        {
            public void OnEnemyDead();
        }
    }
}