using System.Collections.Generic;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using UnityEngine;

namespace Main.Scripts.Enemies
{
    public class NavigationManager : GameLoopEntity
    {
        [SerializeField]
        private int simulatingObjectsCountByTick;
        
        private HashSet<Object> navObjects = new();
        private Dictionary<Object, int> currentSimulatingObjects = new();
        private Queue<NavObjectData> queue = new();
        private HashSet<Object> shouldDeleteObjects = new();

        private int currentTick;

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.StrategyPhase
        };

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            foreach (var (navObject, _) in currentSimulatingObjects)
            {
                if (navObjects.Contains(navObject))
                {
                    queue.Enqueue(new NavObjectData
                    {
                        NavObject = navObject,
                        Tick = currentTick
                    });
                }
            }

            currentTick++;
            currentSimulatingObjects.Clear();

            while (currentSimulatingObjects.Count < simulatingObjectsCountByTick && queue.Count > 0)
            {
                var navObjectData = queue.Dequeue();
                if (shouldDeleteObjects.Contains(navObjectData.NavObject))
                {
                    shouldDeleteObjects.Remove(navObjectData.NavObject);
                }
                else if (navObjects.Contains(navObjectData.NavObject))
                {
                    currentSimulatingObjects.Add(navObjectData.NavObject, currentTick - navObjectData.Tick);
                }
            }
        }

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
        }

        public void Add(Object navObject)
        {
            var navObjectData = new NavObjectData
            {
                NavObject = navObject,
                Tick = currentTick
            };
            if (shouldDeleteObjects.Contains(navObject))
            {
                shouldDeleteObjects.Remove(navObject);
            }
            else
            {
                queue.Enqueue(navObjectData);
            }
            navObjects.Add(navObject);
        }

        public void Remove(Object navObject)
        {
            if (navObjects.Contains(navObject))
            {
                navObjects.Remove(navObject);
                if (currentSimulatingObjects.ContainsKey(navObject))
                {
                    currentSimulatingObjects.Remove(navObject);
                }
                else
                {
                    shouldDeleteObjects.Add(navObject);
                }
            }
        }
        
        public bool IsSimulateOnCurrentTick(Object navObject, out int deltaTicks)
        {
            if (currentSimulatingObjects.ContainsKey(navObject))
            {
                deltaTicks = currentSimulatingObjects[navObject];
                return true;
            }

            deltaTicks = 0;
            return false;
        }

        private struct NavObjectData
        {
            public Object NavObject;
            public int Tick;
        }
    }
}