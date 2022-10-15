using System.Collections;
using System.Linq;
using System.Numerics;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Component;
using Main.Scripts.Weapon;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

namespace Main.Scripts.Enemies
{
    public class EnemyController : NetworkBehaviour,
        ObjectWithTakingDamage,
        ObjectWithGettingKnockBack,
        ObjectWithGettingStun
    {
        private static readonly int IS_MOVING_ANIM = Animator.StringToHash("isMoving");
        private static readonly int ATTACK_ANIM = Animator.StringToHash("Attack");

        private AvoidNavMeshAgent avoidNavMeshAgent;
        private Animator animator;

        [SerializeField]
        private SkillManager skillManager;
        [SerializeField]
        private float attackDistance = 3; //todo replace to activeWeapon parameter
        [SerializeField]
        private float knockBackForce = 3f;
        [SerializeField]
        private float knockBackDuration = 0.1f;

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

        private bool isActivated => gameObject.activeInHierarchy && !isDead;

        void Awake()
        {
            avoidNavMeshAgent = GetComponent<AvoidNavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
        }

        public override void Spawned()
        {
            health = 100;
            isDead = false;
        }

        public override void FixedUpdateNetwork()
        {
            if (!isActivated)
            {
                return;
            }

            if (HasStateAuthority)
            {
                if (canMoveByController() && PlayerManager.GetFirstAlivePlayer() != null)
                {
                    var targetPosition = PlayerManager.GetFirstAlivePlayer().transform.position;
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

            isMoving = canMoveByController() && navigationTarget != default;

            if (!knockBackTimer.ExpiredOrNotRunning(Runner))
            {
                avoidNavMeshAgent.Move(knockBackForce * (Runner.Simulation.DeltaTime / knockBackDuration) * knockBackDirection);
            }

            animator.SetBool(IS_MOVING_ANIM, isMoving);
        }

        private void updateDestination(Vector3? destination)
        {
            navigationTarget = destination ?? default;
            avoidNavMeshAgent.SetDestination(destination);
        }

        private void FireWeapon()
        {
            if (skillManager.ActivateSkill(SkillType.PRIMARY, Object.StateAuthority))
            {
                animator.SetTrigger(ATTACK_ANIM);
            }
        }

        private bool IsAttacking()
        {
            return skillManager.IsSkillRunning(SkillType.PRIMARY);
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