using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using UnityEngine;

namespace Main.Scripts.Gui
{
    public class HealthChangeDisplayManager : GameLoopEntity
    {
        private const int TICK_BUFFER_LENGTH = 10;

        [SerializeField]
        private Transform interpolationTransform = default!;
        [SerializeField]
        private int tickBufferStep = 5;
        [SerializeField]
        private float textLifeTimer = 2f;
        [SerializeField]
        private float textSpeed = 5f;
        [SerializeField]
        private HealthChangeDisplay healthChangeDisplay = default!;

        [Networked]
        [Capacity(TICK_BUFFER_LENGTH)]
        private NetworkArray<float> damageSumBuffer => default!;
        [Networked]
        [Capacity(TICK_BUFFER_LENGTH)]
        private NetworkArray<float> healSumBuffer => default!;

        private HealthChangeDisplay?[] syncDisplayItemsList = new HealthChangeDisplay?[TICK_BUFFER_LENGTH];
        private Stack<HealthChangeDisplay> displayItemsPool = new();
        private int displayItemsCount;
        private int lastTicksGroupChange;

        private void Awake()
        {
            for (var i = 0; i < TICK_BUFFER_LENGTH; i++)
            {
                syncDisplayItemsList[i] = null;
            }
        }

        public override void OnAfterPhysicsSteps()
        {
            var nextIndex = GetNextIndex();
            damageSumBuffer.Set(nextIndex, 0f);
            healSumBuffer.Set(nextIndex, 0f);
        }

        public override void Render()
        {
            var prevIndex = GetPrevIndex();
            var nextIndex = GetNextIndex();

            //будет заспавнен только новый айтем. Если предыдущие были пропущены, то уже не заспавняться
            var shouldSpawnDisplayItem = syncDisplayItemsList[prevIndex] == null;

            //забываем последний айтем, т.к. он был обнулён в FixedNetworkUpdate
            syncDisplayItemsList[nextIndex] = null;

            for (var i = 0; i < TICK_BUFFER_LENGTH; i++)
            {
                var healSum = healSumBuffer[i];
                var damageSum = damageSumBuffer[i];


                if (shouldSpawnDisplayItem && i == prevIndex && (healSum > 0 || damageSum > 0))
                {
                    if (!displayItemsPool.TryPop(out var displayItem) || displayItem == null)
                    {
                        displayItem = Instantiate(healthChangeDisplay, interpolationTransform);
                    }

                    displayItem.SetActive();
                    displayItem.SetTimer(textLifeTimer);
                    displayItem.SetTextSpeed(textSpeed);
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
            var currentIndex = GetCurrentIndex();
            var newDamageSum = damageSumBuffer[currentIndex] + damage;
            damageSumBuffer.Set(currentIndex, newDamageSum);
        }

        public void ApplyHeal(float heal)
        {
            var currentIndex = GetCurrentIndex();
            var newHealSum = healSumBuffer[currentIndex] + heal;
            healSumBuffer.Set(currentIndex, newHealSum);
        }

        private int GetCurrentIndex()
        {
            return (Runner.Tick / tickBufferStep) % TICK_BUFFER_LENGTH;
        }

        private int GetNextIndex()
        {
            return (Runner.Tick / tickBufferStep + 1) % TICK_BUFFER_LENGTH;
        }

        private int GetPrevIndex()
        {
            return (Runner.Tick / tickBufferStep - 1 + TICK_BUFFER_LENGTH) % TICK_BUFFER_LENGTH;
        }

        private void OnDisplayDisable(HealthChangeDisplay display)
        {
            displayItemsPool.Push(display);
        }
    }
}