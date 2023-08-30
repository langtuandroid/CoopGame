using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Data;
using Main.Scripts.Actions.Health;
using Main.Scripts.Actions.Interaction;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Customization;
using Main.Scripts.Drop;
using Main.Scripts.Effects;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Player.Data;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills;
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
        ActiveSkillsManager.EventListener,
        SkillsOwner
    {
        private static readonly int MOVE_X_ANIM = Animator.StringToHash("MoveX");
        private static readonly int MOVE_Z_ANIM = Animator.StringToHash("MoveZ");
        private static readonly int ATTACK_ANIM = Animator.StringToHash("Attack");
        private static readonly float HEALTH_THRESHOLD = 0.01f;

        private PlayerConfig config;
        private DataHolder dataHolder;
        private EventListener eventListener;
        private List<SkillsOwner.Listener> listeners = new();
        private NetworkObject objectContext = default!;

        private Transform transform;
        private Rigidbody rigidbody;
        private Collider collider;
        private NetworkMecanimAnimator networkAnimator;

        private ActiveSkillsManager activeSkillsManager;
        private PassiveSkillsManager passiveSkillsManager;
        private EffectsManager effectsManager;
        private CharacterCustomizationSkinned characterCustomization;
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

        private List<DamageActionData> damageActions = new();
        private List<HealActionData> healActions = new();
        private List<EffectsCombination> effectActions = new();
        private List<DashActionData> dashActions = new();

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

            characterCustomization = dataHolder.GetCachedComponent<CharacterCustomizationSkinned>();

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

            var playerData = playerDataManager.GetPlayerData(objectContext.StateAuthority).ThrowWhenNull();
            ApplyCustomization(playerData.Customization);

            healthBar.SetMaxHealth((uint)Math.Max(0, data.maxHealth));


            interactionInfoView = new InteractionInfoView(interactionInfoDoc, "F", "Resurrect");

            if (objectContext.HasInputAuthority)
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
            damageActions.Clear();
            healActions.Clear();
            effectActions.Clear();
            dashActions.Clear();
            
            if (!objectContext.HasStateAuthority) return;

            ref var data = ref dataHolder.GetPlayerLogicData();

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
                        if (findTargetManager.TryActivate(objectContext.StateAuthority, targetMask, out var unitTarget))
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
            passiveSkillsManager.OnSpawn();
        }

        public PlayerState GetPlayerState()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            return data.state;
        }

        public void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.PlayerInputPhase:
                    break;
                case GameLoopPhase.EffectsRemoveFinishedPhase:
                    OnEffectsRemoveFinishedPhase();
                    break;
                case GameLoopPhase.SkillActivationPhase:
                    activeSkillsManager.OnGameLoopPhase(phase);
                    passiveSkillsManager.OnGameLoopPhase(phase);
                    break;
                case GameLoopPhase.EffectsApplyPhase:
                    OnEffectsApplyPhase();
                    break;
                case GameLoopPhase.EffectsUpdatePhase:
                    OnEffectsUpdatePhase();
                    break;
                case GameLoopPhase.ApplyActionsPhase:
                    OnApplyActionsPhase();
                    break;
                case GameLoopPhase.PhysicsUpdatePhase:
                    OnPhysicsUpdatePhase();
                    break;
                case GameLoopPhase.PhysicsUnitsLookPhase:
                    OnPhysicsUnitsLookPhase();
                    break;
                case GameLoopPhase.AOIUpdatePhase:
                    OnAOIUpdatePhase();
                    break;
                case GameLoopPhase.VisualStateUpdatePhase:
                    OnVisualStateUpdatePhase();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        private void OnEffectsRemoveFinishedPhase()
        {
            effectsManager.RemoveFinishedEffects();
        }

        private void OnEffectsApplyPhase()
        {
            foreach (var effectsCombination in effectActions)
            {
                effectsManager.AddEffects(effectsCombination.Effects); //todo effectActions внутрь effectsManager-а
            }
            effectActions.Clear();
        }

        private void OnEffectsUpdatePhase()
        {
            effectsManager.UpdateEffects();
        }

        private void OnApplyActionsPhase()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            ApplyDashActions(ref data);
            
            ApplyHealActions(ref data);
            ApplyDamageActions(ref data);
            
            CheckIsDead(ref data); //todo проверить как ведёт себя смена статуса
        }

        private void OnPhysicsUpdatePhase()
        {
            if (!objectContext.HasStateAuthority) return;
            
            ref var data = ref dataHolder.GetPlayerLogicData();
            
            if (!IsActivated(ref data))
                return;

            if (!data.dashTimer.ExpiredOrNotRunning(objectContext.Runner))
            {
                var dashVelocity = data.dashSpeed * data.dashDirection;
                Move(ref dashVelocity);
            }
            else if (CanMoveByController(ref data))
            {
                var moveDirection = GetMovingDirection().normalized;
                var currentToMoveVelocityDot = Vector3.Dot(rigidbody.velocity, moveDirection);

                if (currentToMoveVelocityDot < 0 || currentToMoveVelocityDot < data.speed)
                {
                    rigidbody.velocity += Math.Min(moveAcceleration, data.speed - currentToMoveVelocityDot) * moveDirection;
                }
            }
            else
            {
                var velocity = Vector3.zero;
                Move(ref velocity); //todo продумать логику остановки персонажа при условиях в CanMoveByController
            }
        }

        private void OnPhysicsUnitsLookPhase()
        {
            ref var data = ref dataHolder.GetPlayerLogicData();
            
            if (!IsActivated(ref data))
                return;
            
            if (CanMoveByController(ref data))
            {
                ApplyDirections(ref data);
            }
        }

        private void OnAOIUpdatePhase()
        {
            if (objectContext.HasStateAuthority)
            {
                objectContext.Runner.AddPlayerAreaOfInterest(objectContext.StateAuthority, transform.position + Vector3.forward * 5,
                    25);
            }
        }

        private void OnBeforePhysics()
        {
            //todo проверить нужно это или удалить
            ref var data = ref dataHolder.GetPlayerLogicData();
            if (lastState != data.state)
            {
                OnStateChanged(ref data);
            }
        }

        private void OnVisualStateUpdatePhase()
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

        private void Move(ref Vector3 velocity)
        {
            rigidbody.velocity = velocity;
        }

        public void AddDash(ref DashActionData data)
        {
            dashActions.Add(data);
        }

        private void ApplyDashActions(ref PlayerLogicData playerLogicData)
        {
            for (var i = 0; i < dashActions.Count; i++)
            {
                var actionData = dashActions[i];
                ApplyDash(ref playerLogicData, ref actionData);
            }
            dashActions.Clear();
        }

        private void ApplyDash(ref PlayerLogicData playerLogicData, ref DashActionData actionData)
        {
            playerLogicData.dashTimer = TickTimer.CreateFromSeconds(objectContext.Runner, actionData.durationSec);
            playerLogicData.dashDirection = actionData.direction;
            playerLogicData.dashSpeed = actionData.speed;
        }

        private void ApplyDirections(ref PlayerLogicData data)
        {
            transform.LookAt(transform.position + new Vector3(data.aimDirection.x, 0, data.aimDirection.y));
        }

        private void OnPlayerDataChanged(UserId userId, PlayerData playerData, PlayerData oldPlayerData)
        {
            if (playerDataManager.GetPlayerRef(userId) == objectContext.StateAuthority)
            {
                ApplyCustomization(playerData.Customization);
            }
        }

        private void ApplyCustomization(CustomizationData customizationData)
        {
            characterCustomization.ApplyCustomizationData(customizationData);
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
        
        
        private void CheckIsDead(ref PlayerLogicData playerLogicData)
        {
            if (playerLogicData.health < HEALTH_THRESHOLD)
            {
                if (objectContext.HasStateAuthority)
                {
                    UpdateState(ref playerLogicData, PlayerState.Dead);
                    //todo мы не можем гарантировать порядок нанесения урона, поэтому не можем определить кто именно нанёс смертельный удар
                    passiveSkillsManager.OnDead(null);
                }
            }
        }

        private void UpdateState(ref PlayerLogicData data, PlayerState newState)
        {
            data.state = newState;
            OnStateChanged(ref data);
        }

        private void OnStateChanged(ref PlayerLogicData data)
        {
            lastState = data.state;

            //todo переделать обновление статуса под фазы
            if (data.state == PlayerState.Dead)
            {
                collider.enabled = false;
                rigidbody.velocity = Vector3.zero;
            }
            else
            {
                collider.enabled = true;
            }

            eventListener.OnPlayerStateChanged(objectContext.StateAuthority, data.state);
        }

        public bool IsInteractionEnabled(PlayerRef playerRef)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            if (objectContext.StateAuthority == playerRef)
            {
                return false;
            }

            return data.state == PlayerState.Dead;
        }

        public void SetInteractionInfoVisibility(PlayerRef playerRef, bool isVisible)
        {
            interactionInfoView.SetVisibility(isVisible);
        }

        public void AddInteract(PlayerRef playerRef)
        {
            ref var data = ref dataHolder.GetPlayerLogicData();

            if (objectContext.InputAuthority == playerRef)
            {
                throw new Exception("Invalid state interact");
            }

            if (data.state != PlayerState.Dead)
            {
                return;
            }

            Respawn();
        }

        public void ActivateSkill(ActiveSkillType type)
        {
            switch (activeSkillsManager.GetCurrentSkillState())
            {
                case ActiveSkillState.NotAttacking:
                    activeSkillsManager.AddActivateSkill(type);
                    break;
                case ActiveSkillState.WaitingForPoint:
                case ActiveSkillState.WaitingForTarget:
                    activeSkillsManager.AddCancelCurrentSkill();
                    break;
            }
        }

        public void OnPrimaryButtonClicked()
        {
            switch (activeSkillsManager.GetCurrentSkillState())
            {
                case ActiveSkillState.WaitingForPoint:
                case ActiveSkillState.WaitingForTarget:
                    activeSkillsManager.AddExecuteCurrentSkill();
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

        public void AddDamage(ref DamageActionData data)
        {
            damageActions.Add(data);
        }

        private void ApplyDamageActions(ref PlayerLogicData playerLogicData)
        {
            if (!IsActivated(ref playerLogicData) || playerLogicData.state == PlayerState.Dead)
            {
                damageActions.Clear();
                return;
            }
            
            for (var i = 0; i < damageActions.Count; i++)
            {
                var actionData = damageActions[i];
                ApplyDamage(ref playerLogicData, ref actionData);
            }
            damageActions.Clear();
        }

        private void ApplyDamage(ref PlayerLogicData playerLogicData, ref DamageActionData actionData)
        {
            passiveSkillsManager.OnTakenDamage(actionData.damageValue, actionData.damageOwner);

            if (playerLogicData.health - actionData.damageValue < HEALTH_THRESHOLD)
            {
                playerLogicData.health = 0;
            }
            else
            {
                playerLogicData.health -= actionData.damageValue;
            }

            if (healthChangeDisplayManager != null)
            {
                healthChangeDisplayManager.ApplyDamage(actionData.damageValue);
            }
        }

        public void AddHeal(ref HealActionData data)
        {
            healActions.Add(data);
        }

        private void ApplyHealActions(ref PlayerLogicData playerLogicData)
        {
            if (!IsActivated(ref playerLogicData) || playerLogicData.state == PlayerState.Dead)
            {
                healActions.Clear();
                return;
            }

            for (var i = 0; i < healActions.Count; i++)
            {
                var actionData = healActions[i];
                ApplyHeal(ref playerLogicData, ref actionData);
            }
            healActions.Clear();
        }

        private void ApplyHeal(ref PlayerLogicData playerLogicData, ref HealActionData actionData)
        {
            playerLogicData.health = Math.Min(playerLogicData.health + actionData.healValue, playerLogicData.maxHealth);

            passiveSkillsManager.OnTakenHeal(objectContext.StateAuthority, actionData.healValue, actionData.healOwner);
            if (healthChangeDisplayManager != null)
            {
                healthChangeDisplayManager.ApplyHeal(actionData.healValue);
            }
        }

        public void AddEffects(EffectsCombination effectsCombination)
        {
            effectActions.Add(effectsCombination);
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

        public void OnSkillCooldownChanged(ActiveSkillType type, int cooldownLeftTicks)
        {
            foreach (var listener in listeners)
            {
                listener.OnActiveSkillCooldownChanged(type, cooldownLeftTicks);
            }

        }

        public void AddSkillListener(SkillsOwner.Listener listener)
        {
            listeners.Add(listener);
        }

        public void RemoveSkillListener(SkillsOwner.Listener listener)
        {
            listeners.Remove(listener);
        }

        public int GetActiveSkillCooldownLeftTicks(ActiveSkillType skillType)
        {
            return activeSkillsManager.GetSkillCooldownLeftTicks(skillType);
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