using System;
using System.Collections.Generic;
using FSG.MeshAnimator.ShaderAnimated;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Data;
using Main.Scripts.Actions.Health;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Effects;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Mobs.Component;
using Main.Scripts.Mobs.Component.Delegate;
using Main.Scripts.Mobs.Config;
using Main.Scripts.Mobs.Config.Block.Action;
using Main.Scripts.Skills;
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
        ActiveSkillsManager.EventListener,
        SkillsOwner
    {
        private static readonly float HEALTH_THRESHOLD = 0.01f;

        private DataHolder dataHolder;
        private EventListener eventListener;

        private Transform transform;
        private NetworkRigidbody3D rigidbody3D;
        private RichAI richAI;
        private MeshFilter meshFilter;
        private ShaderMeshAnimator meshAnimator;
        
        private ActiveSkillsManager activeSkillsManager;
        private PassiveSkillsManager passiveSkillsManager;
        private EffectsManager effectsManager;

        private MobConfig mobConfig = null!;
        private NetworkObject objectContext = null!;
        
        private NavigationManager navigationManager = null!;
        private MobConfigsBank mobConfigsBank = null!;
        private MobBlockDelegate logicBlockDelegate = null!;

        private int lastAnimationTriggerId;
        private EnemyAnimationState currentAnimationState;

        private Vector3 lookDirection;
        private int movementDeltaTicks;
        private bool shouldDespawn;

        private List<KnockBackActionData> knockBackActions = new();
        private List<StunActionData> stunActions = new();
        private List<DamageActionData> damageActions = new();
        private List<HealActionData> healActions = new();
        private List<EffectsCombination> effectActions = new();

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
            DataHolder dataHolder,
            EventListener eventListener
        )
        {
            this.dataHolder = dataHolder;
            this.eventListener = eventListener;

            rigidbody3D = dataHolder.GetCachedComponent<NetworkRigidbody3D>();
            richAI = dataHolder.GetCachedComponent<RichAI>();
            meshFilter = dataHolder.GetCachedComponent<MeshFilter>();
            meshAnimator = dataHolder.GetCachedComponent<ShaderMeshAnimator>();
            transform = dataHolder.GetCachedComponent<Transform>();

            richAI.updatePosition = false;

            effectsManager = new EffectsManager(
                dataHolder: dataHolder,
                eventListener: this,
                effectsTarget: this
            );
            activeSkillsManager = new ActiveSkillsManager(
                dataHolder: dataHolder,
                eventListener: this,
                transform
            );
            passiveSkillsManager = new PassiveSkillsManager(
                dataHolder: dataHolder,
                transform: transform
            );
        }

        public void Spawned(NetworkObject objectContext)
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            this.objectContext = objectContext;

            navigationManager = dataHolder.GetCachedComponent<NavigationManager>();
            mobConfigsBank = dataHolder.GetCachedComponent<MobConfigsBank>();
            
            mobConfig = mobConfigsBank.GetMobConfig(enemyData.mobConfigKey);
            
            effectsManager.Spawned(objectContext);
            activeSkillsManager.Spawned(
                objectContext: objectContext,
                isPlayerOwner: false,
                config: ref mobConfig.ActiveSkillsConfig
            );
            passiveSkillsManager.Spawned(
                objectContext: objectContext,
                isPlayerOwner: false,
                config: ref mobConfig.PassiveSkillsConfig
            );
            
            logicBlockDelegate = MobBlockDelegateHelper.Create(mobConfig.LogicBlockConfig);

            meshFilter.sharedMesh = mobConfig.MobMesh;
            meshAnimator.SetAnimations(mobConfig.AnimationsArray);

            richAI.enabled = objectContext.HasStateAuthority;
            if (this.objectContext.HasStateAuthority)
            {
                richAI.Teleport(transform.position);
            }

            InitState(ref enemyData);
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            ResetState();

            effectsManager.Despawned(runner, hasState);
            activeSkillsManager.Despawned(runner, hasState);
            passiveSkillsManager.Despawned(runner, hasState);
            
            MobBlockDelegateHelper.Release(logicBlockDelegate);
            logicBlockDelegate = null!;

            objectContext = null!;
        }

        private void InitState(ref EnemyData enemyData)
        {
            if (!objectContext.HasStateAuthority) return;
            
            enemyData.maxHealth = mobConfig.MaxHealth;
            enemyData.speed = mobConfig.MoveSpeed;
            enemyData.health = enemyData.maxHealth;
            enemyData.isDead = false;

            lookDirection = transform.rotation * Vector3.forward;
            
            passiveSkillsManager.ApplyInitialEffects();
        }

        private void ResetState()
        {
            mobConfig = null!;
            knockBackActions.Clear();
            stunActions.Clear();
            damageActions.Clear();
            healActions.Clear();
            effectActions.Clear();
            shouldDespawn = false;
            movementDeltaTicks = default;
            lastAnimationTriggerId = default;
            currentAnimationState = EnemyAnimationState.None;

            //если нужно поддержать респавн, то нужно добавить сюда ресет стейтов у менеджеров
        }

        public void Render()
        {
            switch (currentAnimationState)
            {
                case EnemyAnimationState.Walking:
                    var velocity = rigidbody3D.VelocityInterpolated.magnitude / mobConfig.MoveSpeed;
                    meshAnimator.speed = velocity;
                    break;
            }

            activeSkillsManager.Render();
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

            if (!objectContext.HasStateAuthority || enemyData.isDead || !CanMoveByController(ref enemyData)) return;

            var blockContext = new MobBlockContext
            {
                SelfUnit = objectContext,
                Tick = objectContext.Runner.Tick,
                AlliesLayerMask = mobConfig.AlliesLayerMask,
                OpponentsLayerMask = mobConfig.OpponentsLayerMask
            };
            logicBlockDelegate.Do(ref blockContext, out var blockResult);
            
            var target = blockResult.blockContext.TargetUnit;
            switch (blockResult.actionConfig)
            {
                case DoNothingMobActionBlock:
                    UpdateDestination(ref enemyData, null);
                    break;
                case ActivateSkillMobActionBlock activateSkillAction:
                    if (target != null)
                    {
                        lookDirection = target.transform.position - transform.position;
                        activeSkillsManager.ApplyUnitTarget(target.Id);
                    }
                    activeSkillsManager.AddActivateSkill(activateSkillAction.SkillType, true);
                    break;
                case MoveToTargetMobActionBlock:
                    if (target == null)
                    {
                        throw new Exception($"Target in null after logic blocks calculating in {mobConfig.name}");
                    }
                    UpdateDestination(ref enemyData, target.transform.position);
                    break;
                case MoveToPointMobActionBlock:
                    //todo
                    // UpdateDestination(ref enemyData, target.transform.position);
                    break;
            }
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
            
            if (!objectContext.HasStateAuthority || enemyData.isDead || !enemyData.hasNavTarget) return;

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
                    enemyData.speed = effectsManager.GetModifiedValue(statType, mobConfig.MoveSpeed);
                    break;
                case StatType.MaxHealth:
                    var newMaxHealth = effectsManager.GetModifiedValue(statType, mobConfig.MaxHealth);

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

        private void UpdateDestination(ref EnemyData enemyData, Vector3? destination)
        {
            enemyData.navigationTarget = destination ?? transform.position;
            enemyData.hasNavTarget = destination != null;
            if (!enemyData.hasNavTarget)
            {
                richAI.SetPath(null);
            }
        }

        private bool IsAttacking()
        {
            return activeSkillsManager.GetCurrentSkillState() != ActiveSkillState.NotAttacking;
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
            enemyData.stunTimer = TickTimer.CreateFromTicks(objectContext.Runner, actionData.durationTicks);
        }

        public void AddEffects(EffectsCombination effectsCombination)
        {
            effectActions.Add(effectsCombination);
        }

        private void ApplyEffects()
        {
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
                        if (mobConfig.ActiveSkillAnimationsMap.ContainsKey(ActiveSkillType.PRIMARY))
                        {
                            var animation = mobConfig.ActiveSkillAnimationsMap[ActiveSkillType.PRIMARY].ExecutionSkillAnimation;
                            if (animation != null)
                            {
                                meshAnimator.speed = 1f;
                                meshAnimator.Play(mobConfig.AnimationIndexMap[animation]);
                            }
                        }
                        break;
                    case EnemyAnimationState.Casting:
                        if (mobConfig.ActiveSkillAnimationsMap.ContainsKey(ActiveSkillType.PRIMARY))
                        {
                            var animation = mobConfig.ActiveSkillAnimationsMap[ActiveSkillType.PRIMARY].CastSkillAnimation;
                            if (animation != null)
                            {
                                meshAnimator.speed = 1f;
                                meshAnimator.Play(mobConfig.AnimationIndexMap[animation]);
                            }
                        }
                        break;
                }
            }

            lastAnimationTriggerId = enemyData.animationTriggerId;

            if (currentAnimationState != newAnimationState)
            {
                switch (newAnimationState)
                {
                    case EnemyAnimationState.Walking:
                        if (mobConfig.Animations.RunAnimation != null)
                        {
                            meshAnimator.Play(mobConfig.AnimationIndexMap[mobConfig.Animations.RunAnimation]);
                        }
                        break;
                    case EnemyAnimationState.Idle:
                        if (mobConfig.Animations.IdleAnimation != null)
                        {
                            meshAnimator.speed = 1f;
                            meshAnimator.Play(mobConfig.AnimationIndexMap[mobConfig.Animations.IdleAnimation]);
                        }
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

            if (activeSkillsManager.GetCurrentSkillState() == ActiveSkillState.Casting)
            {
                return EnemyAnimationState.Casting;
            }

            return CanMoveByController(ref enemyData) && enemyData.hasNavTarget
                ? EnemyAnimationState.Walking
                : EnemyAnimationState.Idle;
        }

        private bool CanMoveByController(ref EnemyData enemyData)
        {
            return enemyData.stunTimer.ExpiredOrNotRunning(objectContext.Runner)
                   && !IsAttacking()
                   && !rigidbody3D.IsForceRunning();
        }

        public int GetActiveSkillCooldownLeftTicks(ActiveSkillType skillType)
        {
            return activeSkillsManager.GetSkillCooldownLeftTicks(skillType);
        }

        public bool CanActivateSkill(ActiveSkillType skillType)
        {
            return activeSkillsManager.GetCurrentSkillState() == ActiveSkillState.NotAttacking
                && GetActiveSkillCooldownLeftTicks(skillType) == 0;
        }

        public void AddSkillListener(SkillsOwner.Listener listener) { }

        public void RemoveSkillListener(SkillsOwner.Listener listener) { }

        public interface DataHolder :
            EffectsManager.DataHolder,
            ActiveSkillsManager.DataHolder,
            HealthChangeDisplayManager.DataHolder,
            PassiveSkillsManager.DataHolder
        {
            public ref EnemyData GetEnemyData();
        }

        public interface EventListener
        {
            public void OnEnemyDead();
        }
    }
}