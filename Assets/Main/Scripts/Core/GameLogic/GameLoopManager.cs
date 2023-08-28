using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Profiling;

namespace Main.Scripts.Core.GameLogic
{
    public class GameLoopManager : NetworkBehaviour
    {
        private PhysicsManager physicsManager = default!;

        private HashSet<GameLoopListener> addedListenersSet = new();
        private HashSet<GameLoopListener> removedListenersSet = new();

        private Dictionary<GameLoopPhase, HashSet<GameLoopListener>> listeners = new();
        private HashSet<GameLoopPhase> updateTriggerPhases = new()
        {
            GameLoopPhase.SkillSpawnPhase,
            GameLoopPhase.DespawnPhase
        };
        private GameLoopPhase[] beforePhysicsPhases =
        {
            GameLoopPhase.SyncTransformBeforeAllPhase,
            GameLoopPhase.EffectsRemoveFinishedPhase,
            GameLoopPhase.PlayerInputPhase,
            GameLoopPhase.StrategyPhase,
            GameLoopPhase.SkillActivationPhase,
            GameLoopPhase.SkillSpawnPhase,
            GameLoopPhase.SkillUpdatePhase,
            GameLoopPhase.EffectsApplyPhase,
            GameLoopPhase.EffectsUpdatePhase,
            GameLoopPhase.ApplyActionsPhase,
            GameLoopPhase.DespawnPhase,
            GameLoopPhase.MovementStrategyPhase,
        };
        private GameLoopPhase[] physicsPhases =
        {
            GameLoopPhase.PhysicsUpdatePhase,
            GameLoopPhase.PhysicsSkillMovementPhase,
            GameLoopPhase.PhysicsCheckCollisionsPhase,
            GameLoopPhase.PhysicsUnitsLookPhase,
            GameLoopPhase.PhysicsSkillLookPhase,
        };
        private GameLoopPhase[] afterPhysicsPhases =
        {
            GameLoopPhase.AOIUpdatePhase,
            GameLoopPhase.ObjectsSpawnPhase,
            GameLoopPhase.VisualStateUpdatePhase,
            GameLoopPhase.SyncTransformAfterAllPhase
        };

        private void Awake()
        {
            foreach (var phase in Enum.GetValues(typeof(GameLoopPhase)).Cast<GameLoopPhase>())
            {
                listeners[phase] = new HashSet<GameLoopListener>();
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            physicsManager = PhysicsManager.Instance.ThrowWhenNull();
            physicsManager.Init(Runner.Config.Simulation.TickRate);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            physicsManager = default!;
        }

        public override void FixedUpdateNetwork()
        {
            //todo Выключать симуляцию при переходах между сценами
            Profiler.BeginSample("GameLoopManager::FUN");

            UpdateListeners();

            RunPhases(beforePhysicsPhases);
            PhysicsPhase();
            RunPhases(afterPhysicsPhases);

            Profiler.EndSample();
        }

        public void AddListener(GameLoopListener listener)
        {
            addedListenersSet.Add(listener);
        }

        public void RemoveListener(GameLoopListener listener)
        {
            removedListenersSet.Add(listener);
        }

        private void UpdateListeners()
        {
            foreach (var listener in removedListenersSet)
            {
                foreach (var phase in listener.GetSubscribePhases())
                {
                    listeners[phase].Remove(listener);
                }
            }

            removedListenersSet.Clear();

            foreach (var listener in addedListenersSet)
            {
                foreach (var phase in listener.GetSubscribePhases())
                {
                    listeners[phase].Add(listener);
                }
            }

            addedListenersSet.Clear();
        }

        private void PhysicsPhase()
        {
            Profiler.BeginSample("GameLoopManager::PhysicsPhase");
            for (var i = 0; i < physicsManager.StepsByTick; i++)
            {
                foreach (var phase in physicsPhases)
                {
                    // Debug.Log($"Physics step simulation: LocalTick={Runner.Tick}");
                    foreach (var listener in listeners[phase])
                    {
                        listener.OnGameLoopPhase(phase);
                    }

                    if (phase == GameLoopPhase.PhysicsUpdatePhase)
                    {
                        Profiler.BeginSample("Physics simulate");
                        Physics.SyncTransforms(); //todo поресёрчить что именно синкает и в какую сторону (rigidbody.position в ransform.position или наоборот)
                        physicsManager.Simulate();
                        Profiler.EndSample();
                    }
                }
            }
            Profiler.EndSample();
        }

        private void RunPhases(IEnumerable<GameLoopPhase> phases)
        {
            foreach (var phase in phases)
            {
                Profiler.BeginSample($"Phase={Enum.GetName(typeof(GameLoopPhase), phase)}");
                foreach (var listener in listeners[phase])
                {
                    listener.OnGameLoopPhase(phase);
                }

                if (updateTriggerPhases.Contains(phase))
                {
                    UpdateListeners();
                }
                Profiler.EndSample();
            }
        }
    }
}