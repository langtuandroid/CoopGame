using System;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Health;
using Main.Scripts.Actions.Interaction;
using Main.Scripts.Drop;
using Main.Scripts.Effects;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
using Main.Scripts.UI.Gui;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Main.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : NetworkBehaviour,
        Damageable,
        Healable,
        Affectable,
        ObjectWithPickUp,
        Interactable,
        Movable,
        Dashable
    {
        private static readonly int MOVE_X_ANIM = Animator.StringToHash("MoveX");
        private static readonly int MOVE_Z_ANIM = Animator.StringToHash("MoveZ");
        private static readonly int ATTACK_ANIM = Animator.StringToHash("Attack");
        private static readonly float HEALTH_THRESHOLD = 0.01f;

        private new Rigidbody rigidbody = default!;
        private NetworkRigidbody networkRigidbody = default!;
        private new Collider collider = default!;
        private NetworkMecanimAnimator networkAnimator = default!;

        private ActiveSkillsManager activeSkillsManager = default!;
        private PassiveSkillsManager passiveSkillsManager = default!;
        private EffectsManager effectsManager = default!;

        [SerializeField]
        private UIDocument interactionInfoDoc = default!;

        [SerializeField]
        private uint defaultMaxHealth = 100;
        [SerializeField]
        private HealthBar healthBar = default!;
        [SerializeField]
        private float defaultSpeed = 6f;

        [Networked(OnChanged = nameof(OnStateChanged))]
        public State state { get; private set; }
        [Networked]
        private float maxHealth { get; set; }
        [Networked]
        private float health { get; set; }
        [Networked]
        private float speed { get; set; }
        [Networked]
        private int gold { get; set; }
        [Networked]
        private Vector2 moveDirection { get; set; }
        [Networked]
        private Vector2 aimDirection { get; set; }
        [Networked]
        private TickTimer dashTimer { get; set; }
        [Networked]
        private float dashSpeed { get; set; }
        [Networked]
        private Vector3 dashDirection { get; set; }
        
        [Networked]
        private int animationTriggerId { get; set; }
        private int lastAnimationTriggerId;

        private InteractionInfoView interactionInfoView = default!;
        private PlayerAnimationState currentAnimationState;

        public UnityEvent<PlayerRef, PlayerController, State> OnPlayerStateChangedEvent = default!;

        private bool isActivated =>
            (gameObject.activeInHierarchy && (state == State.Active || state == State.Spawning));
        
        public void OnValidate()
        {
            if (GetComponent<Rigidbody>() == null) throw new MissingComponentException("Rigidbody component is required in PlayerController");
            if (GetComponent<NetworkRigidbody>() == null) throw new MissingComponentException("NetworkRigidbody component is required in PlayerController");
            if (GetComponent<Collider>() == null) throw new MissingComponentException("Collider component is required in PlayerController");
            if (GetComponent<Animator>() == null) throw new MissingComponentException("Animator component is required in PlayerController");
            if (GetComponent<ActiveSkillsManager>() == null) throw new MissingComponentException("ActiveSkillsManager component is required in PlayerController");
            if (GetComponent<PassiveSkillsManager>() == null)  throw new MissingComponentException("PassiveSkillsManager component is required in PlayerController");
            if (GetComponent<EffectsManager>() == null) throw new MissingComponentException("EffectsManager component is required in PlayerController");
        }

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            networkRigidbody = GetComponent<NetworkRigidbody>();
            collider = GetComponent<Collider>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            
            activeSkillsManager = GetComponent<ActiveSkillsManager>();
            passiveSkillsManager = GetComponent<PassiveSkillsManager>();
            effectsManager = GetComponent<EffectsManager>();

            activeSkillsManager.OnActiveSkillStateChangedEvent.AddListener(OnActiveSkillStateChanged);
            effectsManager.OnUpdatedStatModifiersEvent.AddListener(OnUpdatedStatModifiers);
        }

        public override void Spawned()
        {
            if (!IsProxy)
            {
                networkRigidbody.InterpolationDataSource = InterpolationDataSources.NoInterpolation;
            }
            healthBar.SetMaxHealth((uint)Math.Max(0, maxHealth));

            interactionInfoView = new InteractionInfoView(interactionInfoDoc, "F", "Resurrect");
        }

        public void ResetState()
        {
            maxHealth = defaultMaxHealth;
            speed = defaultSpeed;

            effectsManager.ResetState();
            passiveSkillsManager.Init();

            health = maxHealth;
            healthBar.SetMaxHealth((uint)Math.Max(0, maxHealth));

            state = State.Spawning;
        }

        private void OnDestroy()
        {
            activeSkillsManager.OnActiveSkillStateChangedEvent.RemoveListener(OnActiveSkillStateChanged);
            effectsManager.OnUpdatedStatModifiersEvent.RemoveListener(OnUpdatedStatModifiers);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            OnPlayerStateChangedEvent.RemoveAllListeners();
        }

        public override void Render()
        {
            healthBar.SetHealth((uint)Math.Max(0, health));
        }

        public void Active()
        {
            state = State.Active;
            passiveSkillsManager.OnSpawn(Object.InputAuthority);
        }

        public override void FixedUpdateNetwork()
        {
            effectsManager.UpdateEffects();

            if (!dashTimer.ExpiredOrNotRunning(Runner))
            {
                Move(dashSpeed * dashDirection);
            }
            else if (!CanMoveByController())
            {
                Move(Vector3.zero);
            }
            
            UpdateAnimationState();
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

        public void Dash(Vector3 direction, float speed, float durationSec)
        {
            dashTimer = TickTimer.CreateFromSeconds(Runner, durationSec);
            dashDirection = direction;
            dashSpeed = speed;
        }

        public void ApplyDirections()
        {
            if (!isActivated)
                return;

            transform.LookAt(transform.position + new Vector3(aimDirection.x, 0, aimDirection.y));
            
            if (CanMoveByController())
            {
                Move(speed * new Vector3(moveDirection.x, 0, moveDirection.y));
            }
        }

        private bool CanMoveByController()
        {
            return !activeSkillsManager.IsCurrentSkillDisableMove() && dashTimer.ExpiredOrNotRunning(Runner);
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

            ResetState(); //todo сделать нормальное возраждение
            return true;
        }

        public void ActivateSkill(ActiveSkillType type)
        {
            switch (activeSkillsManager.CurrentSkillState)
            {
                case ActiveSkillState.NotAttacking:
                    activeSkillsManager.ActivateSkill(type);
                    break;
                case ActiveSkillState.WaitingForPoint:
                case ActiveSkillState.WaitingForTarget:
                    activeSkillsManager.CancelCurrentSkill();
                    break;
            }
        }

        public void OnPrimaryButtonClicked()
        {
            switch (activeSkillsManager.CurrentSkillState)
            {
                case ActiveSkillState.WaitingForPoint:
                case ActiveSkillState.WaitingForTarget:
                    activeSkillsManager.ExecuteCurrentSkill();
                    break;
                case ActiveSkillState.NotAttacking:
                    ActivateSkill(ActiveSkillType.Primary);
                    break;
            }
        }

        public void ApplyMapTargetPosition(Vector2 position)
        {
            activeSkillsManager.ApplyTargetMapPosition(new Vector3(position.x, 0, position.y));
        }

        private void OnActiveSkillStateChanged(ActiveSkillType type, ActiveSkillState state)
        {
            if (state == ActiveSkillState.Attacking)
            {
                animationTriggerId++;
            }
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
                    case PlayerAnimationState.Attacking:
                        networkAnimator.SetTrigger(ATTACK_ANIM, true);
                        break;
                }
            }
            
            lastAnimationTriggerId = animationTriggerId;
            currentAnimationState = newAnimationState;
            
            if (currentAnimationState == PlayerAnimationState.None)
            {
                AnimateMoving();
            }
        }

        private PlayerAnimationState GetActualAnimationState()
        {
            if (state != State.Active)
            {
                return PlayerAnimationState.None;
            }
            if (activeSkillsManager.CurrentSkillState == ActiveSkillState.Attacking)
            {
                return PlayerAnimationState.Attacking;
            }

            return PlayerAnimationState.None;
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
                    if ((int)newMaxHealth != (int)maxHealth)
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

        private void AnimateMoving()
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

            networkAnimator.Animator.SetFloat(MOVE_X_ANIM, moveX);
            networkAnimator.Animator.SetFloat(MOVE_Z_ANIM, moveZ);
        }

        public float GetMaxHealth()
        {
            return maxHealth;
        }

        public float GetCurrentHealth()
        {
            return health;
        }

        public void ApplyDamage(float damage, NetworkObject? damageOwner)
        {
            if (!isActivated) return;
            
            passiveSkillsManager.OnTakenDamage(Object.InputAuthority, damage, damageOwner);

            if (health - damage < HEALTH_THRESHOLD)
            {
                health = 0;
                if (HasStateAuthority)
                {
                    state = State.Dead;
                    passiveSkillsManager.OnDead(Object.InputAuthority, damageOwner);
                }
            }
            else
            {
                health -= damage;
            }
        }

        public void ApplyHeal(float healValue, NetworkObject? healOwner)
        {
            if (!isActivated || state == State.Dead) return;

            health = Math.Min(health + healValue, maxHealth);
            
            passiveSkillsManager.OnTakenHeal(Object.InputAuthority, healValue, healOwner);
        }
        
        public void ApplyEffects(EffectsCombination effectsCombination)
        {
            effectsManager.AddEffects(effectsCombination.Effects);
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