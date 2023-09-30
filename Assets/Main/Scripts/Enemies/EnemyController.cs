using System;
using System.Collections.Generic;
using FSG.MeshAnimator;
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
using Main.Scripts.Effects.Stats;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Skills.ActiveSkills;
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
        Affectable,
        ObjectWithGettingKnockBack,
        ObjectWithGettingStun
    {
        private Dictionary<Type, Component> cachedComponents = new();

        [SerializeField]
        private EnemyConfig enemyConfig = EnemyConfig.GetDefault();
        [SerializeField]
        private MeshAnimatorBase meshAnimator = default!;

        private EffectsBank effectsBank = default!;

        [Networked]
        private ref EnemyData enemyData => ref MakeRef<EnemyData>();
        [Networked]
        private ref ActiveSkillsData activeSkillsData => ref MakeRef<ActiveSkillsData>();
        [Networked]
        private ref HealthChangeDisplayData healthChangeDisplayData => ref MakeRef<HealthChangeDisplayData>();

        private EffectDataChangeListener? effectDataChangeListener;

        private EnemyLogicDelegate enemyLogicDelegate = default!;

        public UnityEvent<EnemyController> OnDeadEvent = default!;

        public void OnValidate()
        {
            if (GetComponent<NetworkTransform>() == null)
                throw new MissingComponentException("NetworkTransform component is required in EnemyController");
            if (GetComponent<Animator>() == null)
                throw new MissingComponentException("Animator component is required in EnemyController");
            
            EnemyLogicDelegate.OnValidate(gameObject, ref enemyConfig);
        }

        void Awake()
        {
            cachedComponents[typeof(Transform)] = transform;
            cachedComponents[typeof(NetworkRigidbody3D)] = GetComponent<NetworkRigidbody3D>();
            cachedComponents[typeof(Seeker)] = GetComponent<Seeker>();
            cachedComponents[typeof(RichAI)] = GetComponent<RichAI>();
            cachedComponents[typeof(NetworkTransform)] = GetComponent<NetworkTransform>();
            cachedComponents[typeof(MeshAnimatorBase)] = meshAnimator;

            enemyLogicDelegate = new EnemyLogicDelegate(
                config: ref enemyConfig,
                dataHolder: this,
                eventListener: this
            );
        }

        public override void Spawned()
        {
            base.Spawned();
            effectsBank = GlobalResources.Instance.ThrowWhenNull().EffectsBank;
            
            cachedComponents[typeof(EnemiesHelper)] = EnemiesHelper.Instance.ThrowWhenNull();
            cachedComponents[typeof(NavigationManager)] = levelContext.NavigationManager;
            cachedComponents[typeof(EffectsBank)] = effectsBank;

            enemyLogicDelegate.Spawned(Object);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            effectsBank = default!;
            cachedComponents.Remove(typeof(EnemiesHelper));
            cachedComponents.Remove(typeof(NavigationManager));
            cachedComponents.Remove(typeof(EffectsBank));
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

        public void SetEffectDataChangeListener(EffectDataChangeListener? effectDataChangeListener)
        {
            this.effectDataChangeListener = effectDataChangeListener;
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
            //todo NetworkId and get from registered <NetworkId, NetworkObject> map
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

        public void UpdateEffectData(int effectId, ref ActiveEffectData activeEffectData, bool isUnlimitedEffect)
        {
            //todo при переподключении клиента у него не будет изначальных данных об эффектах
            if (!HasStateAuthority) return;
            RPC_UpdateEffectData(effectId, activeEffectData, isUnlimitedEffect);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateEffectData(int effectId, ActiveEffectData activeEffectData, NetworkBool isUnlimitedEffect)
        {
            effectDataChangeListener?.OnUpdateEffectData(effectId, ref activeEffectData, isUnlimitedEffect);
        }

        public void RemoveLimitedEffectData(int effectId)
        {
            if (!HasStateAuthority) return;
            RPC_RemoveLimitedEffectData(effectId);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_RemoveLimitedEffectData(int effectId)
        {
            effectDataChangeListener?.OnRemoveLimitedEffectData(effectId);
        }

        public void UpdateStatAdditiveSum(StatType statType, float constValue, float percentValue)
        {
            if (!HasStateAuthority) return;
            RPC_UpdateStatAdditiveSum(statType, constValue, percentValue);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateStatAdditiveSum(StatType statType, float constValue, float percentValue)
        {
            effectDataChangeListener?.OnUpdateStatAdditiveSum(statType, constValue, percentValue);
        }

        public void ResetAllEffectData()
        {
            if (!HasStateAuthority) return;
            RPC_ResetAllEffectData();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ResetAllEffectData()
        {
            effectDataChangeListener?.OnResetAllEffectData();
        }
    }
}