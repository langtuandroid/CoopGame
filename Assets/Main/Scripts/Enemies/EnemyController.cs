using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Component;
using Main.Scripts.Gui;
using Main.Scripts.Player;
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

        private NetworkCharacterControllerImpl characterController;
        private Animator animator;

        [SerializeField]
        private SkillManager skillManager;
        [SerializeField]
        private int maxHealth = 100;
        [SerializeField]
        private float attackDistance = 3; //todo replace to activeWeapon parameter
        [SerializeField]
        private float knockBackForce = 5f;
        [SerializeField]
        private float knockBackDuration = 0.1f;
        [SerializeField]
        private HealthBar healthBar;

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

        private NavMeshPath navMeshPath;

        private bool isActivated => gameObject.activeInHierarchy && !isDead;

        void Awake()
        {
            characterController = GetComponent<NetworkCharacterControllerImpl>();
            animator = GetComponentInChildren<Animator>();
        }

        public override void Spawned()
        {
            health = maxHealth;
            healthBar.SetMaxHealth(maxHealth);
            isDead = false;
            
            navMeshPath = new NavMeshPath();
        }
        public override void Render()
        {
            healthBar.SetHealth(health);
        }

        public override void FixedUpdateNetwork()
        {
            if (!isActivated)
            {
                return;
            }

            if (HasStateAuthority)
            {
                var targetPlayer = FindObjectOfType<PlayerController>();
                if (canMoveByController() && targetPlayer != null)
                {
                    var targetPosition = targetPlayer.transform.position;
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

            isMoving = canMoveByController() && characterController.Velocity.magnitude < characterController.Controller.minMoveDistance;

            if (!knockBackTimer.ExpiredOrNotRunning(Runner))
            {
                characterController.Move(knockBackForce * (Runner.DeltaTime / knockBackDuration) * knockBackDirection);
            }

            animator.SetBool(IS_MOVING_ANIM, isMoving);
        }

        private void updateDestination(Vector3? destination)
        {
            navigationTarget = destination ?? transform.position;
            if (destination == null) return;

            NavMesh.CalculatePath(transform.position, navigationTarget, NavMesh.AllAreas, navMeshPath);
            var direction = (navMeshPath.corners.Length > 1 ? navMeshPath.corners[1] : navigationTarget) - transform.position;
            direction = new Vector3(direction.x, 0, direction.z);
            transform.LookAt(transform.position + direction);
            characterController.Move(direction);
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