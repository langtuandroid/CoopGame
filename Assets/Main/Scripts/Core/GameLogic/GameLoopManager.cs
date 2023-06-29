using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Utils;
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
            Profiler.BeginSample("GameLoopManager::FUN");
            UpdateListeners();
            
            Profiler.BeginSample("GameLoopManager::OnBeforePhysicsSteps");
            foreach (var listener in listenersSet)
            {
                if (!removedListenersSet.Contains(listener))
                {
                    listener.OnBeforePhysicsSteps();
                }
            }
            Profiler.EndSample();
            
            UpdateListeners();
            for (var i = 0; i < physicsManager.StepsByTick; i++)
            {
                UpdateListeners();
                Profiler.BeginSample("GameLoopManager::OnBeforePhysicsStep");
                foreach (var listener in listenersSet)
                {
                    if (!removedListenersSet.Contains(listener))
                    {
                        listener.OnBeforePhysicsStep();
                    }
                }
                Profiler.EndSample();


                Profiler.BeginSample("GameLoopManager::Simulate");
                physicsManager.Simulate();
                Profiler.EndSample();
                
                UpdateListeners();
                Profiler.BeginSample("GameLoopManager::OnAfterPhysicsStep");
                foreach (var listener in listenersSet)
                {
                    if (!removedListenersSet.Contains(listener))
                    {
                        listener.OnAfterPhysicsStep();
                    }
                }
                Profiler.EndSample();
            }

            UpdateListeners();
            Profiler.BeginSample("GameLoopManager::OnAfterPhysicsSteps");
            foreach (var listener in listenersSet)
            {
                if (!removedListenersSet.Contains(listener))
                {
                    listener.OnAfterPhysicsSteps();
                }
            }
            Profiler.EndSample();
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
    }
}