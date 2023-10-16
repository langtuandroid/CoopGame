using System.Collections.Generic;
using Main.Scripts.Customization.Banks;
using Main.Scripts.Customization.Configs;
using Main.Scripts.Player.Data;
using Main.Scripts.UI.Windows.Customization.Data;

namespace Main.Scripts.UI.Windows.Customization
{
    public class CustomizationPresenterImpl : CustomizationContract.CustomizationPresenter
    {
        private CustomizationContract.CustomizationView view;
        private CustomizationConfigsBank bank;
        private PlayerDataManager playerDataManager;

        private List<CustomizationItemConfigBase> currentItemConfigs = new();

        private int selectedItemIndex = -1;

        public CustomizationPresenterImpl(
            CustomizationContract.CustomizationView view,
            CustomizationConfigsBank bank,
            PlayerDataManager playerDataManager
        )
        {
            this.view = view;
            this.bank = bank;
            this.playerDataManager = playerDataManager;
        }

        public void OnOpen()
        {
            SelectTab(CustomizationTab.HEAD);
        }

        public void OnClose() { }

        public void OnTabClicked(CustomizationTab tab)
        {
            SelectTab(tab);
        }

        public void OnItemApplyClicked(int itemIndex)
        {
            if (selectedItemIndex >= 0)
            {
                var lastConfig = currentItemConfigs[selectedItemIndex];
                view.OnItemUpdate(selectedItemIndex,
                    new CustomizationItemData(lastConfig.NameId, false));
            }

            selectedItemIndex = itemIndex;
            var newConfig = currentItemConfigs[itemIndex];

            var customizationData = playerDataManager.GetLocalHeroData().Customization;
            switch (newConfig)
            {
                case CustomizationHeadItemConfig:
                    customizationData.headId = bank.HeadConfigs.GetCustomizationConfigId(newConfig.NameId);
                    customizationData.fullSetId = -1;
                    break;
                case CustomizationBodyItemConfig:
                    customizationData.bodyId = bank.BodyConfigs.GetCustomizationConfigId(newConfig.NameId);
                    customizationData.fullSetId = -1;
                    break;
                case CustomizationHandsItemConfig:
                    customizationData.handsId = bank.HandsConfigs.GetCustomizationConfigId(newConfig.NameId);
                    customizationData.fullSetId = -1;
                    break;
                case CustomizationLegsItemConfig:
                    customizationData.legsId = bank.LegsConfigs.GetCustomizationConfigId(newConfig.NameId);
                    customizationData.fullSetId = -1;
                    break;
                case CustomizationFootsItemConfig:
                    customizationData.footsId = bank.FootsConfigs.GetCustomizationConfigId(newConfig.NameId);
                    customizationData.fullSetId = -1;
                    break;
                case CustomizationFullSetItemConfig:
                    customizationData.fullSetId = bank.FullSetConfigs.GetCustomizationConfigId(newConfig.NameId);
                    break;
            }

            playerDataManager.ApplyCustomizationData(customizationData);

            view.OnItemUpdate(itemIndex, new CustomizationItemData(newConfig.NameId, true));
        }

        private void SelectTab(CustomizationTab tab)
        {
            currentItemConfigs.Clear();
            selectedItemIndex = -1;
            switch (tab)
            {
                case CustomizationTab.HEAD:
                    currentItemConfigs.AddRange(bank.HeadConfigs.GetConfigs());
                    break;
                case CustomizationTab.BODY:
                    currentItemConfigs.AddRange(bank.BodyConfigs.GetConfigs());
                    break;
                case CustomizationTab.HANDS:
                    currentItemConfigs.AddRange(bank.HandsConfigs.GetConfigs());
                    break;
                case CustomizationTab.LEGS:
                    currentItemConfigs.AddRange(bank.LegsConfigs.GetConfigs());
                    break;
                case CustomizationTab.FOOTS:
                    currentItemConfigs.AddRange(bank.FootsConfigs.GetConfigs());
                    break;
                case CustomizationTab.FULL_SET:
                    currentItemConfigs.AddRange(bank.FullSetConfigs.GetConfigs());
                    break;
            }

            var itemDataList = new List<CustomizationItemData>();
            foreach (var itemConfig in currentItemConfigs)
            {
                itemDataList.Add(new CustomizationItemData(itemConfig.NameId, false));
            }

            view.ShowTab(tab, itemDataList);
        }
    }
}