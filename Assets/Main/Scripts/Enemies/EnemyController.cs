using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Health;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Effects;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace Main.Scripts.Enemies
{
    [SimulationBehaviour(
        Stages = (SimulationStages) 8,
        Modes  = (SimulationModes) 8
    )]
    public class EnemyController : GameLoopEntity,
        Damageable,
        Healable,
        Affectable,
        ObjectWithGettingKnockBack,
        ObjectWithGettingStun
    {
        private static readonly int IS_MOVING_ANIM = Animator.StringToHash("isMoving");
        private static readonly int ATTACK_ANIM = Animator.StringToHash("Attack");
        private static readonly float HEALTH_THRESHOLD = 0.01f;

        private new Rigidbody rigidbody = default!;
        private NetworkTransform networkTransform = default!;
        private NetworkMecanimAnimator networkAnimator = default!;
        private EnemiesHelper enemiesHelper = default!;
        private Transform cachedTransform = default!;

        private float knockBackForce = 30f; //todo get from ApplyKnockBack
        private float knockBackDuration = 0.1f; //todo можно высчитать из knockBackForce и rigidbody.drag
        private float moveAcceleration = 200f;

        [SerializeField]
        private HealthBar healthBar = default!;
        [SerializeField]
        private float defaultMaxHealth = 100;
        [SerializeField]
        private float defaultSpeed = 5;
        [SerializeField]
        private float attackDistance = 3; //todo replace to activeWeapon parameter

        private ActiveSkillsManager activeSkillsManager = default!;
        private PassiveSkillsManager passiveSkillsManager = default!;
        private EffectsManager effectsManager = default!;
        private HealthChangeDisplayManager? healthChangeDisplayManager;

        [Networked]
        private float health { get; set; }
        [Networked]
        private float maxHealth { get; set; }
        [Networked]
        private float speed { get; set; }
        [Networked]
        private bool isDead { get; set; }
        [Networked]
        private Vector3 navigationTarget { get; set; }
        [Networked]
        private TickTimer stunTimer { get; set; }
        [Networked]
        private TickTimer knockBackTimer { get; set; }
        [Networked]
        private Vector3 knockBackDirection { get; set; }
        [Networked]
        private PlayerRef targetPlayerRef { get; set; }
        
        [Networked]
        private int animationTriggerId { get; set; }
        private int lastAnimationTriggerId;

        private List<Vector3> pathCorners = new();
        private EnemyAnimationState currentAnimationState;

        private int nextNavPathCornerIndex;
        private float sqrAttackDistance;

        private bool isActivated => gameObject.activeInHierarchy && !isDead;

        public UnityEvent<EnemyController> OnDeadEvent = default!;

        public void OnValidate()
        {
            if (GetComponent<Rigidbody>() == null) throw new MissingComponentException("Rigidbody component is required in EnemyController");
            if (GetComponent<NetworkTransform>() == null) throw new MissingComponentException("NetworkTransform component is required in EnemyController");
            if (GetComponent<Animator>() == null) throw new MissingComponentException("Animator component is required in EnemyController");
            if (GetComponent<ActiveSkillsManager>() == null) throw new MissingComponentException("ActiveSkillsManager component is required in EnemyController");
            if (GetComponent<PassiveSkillsManager>() == null) throw new MissingComponentException("PassiveSkillsManager component is required in EnemyController");
            if (GetComponent<EffectsManager>() == null) throw new MissingComponentException("EffectsManager component is required in EnemyController");
        }

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            networkTransform = GetComponent<NetworkTransform>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            cachedTransform = transform;

            activeSkillsManager = GetComponent<ActiveSkillsManager>();
            passiveSkillsManager = GetComponent<PassiveSkillsManager>();
            effectsManager = GetComponent<EffectsManager>();
            healthChangeDisplayManager = GetComponent<HealthChangeDisplayManager>();

            activeSkillsManager.OnActiveSkillStateChangedEvent.AddListener(OnActiveSkillStateChanged);
            effectsManager.OnUpdatedStatModifiersEvent.AddListener(OnUpdatedStatModifiers);

            sqrAttackDistance = attackDistance * attackDistance;
        }

        public override void Spawned()
        {
            base.Spawned();
            enemiesHelper = EnemiesHelper.Instance.ThrowWhenNull();
            
            activeSkillsManager.SetOwner(Object.InputAuthority);
            passiveSkillsManager.SetOwner(Object.InputAuthority);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            enemiesHelper = default!;
            OnDeadEvent.RemoveAllListeners();
        }

        private void OnDestroy()
        {
            activeSkillsManager.OnActiveSkillStateChangedEvent.RemoveListener(OnActiveSkillStateChanged);
            effectsManager.OnUpdatedStatModifiersEvent.RemoveListener(OnUpdatedStatModifiers);
        }
        
        public void ResetState()
        {
            maxHealth = defaultMaxHealth;
            speed = defaultSpeed;

            effectsManager.ResetState();
            passiveSkillsManager.Init();

            health = maxHealth;
            healthBar.SetMaxHealth((uint)Math.Max(0, maxHealth));

            isDead = false;
            currentAnimationState = EnemyAnimationState.None;
        }

        public override void Render()
        {
            healthBar.SetMaxHealth((uint)maxHealth);
            healthBar.SetHealth((uint)health);
        }

        public override void OnBeforePhysicsSteps()
        {
            if (CheckIsDead()) return;
            if (!isActivated) return;
            Profiler.BeginSample("EnemiesController::OnBeforePhysicsSteps");

            Profiler.BeginSample("UpdateEffects");
            effectsManager.UpdateEffects();
            Profiler.EndSample();

            Profiler.BeginSample("Enemy logic");
            if (isActivated && HasStateAuthority && CanMoveByController())
            {
                var targetRef = enemiesHelper.FindPlayerTarget(Runner, transform.position, out var targetPosition);
                if (targetRef != null)
                {
                    targetPlayerRef = targetRef.Value;
                    var sqrDistanceToTarget = Vector3.SqrMagnitude(transform.position - targetPosition);

                    if (sqrDistanceToTarget > sqrAttackDistance)
                    {
                        UpdateDestination(targetPosition);
                    }
                    else
                    {
                        UpdateDestination(null);
                        transform.LookAt(targetPosition);
                        FireWeapon();
                    }
                }
                else
                {
                    UpdateDestination(null);
                }
            }
            Profiler.EndSample();
            
            Profiler.EndSample();
        }
        
        public override void OnBeforePhysicsStep()
        {
            if (!isActivated || !HasStateAuthority || !CanMoveByController()) return;
            Profiler.BeginSample("EnemyController::OnBeforePhysicsStep");

            var curPosition = cachedTransform.position;
            var curNavigationTarget = navigationTarget;

            enemiesHelper.StartCalculatePath(ref Object.Id, curPosition, curNavigationTarget);
            
            var newPathCorners = enemiesHelper.GetPathCorners(ref Object.Id);
            if (newPathCorners != null)
            {
                pathCorners.Clear();
                pathCorners.AddRange(newPathCorners);
                nextNavPathCornerIndex = 1;
            }

            if (nextNavPathCornerIndex < pathCorners.Count)
            {
                if (Vector3.SqrMagnitude(pathCorners[nextNavPathCornerIndex] - cachedTransform.position) < 0.04f) //threshold delta
                {
                    nextNavPathCornerIndex++;
                }
            }

            
            var direction = (nextNavPathCornerIndex < pathCorners.Count
                    ? pathCorners[nextNavPathCornerIndex]
                    : curNavigationTarget
                ) - curPosition;
            
            direction = new Vector3(direction.x, 0, direction.z);
            cachedTransform.LookAt(curPosition + direction);
            var currentVelocity = rigidbody.velocity;
            if (currentVelocity.sqrMagnitude < speed * speed)
            {
                var deltaVelocity = speed * direction.normalized - currentVelocity;
                var deltaVelocityMagnitude = deltaVelocity.magnitude;
                rigidbody.velocity = currentVelocity + Mathf.Min(moveAcceleration * PhysicsManager.DeltaTime / deltaVelocityMagnitude, 1f) *
                                      deltaVelocity;
            }
            
            Profiler.EndSample();
        }

        public override void OnAfterPhysicsSteps()
        {
            if (CheckIsDead())
            {
                return;
            }

            UpdateAnimationState();
        }

        private bool CheckIsDead()
        {
            if (isDead)
            {
                OnDeadEvent.Invoke(this);
                Runner.Despawn(Object);
                return true;
            }

            return false;
        }

        private void OnUpdatedStatModifiers(StatType statType)
        {
            switch (statType)
            {
                case StatType.Speed:
                    speed = effectsManager.GetModifiedValue(statType, defaultSpeed);
                    break;
                case StatType.MaxHealth:
                    var newMaxHealth = effectsManager.GetModifiedValue(statType, defaultMaxHealth);
                    if ((int)newMaxHealth == (int)maxHealth)
                    {
                        healthBar.SetMaxHealth((uint)Math.Max(0, newMaxHealth));
                    }

                    maxHealth = newMaxHealth;
                    break;
                case StatType.Damage:
                    break;
                case StatType.ReservedDoNotUse:
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }

        private void OnActiveSkillStateChanged(ActiveSkillType type, ActiveSkillState state)
        {
            switch (state)
            {
                case ActiveSkillState.NotAttacking:
                    break;
                case ActiveSkillState.Attacking:
                    animationTriggerId++;
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

        private void UpdateDestination(Vector3? destination)
        {
            navigationTarget = destination ?? transform.position;

            if (destination == null)
            {
                pathCorners.Clear();
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
            return activeSkillsManager.CurrentSkillState == ActiveSkillState.Attacking;
        }

        public float GetMaxHealth()
        {
            return maxHealth;
        }

        public float GetCurrentHealth()
        {
            return health;
        }

        public void ApplyHeal(float healValue, NetworkObject? healOwner)
        {
            if (!isActivated) return;

            health = Math.Min(health + healValue, maxHealth);
            if (healthChangeDisplayManager != null)
            {
                healthChangeDisplayManager.ApplyHeal(healValue);
            }
        }

        public void ApplyDamage(float damage, NetworkObject? damageOwner)
        {
            if (!isActivated) return;

            if (health - damage < HEALTH_THRESHOLD)
            {
                health = 0;
                isDead = true;
            }
            else
            {
                health -= damage;
            }

            if (healthChangeDisplayManager != null)
            {
                healthChangeDisplayManager.ApplyDamage(damage);
            }
        }

        public void ApplyKnockBack(Vector3 direction)
        {
            if (!isActivated) return;
            knockBackTimer = TickTimer.CreateFromSeconds(Runner, knockBackDuration);
            knockBackDirection = direction;
            rigidbody.AddForce(knockBackForce * knockBackDirection, ForceMode.Impulse);
        }

        public void ApplyStun(float durationSec)
        {
            if (!isActivated) return;
            stunTimer = TickTimer.CreateFromSeconds(Runner, durationSec);
        }

        private void UpdateAnimationState()
        {
            if (IsProxy || !Runner.IsForward)
            {
                return;
            }
            
            var newAnimationState = GetActualAnimationState();

            if (lastAnimationTriggerId < animationTriggerId)
            {
                switch (newAnimationState)
                {
                    case EnemyAnimationState.Attacking:
                        networkAnimator.SetTrigger(ATTACK_ANIM, true);
                        networkAnimator.Animator.SetBool(IS_MOVING_ANIM, false);
                        break;
                }
            }
            lastAnimationTriggerId = animationTriggerId;

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

        private EnemyAnimationState GetActualAnimationState()
        {
            if (activeSkillsManager.CurrentSkillState == ActiveSkillState.Attacking)
            {
                return EnemyAnimationState.Attacking;
            }

            if (CanMoveByController() && rigidbody.velocity.magnitude > 0.01f)
            {
                return EnemyAnimationState.Walking;
            }

            return EnemyAnimationState.Idle;
        }

        private bool CanMoveByController()
        {
            return knockBackTimer.ExpiredOrNotRunning(Runner)
                   && stunTimer.ExpiredOrNotRunning(Runner)
                   && !IsAttacking();
        }

        public void ApplyEffects(EffectsCombination effectsCombination)
        {
            effectsManager.AddEffects(effectsCombination.Effects);
        }
    }
}