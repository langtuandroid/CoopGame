using System;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Health;
using Main.Scripts.Actions.Interaction;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Customization;
using Main.Scripts.Drop;
using Main.Scripts.Effects;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Player.Data;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
using Main.Scripts.UI.Gui;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.Player
{
    public class PlayerLogicDelegate :
        Damageable,
        Healable,
        Affectable,
        ObjectWithPickUp,
        Interactable,
        Movable,
        Dashable,
        EffectsManager.EventListener,
        ActiveSkillsManager.EventListener
    {
        private static readonly int MOVE_X_ANIM = Animator.StringToHash("MoveX");
        private static readonly int MOVE_Z_ANIM = Animator.StringToHash("MoveZ");
        private static readonly int ATTACK_ANIM = Animator.StringToHash("Attack");
        private static readonly float HEALTH_THRESHOLD = 0.01f;

        private PlayerConfig config;
        private DataHolder dataHolder;
        private EventListener eventListener;
        private NetworkObject objectContext = default!;

        private Transform transform;
        private Rigidbody rigidbody;
        private Collider collider;
        private NetworkMecanimAnimator networkAnimator;

        private ActiveSkillsManager activeSkillsManager;
        private PassiveSkillsManager passiveSkillsManager;
        private EffectsManager effectsManager;
        private CharacterCustomization characterCustomization;
        private HealthChangeDisplayManager? healthChangeDisplayManager;
        private FindTargetManager? findTargetManager;
        private UIDocument interactionInfoDoc;
        private HealthBar healthBar;

        private PlayerDataManager playerDataManager = default!;

        private int lastAnimationTriggerId;

        private InteractionInfoView interactionInfoView = default!;
        private PlayerAnimationState currentAnimationState;

        private float moveAcceleration = 200f;

        private PlayerState lastState = PlayerState.None;

        public PlayerLogicDelegate(
            ref PlayerConfig config,
            DataHolder dataHolder,
            EventListener eventListener
        )
        {
            this.config = config;
            this.dataHolder = dataHolder;
            this.eventListener = eventListener;

            transform = dataHolder.GetCachedComponent<Transform>();
            rigidbody = dataHolder.GetCachedComponent<Rigidbody>();
            collider = dataHolder.GetCachedComponent<Collider>();
            networkAnimator = dataHolder.GetCachedComponent<NetworkMecanimAnimator>();

            characterCustomization = dataHolder.GetCachedComponent<CharacterCustomization>();

            healthBar = config.HealthBar;
            interactionInfoDoc = config.InteractionInfoDoc;

            effectsManager = new EffectsManager(
                dataHolder: dataHolder,
                eventListener: this,
                effectsTarget: this
            );
            activeSkillsManager = new ActiveSkillsManager(
                config: ref config.ActiveSkillsConfig,
                dataHolder: dataHolder,
                eventListener: this,
                transform: transform
            );
            passiveSkillsManager = new PassiveSkillsManager(
                config: ref config.PassiveSkillsConfig,
                affectable: this,
                transform: transform
            );

            if (config.ShowHealthChangeDisplay)
            {
                healthChangeDisplayManager = new HealthChangeDisplayManager(
                    config: ref config.HealthChangeDisplayConfig,
                    dataHolder: dataHolder
                );
            }
        }

        public static void OnValidate(GameObject gameObject, ref PlayerConfig config)
        {
            PassiveSkillsManager.OnValidate(gameObject, ref config.PassiveSkillsConfig);
        }

        public void SetOwnerRef(PlayerRef ownerRef)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            data.ownerRef = ownerRef;
        }

        public PlayerRef GetOwnerRef()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            return data.ownerRef;
        }

        public void Spawned(NetworkObject objectContext)
        {
            this.objectContext = objectContext;

            effectsManager.Spawned(objectContext);
            activeSkillsManager.Spawned(objectContext);
            healthChangeDisplayManager?.Spawned(objectContext);

            ResetState();

            ref var data = ref dataHolder.GetPlayerLogicData();

            playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
            playerDataManager.OnPlayerDataChangedEvent.AddListener(OnPlayerDataChanged);

            var playerData = playerDataManager.GetPlayerData(data.ownerRef).ThrowWhenNull();
            ApplyCustomization(playerData.Customization);

            healthBar.SetMaxHealth((uint)Math.Max(0, data.maxHealth));


            interactionInfoView = new InteractionInfoView(interactionInfoDoc, "F", "Resurrect");

            if (HasInputAuthority(ref data))
            {
                findTargetManager = FindTargetManager.Instance.ThrowWhenNull();
            }
        }

        public void Respawn()
        {
            ResetState();
        }

        private void ResetState()
        {
            if (!objectContext.HasStateAuthority) return;

            ref var data = ref dataHolder.GetPlayerLogicData();

            activeSkillsManager.SetOwnerRef(data.ownerRef);
            passiveSkillsManager.SetOwnerRef(data.ownerRef);

            data.maxHealth = config.DefaultMaxHealth;
            data.speed = config.DefaultSpeed;

            effectsManager.ResetState();
            passiveSkillsManager.Init(); //reset after reset effectsManager

            data.health = data.maxHealth;

            UpdateState(ref data, PlayerState.Spawning);
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            effectsManager.Despawned(runner, hasState);
            activeSkillsManager.Despawned(runner, hasState);
            healthChangeDisplayManager?.Despawned(runner, hasState);
            playerDataManager.OnPlayerDataChangedEvent.RemoveListener(OnPlayerDataChanged);

            objectContext = default!;
        }

        public void Render()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            healthBar.SetHealth((uint)Math.Max(0, data.health));

            if (findTargetManager != null)
            {
                if (activeSkillsManager.GetCurrentSkillState() == ActiveSkillState.WaitingForTarget)
                {
                    if (findTargetManager.State != FindTargetState.SELECTED)
                    {
                        var targetMask = activeSkillsManager.GetSelectionTargetType();
                        if (findTargetManager.TryActivate(data.ownerRef, targetMask, out var unitTarget))
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
            
            healthChangeDisplayManager?.Render();
        }

        public void Active()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            UpdateState(ref data, PlayerState.Active);
            passiveSkillsManager.OnSpawn(data.ownerRef);
        }

        public PlayerState GetPlayerState()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            return data.state;
        }

        public void OnBeforePhysicsSteps()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            if (lastState != data.state)
            {
                OnStateChanged(ref data);
            }

            if (objectContext.Runner.IsServer)
            {
                objectContext.Runner.AddPlayerAreaOfInterest(data.ownerRef, transform.position + Vector3.forward * 5,
                    25);
            }

            effectsManager.UpdateEffects();
        }

        public void OnBeforePhysicsStep()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            if (!data.dashTimer.ExpiredOrNotRunning(objectContext.Runner))
            {
                var dashDirection = data.dashSpeed * data.dashDirection;
                Move(ref dashDirection);
            }
            else if (!CanMoveByController(ref data))
            {
                var velocity = Vector3.zero;
                Move(ref velocity);
            }
            else
            {
                ApplyDirections(ref data);
            }
        }

        public void OnAfterPhysicsSteps()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            UpdateAnimationState(ref data);
            
            healthChangeDisplayManager?.OnAfterPhysicsSteps();
        }

        public void SetDirections(ref Vector2 moveDirection, ref Vector2 aimDirection)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            data.moveDirection = moveDirection;
            data.aimDirection = aimDirection;
        }

        public Vector3 GetMovingDirection()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            return new Vector3(data.moveDirection.x, 0, data.moveDirection.y);
        }

        public void Move(ref Vector3 velocity)
        {
            rigidbody.velocity = velocity;
        }

        public void Dash(ref Vector3 direction, float speed, float durationSec)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            data.dashTimer = TickTimer.CreateFromSeconds(objectContext.Runner, durationSec);
            data.dashDirection = direction;
            data.dashSpeed = speed;
        }

        private void ApplyDirections(ref PlayerLogicData data)
        {
            if (!IsActivated(ref data))
                return;

            transform.LookAt(transform.position + new Vector3(data.aimDirection.x, 0, data.aimDirection.y));

            if (CanMoveByController(ref data))
            {
                var currentVelocity = rigidbody.velocity.magnitude;
                if (currentVelocity < data.speed)
                {
                    var deltaVelocity = data.speed * GetMovingDirection().normalized - rigidbody.velocity;
                    rigidbody.velocity +=
                        Mathf.Min(moveAcceleration * PhysicsManager.DeltaTime, deltaVelocity.magnitude) *
                        deltaVelocity.normalized;
                }
            }
        }

        private void OnPlayerDataChanged(UserId userId, PlayerData playerData, PlayerData oldPlayerData)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            if (playerDataManager.GetPlayerRef(userId) == data.ownerRef)
            {
                ApplyCustomization(playerData.Customization);
            }
        }

        private void ApplyCustomization(CustomizationData customizationData)
        {
            characterCustomization.ApplyCustomizationData(customizationData);
        }

        private bool HasInputAuthority(ref PlayerLogicData data)
        {
            return objectContext.Runner.LocalPlayer == data.ownerRef;
        }

        private bool IsActivated(ref PlayerLogicData data)
        {
            return data.state is PlayerState.Active or PlayerState.Spawning;
        }

        private bool CanMoveByController(ref PlayerLogicData data)
        {
            return !activeSkillsManager.IsCurrentSkillDisableMove() &&
                   data.dashTimer.ExpiredOrNotRunning(objectContext.Runner);
        }

        private void UpdateState(ref PlayerLogicData data, PlayerState newState)
        {
            data.state = newState;
            OnStateChanged(ref data);
        }

        private void OnStateChanged(ref PlayerLogicData data)
        {
            lastState = data.state;

            if (data.state == PlayerState.Dead)
            {
                collider.enabled = false;
                rigidbody.velocity = Vector3.zero;
            }
            else
            {
                collider.enabled = true;
            }

            eventListener.OnPlayerStateChanged(data.ownerRef, data.state);
        }

        public bool IsInteractionEnabled(PlayerRef playerRef)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            if (data.ownerRef == playerRef)
            {
                return false;
            }

            return data.state == PlayerState.Dead;
        }

        public void SetInteractionInfoVisibility(PlayerRef player, bool isVisible)
        {
            interactionInfoView.SetVisibility(isVisible);
        }

        public bool Interact(PlayerRef playerRef)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            if (data.ownerRef == playerRef)
            {
                throw new Exception("Invalid state interact");
            }

            if (data.state != PlayerState.Dead)
            {
                return false;
            }

            Respawn();
            return true;
        }

        public void ActivateSkill(ActiveSkillType type)
        {
            switch (activeSkillsManager.GetCurrentSkillState())
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
            switch (activeSkillsManager.GetCurrentSkillState())
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

        public void OnActiveSkillStateChanged(ActiveSkillType type, ActiveSkillState state)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            if (state == ActiveSkillState.Attacking)
            {
                data.animationTriggerId++;
            }
        }

        private void UpdateAnimationState(ref PlayerLogicData data)
        {
            if (objectContext.IsProxy || !objectContext.Runner.IsForward)
            {
                return;
            }

            var newAnimationState = GetActualAnimationState(ref data);

            if (lastAnimationTriggerId < data.animationTriggerId)
            {
                switch (newAnimationState)
                {
                    case PlayerAnimationState.Attacking:
                        networkAnimator.SetTrigger(ATTACK_ANIM, true);
                        break;
                }
            }

            lastAnimationTriggerId = data.animationTriggerId;
            currentAnimationState = newAnimationState;

            if (currentAnimationState == PlayerAnimationState.None)
            {
                AnimateMoving(ref data);
            }
        }

        private PlayerAnimationState GetActualAnimationState(ref PlayerLogicData data)
        {
            if (data.state != PlayerState.Active)
            {
                return PlayerAnimationState.None;
            }

            if (activeSkillsManager.GetCurrentSkillState() == ActiveSkillState.Attacking)
            {
                return PlayerAnimationState.Attacking;
            }

            return PlayerAnimationState.None;
        }

        public void OnUpdatedStatModifiers(StatType statType)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            switch (statType)
            {
                case StatType.Speed:
                    data.speed = effectsManager.GetModifiedValue(statType, config.DefaultSpeed);
                    break;
                case StatType.MaxHealth:
                    var newMaxHealth = effectsManager.GetModifiedValue(statType, config.DefaultMaxHealth);
                    if ((int)newMaxHealth != (int)data.maxHealth)
                    {
                        healthBar.SetMaxHealth((uint)Math.Max(0, newMaxHealth));
                    }

                    data.maxHealth = newMaxHealth;
                    break;
                case StatType.Damage:
                    break;
                case StatType.ReservedDoNotUse:
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }

        private void AnimateMoving(ref PlayerLogicData data)
        {
            var moveX = 0f;
            var moveZ = 0f;
            if (data.moveDirection.sqrMagnitude > 0)
            {
                var moveAngle = Vector3.SignedAngle(Vector3.forward,
                    new Vector3(data.moveDirection.x, 0, data.moveDirection.y),
                    Vector3.up);
                var lookAngle = Vector3.SignedAngle(Vector3.forward,
                    new Vector3(data.aimDirection.x, 0, data.aimDirection.y),
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
            ref var data = ref dataHolder.GetPlayerLogicData();

            return data.maxHealth;
        }

        public float GetCurrentHealth()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            return data.health;
        }

        public void ApplyDamage(float damage, NetworkObject? damageOwner)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            if (!IsActivated(ref data)) return;

            passiveSkillsManager.OnTakenDamage(data.ownerRef, damage, damageOwner);

            if (data.health - damage < HEALTH_THRESHOLD)
            {
                data.health = 0;
                if (objectContext.HasStateAuthority)
                {
                    UpdateState(ref data, PlayerState.Dead);
                    passiveSkillsManager.OnDead(data.ownerRef, damageOwner);
                }
            }
            else
            {
                data.health -= damage;
            }

            if (healthChangeDisplayManager != null)
            {
                healthChangeDisplayManager.ApplyDamage(damage);
            }
        }

        public void ApplyHeal(float healValue, NetworkObject? healOwner)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            if (!IsActivated(ref data) || data.state == PlayerState.Dead) return;

            data.health = Math.Min(data.health + healValue, data.maxHealth);

            passiveSkillsManager.OnTakenHeal(data.ownerRef, healValue, healOwner);
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
            ref var data = ref dataHolder.GetPlayerLogicData();

            switch (dropType)
            {
                case DropType.Gold:
                    data.gold += 1;
                    break;
            }
        }

        public interface DataHolder :
            EffectsManager.DataHolder,
            ActiveSkillsManager.DataHolder,
            HealthChangeDisplayManager.DataHolder
        {
            public ref PlayerLogicData GetPlayerLogicData();
        }

        public interface EventListener
        {
            public void OnPlayerStateChanged(PlayerRef owner, PlayerState state);
        }
    }
}