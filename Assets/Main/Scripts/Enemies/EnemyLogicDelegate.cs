using System;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Health;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Effects;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
using UnityEngine;
using UnityEngine.Profiling;

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
        private Rigidbody rigidbody;
        private NetworkTransform networkTransform;
        private NetworkMecanimAnimator networkAnimator;
        private HealthBar healthBar;

        private EnemiesHelper enemiesHelper = default!;

        private float knockBackForce = 30f; //todo get from ApplyKnockBack
        private float knockBackDuration = 0.1f; //todo можно высчитать из knockBackForce и rigidbody.drag
        private float moveAcceleration = 200f;

        private ActiveSkillsManager activeSkillsManager;
        private PassiveSkillsManager passiveSkillsManager;
        private EffectsManager effectsManager;
        private HealthChangeDisplayManager? healthChangeDisplayManager;

        private int lastAnimationTriggerId;

        private Vector3[] pathCorners = Array.Empty<Vector3>();
        private EnemyAnimationState currentAnimationState;

        private int nextNavPathCornerIndex;
        private float sqrAttackDistance;

        public EnemyLogicDelegate(
            ref EnemyConfig config,
            DataHolder dataHolder,
            EventListener eventListener
        )
        {
            this.config = config;
            this.dataHolder = dataHolder;
            this.eventListener = eventListener;

            rigidbody = dataHolder.GetCachedComponent<Rigidbody>();
            networkTransform = dataHolder.GetCachedComponent<NetworkTransform>();
            networkAnimator = dataHolder.GetCachedComponent<NetworkMecanimAnimator>();
            transform = dataHolder.GetCachedComponent<Transform>();

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
        }

        public void Spawned(NetworkObject objectContext)
        {
            this.objectContext = objectContext;
            ResetState();

            enemiesHelper = dataHolder.GetCachedComponent<EnemiesHelper>();

            effectsManager.Spawned(objectContext);
            activeSkillsManager.Spawned(objectContext);
            healthChangeDisplayManager?.Spawned(objectContext);

            activeSkillsManager.SetOwnerRef(objectContext.InputAuthority);
            passiveSkillsManager.SetOwnerRef(objectContext.InputAuthority);
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

            healthBar.SetMaxHealth((uint)enemyData.maxHealth);
            healthBar.SetHealth((uint)enemyData.health);
            
            healthChangeDisplayManager?.Render();
        }

        public void OnBeforePhysicsSteps()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();

            if (CheckIsDead(ref enemyData)) return;
            Profiler.BeginSample("EnemyLogicDelegate::OnBeforePhysicsSteps");

            Profiler.BeginSample("UpdateEffects");
            effectsManager.UpdateEffects();
            Profiler.EndSample();

            Profiler.BeginSample("Enemy logic");
            if (!CheckIsDead(ref enemyData) && objectContext.HasStateAuthority && CanMoveByController(ref enemyData))
            {
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
                        transform.LookAt(targetPosition);
                        FireWeapon();
                    }
                }
                else
                {
                    UpdateDestination(ref enemyData, null);
                }
            }

            Profiler.EndSample();

            Profiler.EndSample();
        }

        public void OnBeforePhysicsStep()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();

            if (CheckIsDead(ref enemyData) || !objectContext.HasStateAuthority ||
                !CanMoveByController(ref enemyData)) return;
            Profiler.BeginSample("EnemyController::OnBeforePhysicsStep");

            var curPosition = transform.position;
            var curNavigationTarget = enemyData.navigationTarget;

            enemiesHelper.StartCalculatePath(ref objectContext.Id, curPosition, curNavigationTarget);

            //Can allocate on calculate corners internal
            var newPathCorners = enemiesHelper.GetPathCorners(ref objectContext.Id);
            if (newPathCorners.Length > 0)
            {
                pathCorners = newPathCorners;
                nextNavPathCornerIndex = 1;
            }

            if (nextNavPathCornerIndex < pathCorners.Length)
            {
                if (Vector3.SqrMagnitude(pathCorners[nextNavPathCornerIndex] - transform.position) <
                    0.04f) //threshold delta
                {
                    nextNavPathCornerIndex++;
                }
            }


            var direction = (nextNavPathCornerIndex < pathCorners.Length
                    ? pathCorners[nextNavPathCornerIndex]
                    : curNavigationTarget
                ) - curPosition;

            direction = new Vector3(direction.x, 0, direction.z);
            transform.LookAt(curPosition + direction);
            var currentVelocity = rigidbody.velocity;
            if (currentVelocity.sqrMagnitude < enemyData.speed * enemyData.speed)
            {
                var deltaVelocity = enemyData.speed * direction.normalized - currentVelocity;
                var deltaVelocityMagnitude = deltaVelocity.magnitude;
                rigidbody.velocity = currentVelocity +
                                     Mathf.Min(moveAcceleration * PhysicsManager.DeltaTime / deltaVelocityMagnitude,
                                         1f) *
                                     deltaVelocity;
            }

            Profiler.EndSample();
        }

        public void OnAfterPhysicsSteps()
        {
            ref var enemyData = ref dataHolder.GetEnemyData();

            if (CheckIsDead(ref enemyData))
            {
                return;
            }

            UpdateAnimationState(ref enemyData);
            
            healthChangeDisplayManager?.OnAfterPhysicsSteps();
        }

        private bool CheckIsDead(ref EnemyData enemyData)
        {
            if (enemyData.isDead)
            {
                eventListener.OnEnemyDead();
                objectContext.Runner.Despawn(objectContext);
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

            if (destination == null)
            {
                pathCorners = Array.Empty<Vector3>();
                rigidbody.velocity = Vector3.zero;
                nextNavPathCornerIndex = 1;
            }
        }

        private void FireWeapon()
        {
            activeSkillsManager.ActivateSkill(ActiveSkillType.PRIMARY);
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

        public void ApplyHeal(float healValue, NetworkObject? healOwner)
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            if (enemyData.isDead) return;

            enemyData.health = Math.Min(enemyData.health + healValue, enemyData.maxHealth);
            if (healthChangeDisplayManager != null)
            {
                healthChangeDisplayManager.ApplyHeal(healValue);
            }
        }

        public void ApplyDamage(float damage, NetworkObject? damageOwner)
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            if (enemyData.isDead) return;

            if (enemyData.health - damage < HEALTH_THRESHOLD)
            {
                enemyData.health = 0;
                enemyData.isDead = true;
            }
            else
            {
                enemyData.health -= damage;
            }

            if (healthChangeDisplayManager != null)
            {
                healthChangeDisplayManager.ApplyDamage(damage);
            }
        }

        public void ApplyKnockBack(Vector3 direction)
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            if (enemyData.isDead) return;

            enemyData.knockBackTimer = TickTimer.CreateFromSeconds(objectContext.Runner, knockBackDuration);
            enemyData.knockBackDirection = direction;
            rigidbody.AddForce(knockBackForce * enemyData.knockBackDirection, ForceMode.Impulse);
        }

        public void ApplyStun(float durationSec)
        {
            ref var enemyData = ref dataHolder.GetEnemyData();
            if (enemyData.isDead) return;

            enemyData.stunTimer = TickTimer.CreateFromSeconds(objectContext.Runner, durationSec);
        }

        private void UpdateAnimationState(ref EnemyData enemyData)
        {
            if (objectContext.IsProxy || !objectContext.Runner.IsForward)
            {
                return;
            }


            var newAnimationState = GetActualAnimationState(ref enemyData);

            if (lastAnimationTriggerId < enemyData.animationTriggerId)
            {
                switch (newAnimationState)
                {
                    case EnemyAnimationState.Attacking:
                        networkAnimator.SetTrigger(ATTACK_ANIM, true);
                        networkAnimator.Animator.SetBool(IS_MOVING_ANIM, false);
                        break;
                }
            }

            lastAnimationTriggerId = enemyData.animationTriggerId;

            if (currentAnimationState != newAnimationState)
            {
                switch (newAnimationState)
                {
                    case EnemyAnimationState.Walking:
                        networkAnimator.Animator.SetBool(IS_MOVING_ANIM, true);
                        break;
                    case EnemyAnimationState.Idle:
                        networkAnimator.Animator.SetBool(IS_MOVING_ANIM, false);
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

            if (CanMoveByController(ref enemyData) && rigidbody.velocity.magnitude > 0.01f)
            {
                return EnemyAnimationState.Walking;
            }

            return EnemyAnimationState.Idle;
        }

        private bool CanMoveByController(ref EnemyData enemyData)
        {
            return enemyData.knockBackTimer.ExpiredOrNotRunning(objectContext.Runner)
                   && enemyData.stunTimer.ExpiredOrNotRunning(objectContext.Runner)
                   && !IsAttacking();
        }

        public void ApplyEffects(EffectsCombination effectsCombination)
        {
            effectsManager.AddEffects(effectsCombination.Effects);
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