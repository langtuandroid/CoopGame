using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Data;
using Main.Scripts.Actions.Health;
using Main.Scripts.Actions.Interaction;
using Main.Scripts.Core.Architecture;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Core.Resources;
using Main.Scripts.Core.Simulation;
using Main.Scripts.Customization;
using Main.Scripts.Drop;
using Main.Scripts.Effects;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Player.Config;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.Charge;
using Main.Scripts.UI.Windows.HUD;
using Main.Scripts.Utils;
using Pathfinding.RVO;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : GameLoopEntityNetworked,
        IAfterSpawned,
        InterfacesHolder,
        PlayerLogicDelegate.DataHolder,
        PlayerLogicDelegate.EventListener,
        Damageable,
        Healable,
        ObjectWithPickUp,
        Interactable,
        Dashable
    {
        private Dictionary<Type, Component> cachedComponents = new();

        [SerializeField]
        private PlayerPrefabData playerPrefabData = PlayerPrefabData.GetDefault();

        private ReceiveTicksManager receiveTicksManager = default!;
        private EffectsBank effectsBank = default!;
        private PlayersHolder playersHolder = default!;
        private HUDScreen hudScreen = default!;

        [Networked]
        private ref PlayerLogicData playerLogicData => ref MakeRef<PlayerLogicData>();
        [Networked]
        private ref ActiveSkillsData activeSkillsData => ref MakeRef<ActiveSkillsData>();
        [Networked]
        private ref HealthChangeDisplayData healthChangeDisplayData => ref MakeRef<HealthChangeDisplayData>();
        
        private PlayerLogicDelegate playerLogicDelegate = default!;

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.SkillCheckSkillFinished,
            GameLoopPhase.PlayerInputPhase,
            GameLoopPhase.SkillActivationPhase,
            GameLoopPhase.SkillCheckCastFinished,
            GameLoopPhase.SkillSpawnPhase,
            GameLoopPhase.EffectsApplyPhase,
            GameLoopPhase.ApplyActionsPhase,
            GameLoopPhase.EffectsRemoveFinishedPhase,
            GameLoopPhase.PhysicsUpdatePhase,
            GameLoopPhase.PhysicsUnitsLookPhase,
            GameLoopPhase.AOIUpdatePhase,
            GameLoopPhase.VisualStateUpdatePhase,
        };

        public UnityEvent<PlayerRef, PlayerController, PlayerState> OnPlayerStateChangedEvent = default!;

        public void OnValidate()
        {
            if (GetComponent<Rigidbody>() == null)
                throw new MissingComponentException("Rigidbody component is required in PlayerController");
            if (GetComponent<NetworkTransform>() == null)
                throw new MissingComponentException("NetworkTransform component is required in PlayerController");
            if (GetComponent<Collider>() == null)
                throw new MissingComponentException("Collider component is required in PlayerController");
            if (GetComponent<Animator>() == null)
                throw new MissingComponentException("Animator component is required in PlayerController");
        }

        void Awake()
        {
            receiveTicksManager = GetComponent<ReceiveTicksManager>();
            
            cachedComponents[typeof(Transform)] = GetComponent<Transform>();
            cachedComponents[typeof(Rigidbody)] = GetComponent<Rigidbody>();
            cachedComponents[typeof(RVOController)] = GetComponent<RVOController>();
            cachedComponents[typeof(Collider)] = GetComponent<Collider>();
            cachedComponents[typeof(Animator)] = GetComponent<Animator>();

            cachedComponents[typeof(CharacterCustomizationSkinned)] = GetComponent<CharacterCustomizationSkinned>();

            playerLogicDelegate = new PlayerLogicDelegate(
                prefabData: ref playerPrefabData,
                dataHolder: this,
                eventListener: this
            );
        }

        public void Init(int heroConfigKey)
        {
            playerLogicData.heroConfigKey = heroConfigKey;
        }

        public new T GetComponent<T>()
        {
            if (playerLogicDelegate is T typed)
            {
                return typed;
            }

            return gameObject.GetComponent<T>();
        }

        public override void Spawned()
        {
            base.Spawned();
            var globalResources = GlobalResources.Instance.ThrowWhenNull();
            effectsBank = globalResources.EffectsBank;
            cachedComponents[typeof(EffectsBank)] = effectsBank;
            cachedComponents[typeof(HeroConfigsBank)] = globalResources.HeroConfigsBank;
            cachedComponents[typeof(SkillHeatLevelManager)] = levelContext.SkillHeatLevelManager;
            playerLogicDelegate.Spawned(Object);
            
            playersHolder = levelContext.PlayersHolder;
            hudScreen = levelContext.HudScreen;
        }

        public void AfterSpawned()
        {
            playersHolder.Add(Object.StateAuthority, this);
            if (HasStateAuthority)
            {
                hudScreen.Open();
            }
        }

        public void Respawn()
        {
            playerLogicDelegate.Respawn();
        }

        public void OnDisable()
        {
            if (HasStateAuthority && hudScreen != null)
            {
                hudScreen.Close();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            effectsBank = default!;
            cachedComponents.Remove(typeof(EffectsBank));
            cachedComponents.Remove(typeof(HeroConfigsBank));
            cachedComponents.Remove(typeof(SkillHeatLevelManager));

            OnPlayerStateChangedEvent.RemoveAllListeners();
            playerLogicDelegate.Despawned(runner, hasState);
            
            playersHolder.Remove(Object.StateAuthority);
        }

        public override void Render()
        {
            playerLogicDelegate.Render();
        }

        public void Active()
        {
            playerLogicDelegate.Active();
        }

        public PlayerState GetPlayerState()
        {
            return playerLogicDelegate.GetPlayerState();
        }

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            playerLogicDelegate.OnGameLoopPhase(phase);
        }

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
        }

        public void SetDirections(in Vector2 moveDirection, in Vector2 aimDirection)
        {
            playerLogicDelegate.SetDirections(in moveDirection, in aimDirection);
        }

        public void SkillBtnPressed(ActiveSkillType type)
        {
            playerLogicDelegate.SkillBtnPressed(type);
        }

        public void SkillBtnReleased(ActiveSkillType type)
        {
            playerLogicDelegate.SkillBtnReleased(type);
        }

        public void SkillBtnHolding(ActiveSkillType type)
        {
            playerLogicDelegate.SkillBtnHolding(type);
        }

        public void OnCancelButtonClicked()
        {
            playerLogicDelegate.OnCancelButtonClicked();
        }

        public void OnPrimaryButtonClicked()
        {
            playerLogicDelegate.OnPrimaryButtonClicked();
        }

        public void ApplyMapTargetPosition(Vector2 position)
        {
            playerLogicDelegate.ApplyMapTargetPosition(position);
        }

        public void ApplyUnitTarget(NetworkId unitTargetId)
        {
            playerLogicDelegate.ApplyUnitTarget(unitTargetId);
        }

        public T GetCachedComponent<T>() where T : Component
        {
            return (T)cachedComponents[typeof(T)];
        }

        public ref PlayerLogicData GetPlayerLogicData()
        {
            return ref playerLogicData;
        }
        
        public ref ActiveSkillsData GetActiveSkillsData()
        {
            return ref activeSkillsData;
        }

        public ref HealthChangeDisplayData GetHealthChangeDisplayData()
        {
            return ref healthChangeDisplayData;
        }

        public void OnPlayerStateChanged(PlayerRef owner, PlayerState state)
        {
            OnPlayerStateChangedEvent.Invoke(owner, this, state);
        }

        public T? GetInterface<T>()
        {
            if (this is T typedThis)
            {
                return typedThis;
            }
            
            if (playerLogicDelegate is T typed)
            {
                return typed;
            }

            if (receiveTicksManager is T receiveTicksManagerTyped)
            {
                return receiveTicksManagerTyped;
            }

            return default;
        }

        public bool TryGetInterface<T>(out T typed)
        {
            if (this is T typedThis)
            {
                typed = typedThis;
                return true;
            }
            
            if (playerLogicDelegate is T typedDelegate)
            {
                typed = typedDelegate;
                return true;
            }
            
            if (receiveTicksManager is T receiveTicksManagerTyped)
            {
                typed = receiveTicksManagerTyped;
                return true;
            }

            typed = default!;
            return false;
        }

        public float GetMaxHealth()
        {
            return playerLogicDelegate.GetMaxHealth();
        }

        public float GetCurrentHealth()
        {
            return playerLogicDelegate.GetCurrentHealth();
        }
        
        public void AddDamage(ref DamageActionData data)
        {
            RPC_AddDamage(data.damageOwner, data.damageValue);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddDamage(NetworkId damageOwnerId, float damageValue)
        {
            //todo NetworkId and get from registered <NetworkId, NetworkObject> map
            var data = new DamageActionData
            {
                damageOwner = Runner.FindObject(damageOwnerId),
                damageValue = damageValue
            };
            playerLogicDelegate.AddDamage(ref data);
        }

        public void AddHeal(ref HealActionData data)
        {
            RPC_AddHeal(data.healOwner, data.healValue);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddHeal(NetworkId healOwner, float healValue)
        {
            var data = new HealActionData
            {
                healOwner = Runner.FindObject(healOwner),
                healValue = healValue
            };
            playerLogicDelegate.AddHeal(ref data);
        }

        public void AddEffects(EffectsCombination effectsCombination)
        {
            RPC_AddEffects(effectsBank.GetEffectsCombinationId(effectsCombination));
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddEffects(int effectsCombinationId)
        {
            playerLogicDelegate.AddEffects(effectsBank.GetEffectsCombination(effectsCombinationId));
        }

        public void OnPickUp(DropType dropType)
        {
           RPC_OnPickUp(dropType);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_OnPickUp(DropType dropType)
        {
            playerLogicDelegate.OnPickUp(dropType);
        }

        public bool IsInteractionEnabled(PlayerRef playerRef)
        {
            return playerLogicDelegate.IsInteractionEnabled(playerRef);
        }

        public void SetInteractionInfoVisibility(PlayerRef playerRef, bool isVisible)
        {
            playerLogicDelegate.SetInteractionInfoVisibility(playerRef, isVisible);
        }

        public void AddInteract(PlayerRef playerRef)
        {
            RPC_AddInteract(playerRef);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddInteract(PlayerRef playerRef)
        {
            playerLogicDelegate.AddInteract(playerRef);
        }

        public void AddDash(ref DashActionData data)
        {
            RPC_AddDash(data.direction, data.speed, data.durationTicks);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddDash(Vector3 direction, float speed, int durationTicks)
        {
            var data = new DashActionData
            {
                direction = direction,
                speed = speed,
                durationTicks = durationTicks
            };
            playerLogicDelegate.AddDash(ref data);
        }
    }
}