using System;
using System.Collections.Generic;
using FSG.MeshAnimator.ShaderAnimated;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Data;
using Main.Scripts.Actions.Health;
using Main.Scripts.Core.Architecture;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Core.Resources;
using Main.Scripts.Effects;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Mobs.Config;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.Charge;
using Main.Scripts.Utils;
using Pathfinding;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Enemies
{
    [SimulationBehaviour(
        Stages = (SimulationStages)8,
        Modes = (SimulationModes)8
    )]
    public class EnemyController : GameLoopEntityNetworked,
        InterfacesHolder,
        EnemyLogicDelegate.DataHolder,
        EnemyLogicDelegate.EventListener,
        Damageable,
        Healable,
        ObjectWithGettingKnockBack,
        ObjectWithGettingStun
    {
        private Dictionary<Type, Component> cachedComponents = new();

        [SerializeField]
        private MeshFilter meshFilter = null!;
        [SerializeField]
        private ShaderMeshAnimator shaderMeshAnimator = null!;

        private EffectsBank effectsBank = null!;
        private MobConfigsBank mobConfigsBank = null!;

        [Networked]
        private ref EnemyData enemyData => ref MakeRef<EnemyData>();
        [Networked]
        private ref ActiveSkillsData activeSkillsData => ref MakeRef<ActiveSkillsData>();
        [Networked]
        private ref HealthChangeDisplayData healthChangeDisplayData => ref MakeRef<HealthChangeDisplayData>();

        private EnemyLogicDelegate enemyLogicDelegate = null!;

        public UnityEvent<EnemyController> OnDeadEvent = null!;

        public void OnValidate()
        {
            if (GetComponent<NetworkRigidbody3D>() == null)
                throw new MissingComponentException("NetworkTransform component is required for EnemyController");
            if (GetComponent<Seeker>() == null)
                throw new MissingComponentException("Seeker component is required for EnemyController");
            if (GetComponent<RichAI>() == null)
                throw new MissingComponentException("RichAI component is required for EnemyController");
            if (meshFilter == null)
                throw new MissingComponentException("MeshFilter parameter is required for EnemyController config");
            if (shaderMeshAnimator == null)
                throw new MissingComponentException("ShaderMeshAnimator parameter is required for EnemyController config");
        }

        void Awake()
        {
            cachedComponents[typeof(Transform)] = transform;
            cachedComponents[typeof(NetworkRigidbody3D)] = GetComponent<NetworkRigidbody3D>();
            cachedComponents[typeof(Seeker)] = GetComponent<Seeker>();
            cachedComponents[typeof(RichAI)] = GetComponent<RichAI>();
            cachedComponents[typeof(MeshFilter)] = meshFilter;
            cachedComponents[typeof(ShaderMeshAnimator)] = shaderMeshAnimator;

            enemyLogicDelegate = new EnemyLogicDelegate(
                dataHolder: this,
                eventListener: this
            );
        }

        public void Init(int mobConfigKey)
        {
            enemyData.mobConfigKey = mobConfigKey;
        }

        public override void Spawned()
        {
            base.Spawned();
            var globalResources = GlobalResources.Instance.ThrowWhenNull();
            effectsBank = globalResources.EffectsBank;
            mobConfigsBank = globalResources.MobConfigsBank;

            cachedComponents[typeof(SkillChargeManager)] = levelContext.SkillChargeManager;
            cachedComponents[typeof(NavigationManager)] = levelContext.NavigationManager;
            cachedComponents[typeof(EffectsBank)] = effectsBank;
            cachedComponents[typeof(MobConfigsBank)] = mobConfigsBank;

            enemyLogicDelegate.Spawned(Object);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            effectsBank = null!;
            mobConfigsBank = null!;
            cachedComponents.Remove(typeof(SkillChargeManager));
            cachedComponents.Remove(typeof(NavigationManager));
            cachedComponents.Remove(typeof(EffectsBank));
            cachedComponents.Remove(typeof(MobConfigsBank));
            OnDeadEvent.RemoveAllListeners();

            enemyLogicDelegate.Despawned(runner, hasState);
        }

        public override void Render()
        {
            enemyLogicDelegate.Render();
        }

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            enemyLogicDelegate.OnGameLoopPhase(phase);
        }

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return enemyLogicDelegate.gameLoopPhases;
        }

        public T GetCachedComponent<T>() where T : Component
        {
            return (T)cachedComponents[typeof(T)];
        }

        public ref EnemyData GetEnemyData()
        {
            return ref enemyData;
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
            if (this is T typedThis)
            {
                return typedThis;
            }

            if (enemyLogicDelegate is T typed)
            {
                return typed;
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

            if (enemyLogicDelegate is T typedDelegate)
            {
                typed = typedDelegate;
                return true;
            }

            typed = default!;
            return false;
        }

        public float GetMaxHealth()
        {
            return enemyLogicDelegate.GetMaxHealth();
        }

        public float GetCurrentHealth()
        {
            return enemyLogicDelegate.GetCurrentHealth();
        }

        public void AddDamage(ref DamageActionData data)
        {
            RPC_AddDamage(data.damageOwner, data.damageValue);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddDamage(NetworkId damageOwnerId, float damageValue)
        {
            var data = new DamageActionData
            {
                damageOwner = Runner.FindObject(damageOwnerId),
                damageValue = damageValue
            };
            enemyLogicDelegate.AddDamage(ref data);
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
            enemyLogicDelegate.AddHeal(ref data);
        }

        public void AddEffects(EffectsCombination effectsCombination)
        {
            RPC_ApplyEffects(effectsBank.GetEffectsCombinationId(effectsCombination));
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ApplyEffects(int effectsCombinationId)
        {
            enemyLogicDelegate.AddEffects(effectsBank.GetEffectsCombination(effectsCombinationId));
        }

        public void AddKnockBack(ref KnockBackActionData data)
        {
            enemyLogicDelegate.AddKnockBack(ref data);

            if (!HasStateAuthority)
            {
                RPC_AddKnockBack(data.force);
            }
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_AddKnockBack(Vector3 direction)
        {
            var data = new KnockBackActionData
            {
                force = direction
            };
            enemyLogicDelegate.AddKnockBack(ref data);
        }

        public void AddStun(ref StunActionData data)
        {
            enemyLogicDelegate.AddStun(ref data);

            if (!HasStateAuthority)
            {
                RPC_AddStun(data.durationTicks);
            }
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_AddStun(int durationTicks)
        {
            var data = new StunActionData
            {
                durationTicks = durationTicks
            };
            enemyLogicDelegate.AddStun(ref data);
        }
    }
}