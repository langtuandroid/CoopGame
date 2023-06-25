using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Health;
using Main.Scripts.Effects;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui;
using Main.Scripts.Levels;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Main.Scripts.Enemies
{
    public class EnemyController : NetworkBehaviour,
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
        private NetworkRigidbody networkRigidbody = default!;
        private NetworkMecanimAnimator networkAnimator = default!;
        private EnemiesHelper enemiesHelper = default!;

        private float knockBackForce = 30f; //todo get from ApplyKnockBack
        private float knockBackDuration = 0.1f; //todo можно высчитать из knockBackForce и rigidbody.drag
        private float moveAcceleration = 50f;

        [SerializeField]
        private HealthBar healthBar = default!;
        [SerializeField]
        private float defaultMaxHealth = 100;
        [SerializeField]
        private float defaultSpeed = 5;
        [SerializeField]
        private float attackDistance = 3; //todo replace to activeWeapon parameter

        private LevelContext levelContext = default!;

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

        private bool isActivated => gameObject.activeInHierarchy && !isDead;

        public UnityEvent<EnemyController> OnDeadEvent = default!;

        public void OnValidate()
        {
            if (GetComponent<Rigidbody>() == null) throw new MissingComponentException("Rigidbody component is required in EnemyController");
            if (GetComponent<NetworkRigidbody>() == null) throw new MissingComponentException("NetworkRigidbody component is required in EnemyController");
            if (GetComponent<Animator>() == null) throw new MissingComponentException("Animator component is required in EnemyController");
            if (GetComponent<ActiveSkillsManager>() == null) throw new MissingComponentException("ActiveSkillsManager component is required in EnemyController");
            if (GetComponent<PassiveSkillsManager>() == null) throw new MissingComponentException("PassiveSkillsManager component is required in EnemyController");
            if (GetComponent<EffectsManager>() == null) throw new MissingComponentException("EffectsManager component is required in EnemyController");
        }

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            networkRigidbody = GetComponent<NetworkRigidbody>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();

            activeSkillsManager = GetComponent<ActiveSkillsManager>();
            passiveSkillsManager = GetComponent<PassiveSkillsManager>();
            effectsManager = GetComponent<EffectsManager>();
            healthChangeDisplayManager = GetComponent<HealthChangeDisplayManager>();
            
            levelContext = LevelContext.Instance.ThrowWhenNull();

            activeSkillsManager.OnActiveSkillStateChangedEvent.AddListener(OnActiveSkillStateChanged);
            effectsManager.OnUpdatedStatModifiersEvent.AddListener(OnUpdatedStatModifiers);
        }

        public override void Spawned()
        {
            enemiesHelper = EnemiesHelper.Instance.ThrowWhenNull();
        }

        private void OnDestroy()
        {
            activeSkillsManager.OnActiveSkillStateChangedEvent.RemoveListener(OnActiveSkillStateChanged);
            effectsManager.OnUpdatedStatModifiersEvent.RemoveListener(OnUpdatedStatModifiers);
            
            OnDeadEvent.RemoveAllListeners();
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

        public override void FixedUpdateNetwork()
        {
            if (CheckIsDead()) return;
            if (!isActivated) return;

            effectsManager.UpdateEffects();

            if (isActivated && HasStateAuthority && canMoveByController())
            {
                var targetRef = enemiesHelper.FindPlayerTarget(transform.position);
                if (targetRef != null)
                {
                    targetPlayerRef = targetRef.Value;
                    var targetPosition = levelContext.PlayersHolder.Get(targetPlayerRef).transform.position;
                    var distanceToTarget = Vector3.Distance(transform.position, targetPosition);

                    if (distanceToTarget > attackDistance)
                    {
                        updateDestination(targetPosition);
                    }
                    else
                    {
                        updateDestination(null);
                        transform.LookAt(targetPosition);
                        FireWeapon();
                    }
                }
                else
                {
                    updateDestination(null);
                }
            }

            CheckIsDead();

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

        private void updateDestination(Vector3? destination)
        {
            navigationTarget = destination ?? transform.position;

            if (destination == null)
            {
                pathCorners.Clear();
                rigidbody.velocity = Vector3.zero;
                nextNavPathCornerIndex = 1;

                return;
            }
            
            enemiesHelper.StartCalculatePath(Object.Id, transform.position, navigationTarget);
            var newPathCorners = enemiesHelper.GetPathCorners(Object.Id);
            if (newPathCorners != null)
            {
                pathCorners.Clear();
                pathCorners.AddRange(newPathCorners);
                nextNavPathCornerIndex = 1;
            }

            if (nextNavPathCornerIndex < pathCorners.Count)
            {
                if (Vector3.Distance(pathCorners[nextNavPathCornerIndex], transform.position) < 0.2f)
                {
                    nextNavPathCornerIndex++;
                }
            }
            var direction = (nextNavPathCornerIndex < pathCorners.Count ? pathCorners[nextNavPathCornerIndex] : navigationTarget) -
                            transform.position;
            direction = new Vector3(direction.x, 0, direction.z);
            transform.LookAt(transform.position + direction);
            var currentVelocity = rigidbody.velocity.magnitude;
            if (currentVelocity < speed)
            {
                var deltaVelocity = speed * direction.normalized - rigidbody.velocity;
                rigidbody.velocity += Mathf.Min(moveAcceleration * Runner.DeltaTime, deltaVelocity.magnitude) *
                                      deltaVelocity.normalized;
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

            if (canMoveByController() && rigidbody.velocity.magnitude > 0.01f)
            {
                return EnemyAnimationState.Walking;
            }

            return EnemyAnimationState.Idle;
        }

        private bool canMoveByController()
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