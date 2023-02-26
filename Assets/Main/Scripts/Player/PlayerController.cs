using System;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Drop;
using Main.Scripts.Gui;
using Main.Scripts.Weapon;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : NetworkBehaviour,
        ObjectWithTakingDamage,
        ObjectWithPickUp
    {
        private static readonly int MOVE_X_ANIM = Animator.StringToHash("MoveX");
        private static readonly int MOVE_Z_ANIM = Animator.StringToHash("MoveZ");
        private static readonly int ATTACK_ANIM = Animator.StringToHash("Attack");

        private new Rigidbody rigidbody = default!;
        private Animator animator = default!;

        [SerializeField]
        private ActiveSkillManager activeSkillManager = default!;
        [SerializeField]
        private int maxHealth = 100;
        [SerializeField]
        private HealthBar healthBar = default!;

        [SerializeField]
        private float speed = 6f;

        [Networked(OnChanged = nameof(OnStateChanged))]
        public State state { get; private set; }
        [Networked]
        private int health { get; set; }
        [Networked]
        private int gold { get; set; }
        [Networked]
        private Vector2 moveDirection { get; set; }
        [Networked]
        private Vector2 aimDirection { get; set; }

        public UnityEvent<PlayerRef, PlayerController, State> OnPlayerStateChangedEvent = default!;

        private bool isActivated => (gameObject.activeInHierarchy && (state == State.Active || state == State.Spawning));

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
        }

        public override void Spawned()
        {
            health = maxHealth;
            healthBar.SetMaxHealth(maxHealth);

            state = State.Spawning;
        }

        public override void Render()
        {
            healthBar.SetHealth(health);
        }

        public void Active()
        {
            state = State.Active;
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
            rigidbody.velocity = speed * new Vector3(moveDirection.x, 0, moveDirection.y);
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

            OnPlayerStateChangedEvent.Invoke(Object.InputAuthority, this, state);
        }

        public void ActivateSkill(ActiveSkillType activeSkillType)
        {
            if (activeSkillManager.ActivateSkill(activeSkillType, Object.InputAuthority))
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
            }
            else
            {
                health -= damage;
            }
        }

        public void OnPickUp(DropType dropType)
        {
            switch (dropType)
            {
                case DropType.Gold:
                    gold += 1;
                    break;
            }
        }

        public enum State
        {
            None,
            Despawned,
            Spawning,
            Active,
            Dead
        }
    }
}