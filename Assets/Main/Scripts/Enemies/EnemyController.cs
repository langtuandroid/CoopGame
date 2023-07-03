using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.Architecture;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.Resources;
using Main.Scripts.Effects;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Enemies
{
    [SimulationBehaviour(
        Stages = (SimulationStages)8,
        Modes = (SimulationModes)8
    )]
    public class EnemyController : GameLoopEntity,
        InterfacesHolder,
        EnemyLogicDelegate.DataHolder,
        EnemyLogicDelegate.EventListener
    {
        private Dictionary<Type, Component> cachedComponents = new();

        [SerializeField]
        private EnemyConfig enemyConfig = EnemyConfig.GetDefault();

        [Networked]
        private ref EnemyData enemyData => ref MakeRef<EnemyData>();
        [Networked]
        private ref EffectsData effectsData => ref MakeRef<EffectsData>();
        [Networked]
        private ref ActiveSkillsData activeSkillsData => ref MakeRef<ActiveSkillsData>();
        [Networked]
        private ref HealthChangeDisplayData healthChangeDisplayData => ref MakeRef<HealthChangeDisplayData>();

        private EnemyLogicDelegate enemyLogicDelegate = default!;

        public UnityEvent<EnemyController> OnDeadEvent = default!;

        public void OnValidate()
        {
            if (GetComponent<Rigidbody>() == null)
                throw new MissingComponentException("Rigidbody component is required in EnemyController");
            if (GetComponent<NetworkTransform>() == null)
                throw new MissingComponentException("NetworkTransform component is required in EnemyController");
            if (GetComponent<Animator>() == null)
                throw new MissingComponentException("Animator component is required in EnemyController");
            
            EnemyLogicDelegate.OnValidate(gameObject, ref enemyConfig);
        }

        void Awake()
        {
            cachedComponents[typeof(Transform)] = transform;
            cachedComponents[typeof(Rigidbody)] = GetComponent<Rigidbody>();
            cachedComponents[typeof(NetworkTransform)] = GetComponent<NetworkTransform>();
            cachedComponents[typeof(NetworkMecanimAnimator)] = GetComponent<NetworkMecanimAnimator>();

            enemyLogicDelegate = new EnemyLogicDelegate(
                config: ref enemyConfig,
                dataHolder: this,
                eventListener: this
            );
        }

        public override void Spawned()
        {
            base.Spawned();
            cachedComponents[typeof(EnemiesHelper)] = EnemiesHelper.Instance.ThrowWhenNull();
            cachedComponents[typeof(EffectsBank)] = GlobalResources.Instance.ThrowWhenNull().EffectsBank;

            enemyLogicDelegate.Spawned(Object);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            cachedComponents.Remove(typeof(EnemiesHelper));
            cachedComponents.Remove(typeof(EffectsBank));
            OnDeadEvent.RemoveAllListeners();

            enemyLogicDelegate.Despawned(runner, hasState);
        }

        public override void Render()
        {
            enemyLogicDelegate.Render();
        }

        public override void OnBeforePhysicsSteps()
        {
            enemyLogicDelegate.OnBeforePhysicsSteps();
        }

        public override void OnBeforePhysicsStep()
        {
            enemyLogicDelegate.OnBeforePhysicsStep();
        }

        public override void OnAfterPhysicsSteps()
        {
            enemyLogicDelegate.OnAfterPhysicsSteps();
        }

        public T GetCachedComponent<T>() where T : Component
        {
            return (T)cachedComponents[typeof(T)];
        }

        public ref EnemyData GetEnemyData()
        {
            return ref enemyData;
        }

        public ref EffectsData GetEffectsData()
        {
            return ref effectsData;
        }
        
        public ref ActiveSkillsData GetActiveSkillsData()
        {
            return ref activeSkillsData;
        }

        public ref HealthChangeDisplayData GetHealthChangeDisplayData()
        {
            return ref healthChangeDisplayData;
        }

        public void OnEnemyDead()
        {
            OnDeadEvent.Invoke(this);
        }

        public T? GetInterface<T>()
        {
            if (enemyLogicDelegate is T typed)
            {
                return typed;
            }

            return default;
        }

        public bool TryGetInterface<T>(out T typed)
        {
            if (enemyLogicDelegate is T typedDelegate)
            {
                typed = typedDelegate;
                return true;
            }

            typed = default!;
            return false;
        }
    }
}