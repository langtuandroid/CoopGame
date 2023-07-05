using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.Architecture;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.Resources;
using Main.Scripts.Customization;
using Main.Scripts.Effects;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : GameLoopEntity,
        InterfacesHolder,
        PlayerLogicDelegate.DataHolder,
        PlayerLogicDelegate.EventListener
    {
        private Dictionary<Type, Component> cachedComponents = new();

        [SerializeField]
        private PlayerConfig playerConfig = PlayerConfig.GetDefault();

        [Networked]
        private ref PlayerLogicData playerLogicData => ref MakeRef<PlayerLogicData>();
        [Networked]
        private ref EffectsData effectsData => ref MakeRef<EffectsData>();
        [Networked]
        private ref ActiveSkillsData activeSkillsData => ref MakeRef<ActiveSkillsData>();
        [Networked]
        private ref HealthChangeDisplayData healthChangeDisplayData => ref MakeRef<HealthChangeDisplayData>();

        private PlayerLogicDelegate playerLogicDelegate = default!;

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

            PlayerLogicDelegate.OnValidate(gameObject, ref playerConfig);
        }

        void Awake()
        {
            cachedComponents[typeof(Transform)] = GetComponent<Transform>();
            cachedComponents[typeof(Rigidbody)] = GetComponent<Rigidbody>();
            cachedComponents[typeof(Collider)] = GetComponent<Collider>();
            cachedComponents[typeof(NetworkMecanimAnimator)] = GetComponent<NetworkMecanimAnimator>();

            cachedComponents[typeof(CharacterCustomizationSkinned)] = GetComponent<CharacterCustomizationSkinned>();

            playerLogicDelegate = new PlayerLogicDelegate(
                config: ref playerConfig,
                dataHolder: this,
                eventListener: this
            );
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
            cachedComponents[typeof(EffectsBank)] = GlobalResources.Instance.ThrowWhenNull().EffectsBank;
            playerLogicDelegate.Spawned(Object);
        }

        public void SetOwnerRef(PlayerRef ownerRef)
        {
            playerLogicDelegate.SetOwnerRef(ownerRef);
        }

        public PlayerRef GetOwnerRef()
        {
            return playerLogicDelegate.GetOwnerRef();
        }

        public void Respawn()
        {
            playerLogicDelegate.Respawn();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            cachedComponents.Remove(typeof(EffectsBank));

            OnPlayerStateChangedEvent.RemoveAllListeners();
            playerLogicDelegate.Despawned(runner, hasState);
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

        public override void OnBeforePhysicsSteps()
        {
            playerLogicDelegate.OnBeforePhysicsSteps();
        }

        public override void OnBeforePhysicsStep()
        {
            playerLogicDelegate.OnBeforePhysicsStep();
        }

        public override void OnAfterPhysicsSteps()
        {
            playerLogicDelegate.OnAfterPhysicsSteps();
        }

        public void SetDirections(ref Vector2 moveDirection, ref Vector2 aimDirection)
        {
            playerLogicDelegate.SetDirections(ref moveDirection, ref aimDirection);
        }

        public void ActivateSkill(ActiveSkillType type)
        {
            playerLogicDelegate.ActivateSkill(type);
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

        public ref EffectsData GetEffectsData()
        {
            return ref effectsData;
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
            if (playerLogicDelegate is T typed)
            {
                return typed;
            }

            return default;
        }

        public bool TryGetInterface<T>(out T typed)
        {
            if (playerLogicDelegate is T typedDelegate)
            {
                typed = typedDelegate;
                return true;
            }

            typed = default!;
            return false;
        }
    }
}