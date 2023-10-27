using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Profiling;

namespace Main.Scripts.Core.GameLogic
{
    public class GameLoopManager : MonoBehaviour
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
            GameLoopPhase.SkillCheckSkillFinished,
            GameLoopPhase.PlayerInputPhase,
            GameLoopPhase.StrategyPhase,
            GameLoopPhase.SkillChargeUpdate,
            GameLoopPhase.SkillActivationPhase,
            GameLoopPhase.SkillCheckCastFinished,
            GameLoopPhase.SkillSpawnPhase,
            GameLoopPhase.SkillUpdatePhase,
            GameLoopPhase.SkillVisualSpawnPhase,
            GameLoopPhase.EffectsApplyPhase,
            GameLoopPhase.ApplyActionsPhase,
            GameLoopPhase.EffectsRemoveFinishedPhase,
            GameLoopPhase.DespawnPhase,
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
            GameLoopPhase.NavigationPhase,
            GameLoopPhase.AOIUpdatePhase,
            GameLoopPhase.ObjectsSpawnPhase,
            GameLoopPhase.VisualStateUpdatePhase,
            GameLoopPhase.SyncTransformAfterAllPhase,
            GameLoopPhase.SyncChargeValue,
            GameLoopPhase.LevelStrategyPhase
        };

        private void Awake()
        {
            foreach (var phase in Enum.GetValues(typeof(GameLoopPhase)).Cast<GameLoopPhase>())
            {
                listeners[phase] = new HashSet<GameLoopListener>();
            }
        }

        public void Init(int tickRate)
        {
            physicsManager = PhysicsManager.Instance.ThrowWhenNull();
            physicsManager.Init(tickRate);
        }

        public void OnDestroy()
        {
            physicsManager = default!;
        }

        public void SimulateLoop()
        {
            Profiler.BeginSample("GameLoopManager::SimulateLoop");

            UpdateListeners();

            RunPhases(beforePhysicsPhases);
            PhysicsPhases();
            RunPhases(afterPhysicsPhases);

            Profiler.EndSample();
        }

        public void AddListener(GameLoopListener listener)
        {
            removedListenersSet.Remove(listener);
            addedListenersSet.Add(listener);
        }

        public void RemoveListener(GameLoopListener listener)
        {
            addedListenersSet.Remove(listener);
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

        private void PhysicsPhases()
        {
            Profiler.BeginSample("GameLoopManager::PhysicsPhase");
            for (var i = 0; i < physicsManager.StepsByTick; i++)
            {
                foreach (var phase in physicsPhases)
                {
                    // Debug.Log($"Physics step simulation: LocalTick={Runner.Tick}");
                    Profiler.BeginSample($"Phase={Enum.GetName(typeof(GameLoopPhase), phase)}");
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
                    Profiler.EndSample();
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