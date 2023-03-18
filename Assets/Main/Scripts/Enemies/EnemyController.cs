using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.ActiveSkills;
using Main.Scripts.Gui;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Main.Scripts.Enemies
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class EnemyController : NetworkBehaviour,
        ObjectWithTakingDamage,
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
        private ActiveSkillManager activeSkillManager = default!;
        [SerializeField]
        private HealthBar healthBar = default!;
        [SerializeField]
        private int maxHealth = 100;
        [SerializeField]
        private float speed = 5;
        [SerializeField]
        private float attackDistance = 3; //todo replace to activeWeapon parameter

        [Networked]
        private int health { get; set; }
        [Networked]
        private bool isDead { get; set; }
        [Networked]
        private Vector3 navigationTarget { get; set; }
        [Networked]
        private bool isMoving { get; set; }
        [Networked]
        private TickTimer stunTimer { get; set; }
        [Networked]
        private TickTimer knockBackTimer { get; set; }
        [Networked]
        private Vector3 knockBackDirection { get; set; }

        private NavMeshPath navMeshPath = default!;

        private bool isActivated => gameObject.activeInHierarchy && !isDead;

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            networkRigidbody = GetComponent<NetworkRigidbody>();
            animator = GetComponent<Animator>();
            navMeshPath = new NavMeshPath();
        }

        public override void Spawned()
        {
            enemiesHelper = EnemiesHelper.Instance.ThrowWhenNull();
        }

        public void ResetState()
        {
            health = maxHealth;
            isDead = false;
        }

        public override void Render()
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(health);
        }

        public override void FixedUpdateNetwork()
        {
            if (!isActivated)
            {
                return;
            }

            if (HasStateAuthority && canMoveByController())
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

            isMoving = canMoveByController() && rigidbody.velocity.magnitude > 0.01f;

            animator.SetBool(IS_MOVING_ANIM, isMoving);
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
            var direction = (navMeshPath.corners.Length > 1 ? navMeshPath.corners[1] : navigationTarget) - transform.position;
            direction = new Vector3(direction.x, 0, direction.z);
            transform.LookAt(transform.position + direction);
            var currentVelocity = rigidbody.velocity.magnitude;
            if (currentVelocity < speed)
            {
                var deltaVelocity = speed * direction.normalized - rigidbody.velocity;
                rigidbody.velocity += Mathf.Min(moveAcceleration * Runner.DeltaTime, deltaVelocity.magnitude) * deltaVelocity.normalized;
            }
        }

        private void FireWeapon()
        {
            if (activeSkillManager.ActivateSkill(ActiveSkillType.Primary))
            {
                animator.SetTrigger(ATTACK_ANIM);
            }
        }

        private bool IsAttacking()
        {
            return activeSkillManager.CurrentSkillState == ActiveSkillState.Attacking;
        }

        public void ApplyDamage(int damage)
        {
            if (!isActivated) return;

            health -= damage;

            if (health > 100 || health < 0)
            {
                health = 0;
            }

            if (health == 0)
            {
                isDead = true;
                Runner.Despawn(Object);
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

        private bool canMoveByController()
        {
            return knockBackTimer.ExpiredOrNotRunning(Runner)
                   && stunTimer.ExpiredOrNotRunning(Runner)
                   && !IsAttacking();
        }
    }
}