using System;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Interaction;
using Main.Scripts.ActiveSkills;
using Main.Scripts.Drop;
using Main.Scripts.Gui;
using Main.Scripts.UI.Gui;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Main.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : NetworkBehaviour,
        ObjectWithTakingDamage,
        ObjectWithPickUp,
        Interactable,
        Movable
    {
        private static readonly int MOVE_X_ANIM = Animator.StringToHash("MoveX");
        private static readonly int MOVE_Z_ANIM = Animator.StringToHash("MoveZ");
        private static readonly int ATTACK_ANIM = Animator.StringToHash("Attack");

        private new Rigidbody rigidbody = default!;
        private NetworkRigidbody networkRigidbody = default!;
        private new Collider collider = default!;
        private Animator animator = default!;

        [SerializeField]
        private ActiveSkillManager activeSkillManager = default!;
        [SerializeField]
        private UIDocument interactionInfoDoc = default!;

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

        private InteractionInfoView interactionInfoView = default!;

        public UnityEvent<PlayerRef, PlayerController, State> OnPlayerStateChangedEvent = default!;

        private bool isActivated => (gameObject.activeInHierarchy && (state == State.Active || state == State.Spawning));

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            networkRigidbody = GetComponent<NetworkRigidbody>();
            collider = GetComponent<Collider>();
            animator = GetComponent<Animator>();
            activeSkillManager.OnActiveSkillStateChangedEvent.AddListener(OnActiveSkillStateChanged);
        }

        public override void Spawned()
        {
            if (!IsProxy)
            {
                networkRigidbody.InterpolationDataSource = InterpolationDataSources.NoInterpolation;
            }

            interactionInfoView = new InteractionInfoView(interactionInfoDoc, "F", "Resurrect");
        }

        public void Reset()
        {
            health = maxHealth;
            healthBar.SetMaxHealth(maxHealth);

            state = State.Spawning;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            OnPlayerStateChangedEvent.RemoveAllListeners();
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

        public Vector3 GetMovingDirection()
        {
            return new Vector3(moveDirection.x, 0, moveDirection.y);
        }

        public void Move(Vector3 velocity)
        {
            rigidbody.velocity = velocity;
        }

        public void ApplyDirections()
        {
            if (!isActivated)
                return;

            transform.LookAt(transform.position + new Vector3(aimDirection.x, 0, aimDirection.y));
            
            if (!activeSkillManager.IsCurrentSkillOverrideMove())
            {
                Move(speed * new Vector3(moveDirection.x, 0, moveDirection.y));
            }
        }

        private static void OnStateChanged(Changed<PlayerController> changed)
        {
            if (changed.Behaviour)
                changed.Behaviour.OnStateChanged();
        }

        private void OnStateChanged()
        {
            if (state == State.Dead)
            {
                collider.enabled = false;
                rigidbody.velocity = Vector3.zero;
            }
            else
            {
                collider.enabled = true;
            }

            OnPlayerStateChangedEvent.Invoke(Object.InputAuthority, this, state);
        }

        public bool IsInteractionEnabled(PlayerRef playerRef)
        {
            if (Object.InputAuthority == playerRef)
            {
                return false;
            }

            return state == State.Dead;
        }

        public void SetInteractionInfoVisibility(PlayerRef player, bool isVisible)
        {
            interactionInfoView.SetVisibility(isVisible);
        }

        public bool Interact(PlayerRef playerRef)
        {
            if (Object.InputAuthority == playerRef)
            {
                throw new Exception("Invalid state interact");
            }

            if (state != State.Dead)
            {
                return false;
            }

            Reset();
            return true;
        }

        public void ActivateSkill(ActiveSkillType type)
        {
            switch (activeSkillManager.CurrentSkillState)
            {
                case ActiveSkillState.NotAttacking:
                    activeSkillManager.ActivateSkill(type);
                    break;
                case ActiveSkillState.WaitingForPoint:
                case ActiveSkillState.WaitingForTarget:
                    activeSkillManager.CancelCurrentSkill();
                    break;
            }
        }

        public void OnPrimaryButtonClicked()
        {
            switch (activeSkillManager.CurrentSkillState)
            {
                case ActiveSkillState.WaitingForPoint:
                case ActiveSkillState.WaitingForTarget:
                    activeSkillManager.ExecuteCurrentSkill();
                    break;
                case ActiveSkillState.NotAttacking:
                    ActivateSkill(ActiveSkillType.Primary);
                    break;
            }
        }

        public void ApplyMapTargetPosition(Vector2 position)
        {
            activeSkillManager.ApplyTargetMapPosition(position);
        }

        private void OnActiveSkillStateChanged(ActiveSkillType type, ActiveSkillState state)
        {
            switch (state)
            {
                case ActiveSkillState.NotAttacking:
                    break;
                case ActiveSkillState.Attacking:
                    animator.SetTrigger(ATTACK_ANIM);
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

                moveZ = (float)Math.Cos(animationAngle);
                moveX = (float)Math.Sin(animationAngle);
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
                if (HasStateAuthority)
                {
                    state = State.Dead;
                }
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