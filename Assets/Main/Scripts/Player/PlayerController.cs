using System;
using System.Threading.Tasks;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Component;
using Main.Scripts.Gui;
using Main.Scripts.Room;
using Main.Scripts.Weapon;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Player
{
    public class PlayerController : NetworkBehaviour,
        ObjectWithTakingDamage
    {
        private static readonly int MOVE_X_ANIM = Animator.StringToHash("MoveX");
        private static readonly int MOVE_Z_ANIM = Animator.StringToHash("MoveZ");
        private static readonly int ATTACK_ANIM = Animator.StringToHash("Attack");

        private NetworkCharacterControllerImpl characterController;
        private Animator animator;

        [SerializeField]
        private SkillManager skillManager;
        [SerializeField]
        private int maxHealth = 100;
        [SerializeField]
        private HealthBar healthBar;

        [SerializeField]
        private float speed = 6f;

        public UnityEvent<PlayerRef> OnPlayerDeadEvent;

        [Networked(OnChanged = nameof(OnStateChanged))]
        public State state { get; private set; }
        [Networked]
        private int health { get; set; }
        [Networked]
        private Vector2 moveDirection { get; set; }
        [Networked]
        private Vector2 aimDirection { get; set; }

        public bool isActivated => (gameObject.activeInHierarchy && (state == State.Active || state == State.Spawning));

        void Awake()
        {
            characterController = GetComponent<NetworkCharacterControllerImpl>();
            animator = GetComponent<Animator>();
        }

        public override void Spawned()
        {
            health = maxHealth;
            healthBar.SetMaxHealth(maxHealth);

            state = State.Active;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            OnPlayerDeadEvent.RemoveAllListeners();
        }

        public override void Render()
        {
            healthBar.SetHealth(health);
        }

        public override void FixedUpdateNetwork()
        {
            AnimatePlayer();
        }

        public void SetDirections(Vector2 moveDirection, Vector2 aimDirection)
        {
            this.moveDirection = moveDirection;
            this.aimDirection = aimDirection;
        }

        public void Move()
        {
            if (!isActivated)
                return;

            transform.LookAt(transform.position + new Vector3(aimDirection.x, 0, aimDirection.y));
            characterController.Move(speed * new Vector3(moveDirection.x, 0, moveDirection.y));
        }

        private static void OnStateChanged(Changed<PlayerController> changed)
        {
            if (changed.Behaviour)
                changed.Behaviour.OnStateChanged();
        }

        private void OnStateChanged()
        {
            switch (state)
            {
                //todo
            }
        }

        public void ActivateSkill(SkillType skillType)
        {
            if (skillManager.ActivateSkill(skillType, Object.InputAuthority))
            {
                animator.SetTrigger(ATTACK_ANIM);
            }
        }

        private void AnimatePlayer()
        {
            var moveX = 0f;
            var moveZ = 0f;
            if (moveDirection.sqrMagnitude > 0)
            {
                var moveAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(moveDirection.x, 0, moveDirection.y),
                    Vector3.up);
                var lookAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(aimDirection.x, 0, aimDirection.y),
                    Vector3.up);
                var animationAngle = Mathf.Deg2Rad * (moveAngle - lookAngle);

                moveZ = (float) Math.Cos(animationAngle);
                moveX = (float) Math.Sin(animationAngle);
            }

            animator.SetFloat(MOVE_X_ANIM, moveX);
            animator.SetFloat(MOVE_Z_ANIM, moveZ);
        }

        public void ApplyDamage(int damage)
        {
            if (!isActivated) return;

            if (damage >= health)
            {
                health = 0;
                state = State.Dead;

                OnPlayerDeadEvent.Invoke(Object.InputAuthority);
            }
            else
            {
                health -= damage;
            }
        }

        public enum State
        {
            New,
            Despawned,
            Spawning,
            Active,
            Dead
        }
    }
}