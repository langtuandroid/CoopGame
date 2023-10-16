using System.Collections.Generic;
using Fusion;
using Main.Scripts.Player.Config;
using Main.Scripts.Player.Data;

namespace Main.Scripts.UI.Windows.HeroPicker
{
    public class HeroPickerPresenterImpl : HeroPickerContract.HeroPickerPresenter
    {
        private HeroPickerContract.HeroPickerView view;
        private PlayerDataManager playerDataManager;
        private HeroConfigsBank heroConfigsBank;

        private List<HeroInfoData> heroInfoDataList = new();

        public HeroPickerPresenterImpl(
            PlayerDataManager playerDataManager,
            HeroConfigsBank heroConfigsBank,
            HeroPickerContract.HeroPickerView view
        )
        {
            this.view = view;
            this.playerDataManager = playerDataManager;
            this.heroConfigsBank = heroConfigsBank;
        }

        private void OnHeroDataChanged(PlayerRef playerRef)
        {
            if (playerRef != playerDataManager.Runner.LocalPlayer) return;

            Rebind();
        }

        public void Show()
        {
            playerDataManager.OnHeroDataChangedEvent.AddListener(OnHeroDataChanged);
            Rebind();
            view.SetVisibility(true);
        }

        private void Rebind()
        {
            heroInfoDataList.Clear();
            foreach (var heroConfig in heroConfigsBank.GetHeroConfigs())
            {
                var skillInfoData = new HeroInfoData
                {
                    HeroConfig = heroConfig,
                    IsSelected = heroConfig.Id == playerDataManager.SelectedHeroId
                };
                heroInfoDataList.Add(skillInfoData);
            }

            view.Bind(heroInfoDataList);
        }

        public void Hide()
        {
            playerDataManager.OnHeroDataChangedEvent.RemoveListener(OnHeroDataChanged);
            view.SetVisibility(false);
        }

        public void OnSelectHeroClicked(HeroConfig heroConfig)
        {
            playerDataManager.SelectHero(heroConfig.Id);
            Rebind();
        }
    }
}