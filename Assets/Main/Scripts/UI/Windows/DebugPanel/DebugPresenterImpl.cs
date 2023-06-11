using System.Collections.Generic;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;
using Main.Scripts.UI.Windows.DebugPanel.Data;

namespace Main.Scripts.UI.Windows.DebugPanel
{
    public class DebugPresenterImpl : DebugContract.DebugPresenter
    {
        private DebugContract.DebugView view;
        private ModifierIdsBank modifiersBank;
        private PlayerDataManager playerDataManager;

        public DebugPresenterImpl(
            DebugContract.DebugView view,
            ModifierIdsBank modifiersBank,
            PlayerDataManager playerDataManager
        )
        {
            this.view = view;
            this.modifiersBank = modifiersBank;
            this.playerDataManager = playerDataManager;
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

        public void OnItemChangeValue(int itemIndex, bool value)
        {
            playerDataManager.SetModifierEnable(itemIndex, value);
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
                itemDataList.Add(new ModifierItemData(modifierId.name, playerData.Modifiers.Values[modifierToken]));
            }

            view.ShowTab(tab, itemDataList);
        }
    }
}