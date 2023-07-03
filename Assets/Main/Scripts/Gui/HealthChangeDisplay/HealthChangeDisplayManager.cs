using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Main.Scripts.Gui.HealthChangeDisplay
{
    public class HealthChangeDisplayManager
    {
        private static int TICK_BUFFER_LENGTH = HealthChangeDisplayData.TICK_BUFFER_LENGTH;

        private HealthChangeDisplayConfig config;
        private DataHolder dataHolder;
        private NetworkObject objectContext = default!;

        private HealthChangeDisplay?[] syncDisplayItemsList = new HealthChangeDisplay?[TICK_BUFFER_LENGTH];
        private Stack<HealthChangeDisplay> displayItemsPool = new();
        private int displayItemsCount;
        private int lastTicksGroupChange;

        public HealthChangeDisplayManager(
            ref HealthChangeDisplayConfig config,
            DataHolder dataHolder
        )
        {
            this.config = config;
            this.dataHolder = dataHolder;
            
            for (var i = 0; i < TICK_BUFFER_LENGTH; i++)
            {
                syncDisplayItemsList[i] = null;
            }
        }

        public void Spawned(NetworkObject objectContext)
        {
            this.objectContext = objectContext;
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            objectContext = default!;
        }

        public void OnAfterPhysicsSteps()
        {
            ref var data = ref dataHolder.GetHealthChangeDisplayData();
            
            var nextIndex = GetNextIndex();
            data.damageSumBuffer.Set(nextIndex, 0f);
            data.healSumBuffer.Set(nextIndex, 0f);
        }

        public void Render()
        {
            ref var data = ref dataHolder.GetHealthChangeDisplayData();
            
            var prevIndex = GetPrevIndex();
            var nextIndex = GetNextIndex();

            //будет заспавнен только новый айтем. Если предыдущие были пропущены, то уже не заспавняться
            var shouldSpawnDisplayItem = syncDisplayItemsList[prevIndex] == null;

            //забываем последний айтем, т.к. он был обнулён в FixedNetworkUpdate
            syncDisplayItemsList[nextIndex] = null;

            for (var i = 0; i < TICK_BUFFER_LENGTH; i++)
            {
                var healSum = data.healSumBuffer[i];
                var damageSum = data.damageSumBuffer[i];


                if (shouldSpawnDisplayItem && i == prevIndex && (healSum > 0 || damageSum > 0))
                {
                    if (!displayItemsPool.TryPop(out var displayItem) || displayItem == null)
                    {
                        displayItem = Object.Instantiate(config.healthChangeDisplay, config.interpolationTransform);
                    }

                    displayItem.SetActive();
                    displayItem.SetTimer(config.textLifeTimer);
                    displayItem.SetTextSpeed(config.textSpeed);
                    displayItem.OnDisplayDisableAction += OnDisplayDisable;

                    syncDisplayItemsList[i] = displayItem;
                    
                }

                var syncDisplayItem = syncDisplayItemsList[i];
                if (syncDisplayItem != null)
                {
                    syncDisplayItem.SetDamage(damageSum);
                    syncDisplayItem.SetHeal(healSum);
                }
            }
        }

        public void ApplyDamage(float damage)
        {
            ref var data = ref dataHolder.GetHealthChangeDisplayData();
            
            var currentIndex = GetCurrentIndex();
            var newDamageSum = data.damageSumBuffer[currentIndex] + damage;
            data.damageSumBuffer.Set(currentIndex, newDamageSum);
        }

        public void ApplyHeal(float heal)
        {
            ref var data = ref dataHolder.GetHealthChangeDisplayData();
            
            var currentIndex = GetCurrentIndex();
            var newHealSum = data.healSumBuffer[currentIndex] + heal;
            data.healSumBuffer.Set(currentIndex, newHealSum);
        }

        private int GetCurrentIndex()
        {
            return (objectContext.Runner.Tick / config.tickBufferStep) % TICK_BUFFER_LENGTH;
        }

        private int GetNextIndex()
        {
            return (objectContext.Runner.Tick / config.tickBufferStep + 1) % TICK_BUFFER_LENGTH;
        }

        private int GetPrevIndex()
        {
            return (objectContext.Runner.Tick / config.tickBufferStep - 1 + TICK_BUFFER_LENGTH) % TICK_BUFFER_LENGTH;
        }

        private void OnDisplayDisable(HealthChangeDisplay display)
        {
            displayItemsPool.Push(display);
        }

        public interface DataHolder
        {
            public ref HealthChangeDisplayData GetHealthChangeDisplayData();
        }
    }
}