using System;
using System.Collections.Generic;
using Main.Scripts.Enemies;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;
using Main.Scripts.UI.Windows.DebugPanel.Data;
using UnityEngine;

namespace Main.Scripts.UI.Windows.DebugPanel
{
    public class DebugPresenterImpl : DebugContract.DebugPresenter
    {
        private DebugContract.DebugView view;
        private ModifierIdsBank modifiersBank;
        private PlayerDataManager playerDataManager;
        private EnemiesManager enemiesManager;

        public DebugPresenterImpl(
            DebugContract.DebugView view,
            ModifierIdsBank modifiersBank,
            PlayerDataManager playerDataManager,
            EnemiesManager enemiesManager
        )
        {
            this.view = view;
            this.modifiersBank = modifiersBank;
            this.playerDataManager = playerDataManager;
            this.enemiesManager = enemiesManager;
        }

        public void OnOpen()
        {
            SelectTab(DebugTab.Modifiers);
        }

        public void OnClose() { }

        public void OnTabClicked(DebugTab tab)
        {
            SelectTab(tab);
        }

        public void OnItemChangeValue(int itemIndex, int value)
        {
            var modifierId = modifiersBank.GetModifierId(itemIndex);
            var newLevel = Mathf.Clamp(value, 0, modifierId.LevelsCount + 1);
            playerDataManager.SetModifierLevel(itemIndex, (ushort)newLevel);
        }

        public void OnSpawnEnemiesClicked(int count)
        {
            for (var i = 0; i < count; i++)
            {
                enemiesManager.SpawnEnemy(Vector3.zero);
            }
        }

        private void SelectTab(DebugTab tab)
        {
            var currentModifierIds = new List<ModifierId>();
            switch (tab)
            {
                case DebugTab.Modifiers:
                    currentModifierIds.AddRange(modifiersBank.GetModifierIds());
                    break;
            }

            var itemDataList = new List<ModifierItemData>();
            var playerData = playerDataManager.LocalPlayerData;
            foreach (var modifierId in currentModifierIds)
            {
                var modifierToken = modifiersBank.GetModifierIdToken(modifierId);
                itemDataList.Add(new ModifierItemData(
                    name: modifierId.name,
                    level: playerData.Modifiers.ModifiersLevel[modifierToken],
                    maxLevel: modifierId.LevelsCount
                ));
            }

            view.ShowTab(tab, itemDataList);
        }
    }
}