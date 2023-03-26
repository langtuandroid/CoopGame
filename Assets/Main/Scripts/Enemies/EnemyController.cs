using System;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Health;
using Main.Scripts.Effects;
using Main.Scripts.Effects.PeriodicEffects;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

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

        private new Rigidbody rigidbody = default!;
        private NetworkRigidbody networkRigidbody = default!;
        private Animator animator = default!;
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

        private ActiveSkillsManager activeSkillsManager = default!;
        private PassiveSkillsManager passiveSkillsManager = default!;
        private EffectsManager effectsManager = default!;

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

        private NavMeshPath navMeshPath = default!;
        private EnemyAnimationState currentAnimationState;

        private bool isActivated => gameObject.activeInHierarchy && !isDead;

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
            animator = GetComponent<Animator>();
            navMeshPath = new NavMeshPath();

            activeSkillsManager = GetComponent<ActiveSkillsManager>();
            passiveSkillsManager = GetComponent<PassiveSkillsManager>();
            effectsManager = GetComponent<EffectsManager>();

            effectsManager.OnUpdatedStatModifiersEvent.AddListener(OnUpdatedStatModifiers);
        }

        public override void Spawned()
        {
            enemiesHelper = EnemiesHelper.Instance.ThrowWhenNull();
        }

        private void OnDestroy()
        {
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
        }

        public override void Render()
        {
            UpdateAnimationState();
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
                var target = enemiesHelper.findPlayerTarget(transform.position);
                if (target != null)
                {
                    var targetPosition = target.Value;
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
        }

        private bool CheckIsDead()
        {
            if (isDead)
            {
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

        private void updateDestination(Vector3? destination)
        {
            navigationTarget = destination ?? transform.position;
            if (destination == null)
            {
                rigidbody.velocity = Vector3.zero;
                return;
            }

            NavMesh.CalculatePath(transform.position, navigationTarget, NavMesh.AllAreas, navMeshPath);
            var direction = (navMeshPath.corners.Length > 1 ? navMeshPath.corners[1] : navigationTarget) -
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
            activeSkillsManager.ActivateSkill(ActiveSkillType.Primary);
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

        public void ApplyHeal(float healValue)
        {
            if (!isActivated) return;

            health = Math.Min(health + healValue, maxHealth);
        }

        public void ApplyDamage(float damage)
        {
            if (!isActivated) return;

            if (damage >= health)
            {
                health = 0;
                isDead = true;
            }
            else
            {
                health -= damage;
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

        /* todo fix when attacks are called just after each other, the method might not catch the state change, and animation may be lost
        same issue with UpdateAnimationState() in PlayerController */
        private void UpdateAnimationState()
        {
            if (currentAnimationState != GetActualAnimationState())
            {
                currentAnimationState = GetActualAnimationState();
                switch (currentAnimationState)
                {
                    case EnemyAnimationState.Attacking:
                        animator.SetTrigger(ATTACK_ANIM);
                        break;
                    case EnemyAnimationState.Walking:
                        animator.SetBool(IS_MOVING_ANIM, true);
                        break;
                    case EnemyAnimationState.Idle:
                        animator.SetBool(IS_MOVING_ANIM, false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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