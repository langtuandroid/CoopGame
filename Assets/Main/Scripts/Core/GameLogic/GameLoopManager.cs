using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Profiling;

namespace Main.Scripts.Core.GameLogic
{
    public class GameLoopManager : NetworkBehaviour
    {
        private PhysicsManager physicsManager = default!;

        private HashSet<GameLoopListener> listenersSet = new();
        private HashSet<GameLoopListener> addedListenersSet = new();
        private HashSet<GameLoopListener> removedListenersSet = new();

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

            SyncTransformBeforeAllPhase();

            InputPhase();

            BeforePhysicsPhase();
            
            PhysicsPhase();

            AfterPhysicsPhase();

            SyncTransformAfterAllPhase();
            
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
                listenersSet.Remove(listener);
            }
            removedListenersSet.Clear();

            foreach (var listener in addedListenersSet)
            {
                listenersSet.Add(listener);
            }
            addedListenersSet.Clear();
        }

        private void InputPhase()
        {
            UpdateListeners();
            
            Profiler.BeginSample("GameLoopManager::InputPhase");
            foreach (var listener in listenersSet)
            {
                if (!removedListenersSet.Contains(listener))
                {
                    listener.OnInputPhase();
                }
            }
            Profiler.EndSample();
        }

        private void SyncTransformBeforeAllPhase()
        {
            UpdateListeners();
            
            Profiler.BeginSample("GameLoopManager::SyncTransformBeforeAllPhase");
            foreach (var listener in listenersSet)
            {
                if (!removedListenersSet.Contains(listener))
                {
                    listener.OnSyncTransformBeforeAll();
                }
            }
            Profiler.EndSample();
        }

        private void SyncTransformAfterAllPhase()
        {
            UpdateListeners();
            
            Profiler.BeginSample("GameLoopManager::SyncTransformAfterAllPhase");
            foreach (var listener in listenersSet)
            {
                if (!removedListenersSet.Contains(listener))
                {
                    listener.OnSyncTransformAfterAll();
                }
            }
            Profiler.EndSample();
        }

        private void BeforePhysicsPhase()
        {
            UpdateListeners();
            
            Profiler.BeginSample("GameLoopManager::BeforePhysicsPhase");
            foreach (var listener in listenersSet)
            {
                if (!removedListenersSet.Contains(listener))
                {
                    listener.OnBeforePhysics();
                }
            }
            Profiler.EndSample();
        }

        private void PhysicsPhase()
        {
            UpdateListeners();
            for (var i = 0; i < physicsManager.StepsByTick; i++)
            {
                BeforePhysicsStepPhase();

                // Debug.Log($"Physics step simulation: LocalTick={Runner.Tick}");
                Profiler.BeginSample("GameLoopManager::Simulate");
                Physics.SyncTransforms(); //todo поресёрчить что именно синкает и в какую сторону (rigidbody.position в ransform.position или наоборот)
                physicsManager.Simulate();
                Profiler.EndSample();
                
                AfterPhysicsStepPhase();
            }
        }

        private void BeforePhysicsStepPhase()
        {
            UpdateListeners();
            Profiler.BeginSample("GameLoopManager::BeforePhysicsStepPhase");
            foreach (var listener in listenersSet)
            {
                if (!removedListenersSet.Contains(listener))
                {
                    listener.OnBeforePhysicsStep();
                }
            }
            Profiler.EndSample();
        }

        private void AfterPhysicsStepPhase()
        {
            UpdateListeners();
            Profiler.BeginSample("GameLoopManager::AfterPhysicsStepPhase");
            foreach (var listener in listenersSet)
            {
                if (!removedListenersSet.Contains(listener))
                {
                    listener.OnAfterPhysicsStep();
                }
            }
            Profiler.EndSample();
        }

        private void AfterPhysicsPhase()
        {
            UpdateListeners();
            Profiler.BeginSample("GameLoopManager::AfterPhysicsPhase");
            foreach (var listener in listenersSet)
            {
                if (!removedListenersSet.Contains(listener))
                {
                    listener.OnAfterPhysicsSteps();
                }
            }
            Profiler.EndSample();
        }
    }
}