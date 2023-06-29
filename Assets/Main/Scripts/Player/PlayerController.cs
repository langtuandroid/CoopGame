using System;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Health;
using Main.Scripts.Actions.Interaction;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Customization;
using Main.Scripts.Drop;
using Main.Scripts.Effects;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui;
using Main.Scripts.Player.Data;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
using Main.Scripts.UI.Gui;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using CustomizationData = Main.Scripts.Player.Data.CustomizationData;

namespace Main.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : GameLoopEntity,
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
        private new Collider collider = default!;
        private NetworkMecanimAnimator networkAnimator = default!;

        private ActiveSkillsManager activeSkillsManager = default!;
        private PassiveSkillsManager passiveSkillsManager = default!;
        private EffectsManager effectsManager = default!;
        private CharacterCustomization characterCustomization = default!;
        private HealthChangeDisplayManager? healthChangeDisplayManager;
        private FindTargetManager? findTargetManager;

        private PlayerDataManager playerDataManager = default!;

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
        private PlayerRef owner { get; set; }
        
        [Networked]
        private int animationTriggerId { get; set; }
        private int lastAnimationTriggerId;

        private InteractionInfoView interactionInfoView = default!;
        private PlayerAnimationState currentAnimationState;
        
        private float moveAcceleration = 200f;
        
        private bool isActivated =>
            (gameObject.activeInHierarchy && (state == State.Active || state == State.Spawning));
        private bool hasInputAuthority => Runner.LocalPlayer == owner;

        public UnityEvent<PlayerRef, PlayerController, State> OnPlayerStateChangedEvent = default!;
        public PlayerRef Owner => owner;

        public void OnValidate()
        {
            if (GetComponent<Rigidbody>() == null) throw new MissingComponentException("Rigidbody component is required in PlayerController");
            if (GetComponent<NetworkTransform>() == null) throw new MissingComponentException("NetworkTransform component is required in PlayerController");
            if (GetComponent<Collider>() == null) throw new MissingComponentException("Collider component is required in PlayerController");
            if (GetComponent<Animator>() == null) throw new MissingComponentException("Animator component is required in PlayerController");
            if (GetComponent<ActiveSkillsManager>() == null) throw new MissingComponentException("ActiveSkillsManager component is required in PlayerController");
            if (GetComponent<PassiveSkillsManager>() == null)  throw new MissingComponentException("PassiveSkillsManager component is required in PlayerController");
            if (GetComponent<EffectsManager>() == null) throw new MissingComponentException("EffectsManager component is required in PlayerController");
        }

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            
            activeSkillsManager = GetComponent<ActiveSkillsManager>();
            passiveSkillsManager = GetComponent<PassiveSkillsManager>();
            effectsManager = GetComponent<EffectsManager>();
            characterCustomization = GetComponent<CharacterCustomization>();
            healthChangeDisplayManager = GetComponent<HealthChangeDisplayManager>();

            activeSkillsManager.OnActiveSkillStateChangedEvent.AddListener(OnActiveSkillStateChanged);
            effectsManager.OnUpdatedStatModifiersEvent.AddListener(OnUpdatedStatModifiers);
        }

        public override void Spawned()
        {
            base.Spawned();
            playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
            playerDataManager.OnPlayerDataChangedEvent.AddListener(OnPlayerDataChanged);

            var playerData = playerDataManager.GetPlayerData(owner).ThrowWhenNull();
            ApplyCustomization(playerData.Customization);
            
            healthBar.SetMaxHealth((uint)Math.Max(0, maxHealth));

            interactionInfoView = new InteractionInfoView(interactionInfoDoc, "F", "Resurrect");

            if (hasInputAuthority)
            {
                findTargetManager = FindTargetManager.Instance.ThrowWhenNull();
            }
            
            activeSkillsManager.SetOwner(owner);
            passiveSkillsManager.SetOwner(owner);
        }

        public void Init(PlayerRef owner)
        {
            this.owner = owner;
            ResetState();
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
            base.Despawned(runner, hasState);
            OnPlayerStateChangedEvent.RemoveAllListeners();
            playerDataManager.OnPlayerDataChangedEvent.RemoveListener(OnPlayerDataChanged);
        }

        public override void Render()
        {
            healthBar.SetHealth((uint)Math.Max(0, health));

            if (findTargetManager != null)
            {
                if (activeSkillsManager.CurrentSkillState == ActiveSkillState.WaitingForTarget)
                {
                    if (findTargetManager.State != FindTargetState.SELECTED)
                    {
                        var targetMask = activeSkillsManager.GetSelectionTargetType();
                        if (findTargetManager.TryActivate(Object, targetMask, out var unitTarget))
                        {
                            activeSkillsManager.ApplyUnitTarget(unitTarget);
                        }
                    }
                }
                else
                {
                    if (findTargetManager.State != FindTargetState.NOT_ACTIVE)
                    {
                        findTargetManager.StopActive(true);
                    }
                }
            }
        }

        public void Active()
        {
            state = State.Active;
            passiveSkillsManager.OnSpawn(owner);
        }

        public override void OnBeforePhysicsSteps()
        {
            if (Runner.IsServer)
            {
                Runner.AddPlayerAreaOfInterest(owner, transform.position + Vector3.forward * 5, 25);
            }
            
            effectsManager.UpdateEffects();
        }

        public override void OnBeforePhysicsStep()
        {
            if (!dashTimer.ExpiredOrNotRunning(Runner))
            {
                Move(dashSpeed * dashDirection);
            }
            else if (!CanMoveByController())
            {
                Move(Vector3.zero);
            }
            else
            {
                ApplyDirections();
            }
        }

        public override void OnAfterPhysicsSteps()
        {
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

        private void ApplyDirections()
        {
            if (!isActivated)
                return;

            transform.LookAt(transform.position + new Vector3(aimDirection.x, 0, aimDirection.y));
            
            if (CanMoveByController())
            {
                var currentVelocity = rigidbody.velocity.magnitude;
                if (currentVelocity < speed)
                {
                    var deltaVelocity = speed * GetMovingDirection().normalized - rigidbody.velocity;
                    rigidbody.velocity += Mathf.Min(moveAcceleration * PhysicsManager.DeltaTime, deltaVelocity.magnitude) *
                                          deltaVelocity.normalized;
                }
            }
        }

        private void OnPlayerDataChanged(UserId userId, PlayerData playerData, PlayerData oldPlayerData)
        {
            if (playerDataManager.GetPlayerRef(userId) == owner)
            {
                ApplyCustomization(playerData.Customization);
            }
        }

        private void ApplyCustomization(CustomizationData customizationData)
        {
            characterCustomization.ApplyCustomizationData(customizationData);
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

            OnPlayerStateChangedEvent.Invoke(owner, this, state);
        }

        public bool IsInteractionEnabled(PlayerRef playerRef)
        {
            if (owner == playerRef)
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
            if (owner == playerRef)
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
                    ActivateSkill(ActiveSkillType.PRIMARY);
                    break;
            }
        }

        public void ApplyMapTargetPosition(Vector2 position)
        {
            activeSkillsManager.ApplyTargetMapPosition(new Vector3(position.x, 0, position.y));
        }

        public void ApplyUnitTarget(NetworkId unitTargetId)
        {
            activeSkillsManager.ApplyUnitTarget(unitTargetId);
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
            
            passiveSkillsManager.OnTakenDamage(owner, damage, damageOwner);

            if (health - damage < HEALTH_THRESHOLD)
            {
                health = 0;
                if (HasStateAuthority)
                {
                    state = State.Dead;
                    passiveSkillsManager.OnDead(owner, damageOwner);
                }
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

        public void ApplyHeal(float healValue, NetworkObject? healOwner)
        {
            if (!isActivated || state == State.Dead) return;

            health = Math.Min(health + healValue, maxHealth);
            
            passiveSkillsManager.OnTakenHeal(owner, healValue, healOwner);
            if (healthChangeDisplayManager != null)
            {
                healthChangeDisplayManager.ApplyHeal(healValue);
            }
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