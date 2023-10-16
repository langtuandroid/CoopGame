using System.Collections.Generic;
using Main.Scripts.Core.Mvp;
using Main.Scripts.Player.Config;

namespace Main.Scripts.UI.Windows.HeroPicker
{
    public interface HeroPickerContract : MvpContract
    {
        interface HeroPickerPresenter : Presenter
        {
            void Show();
            void Hide();
            void OnSelectHeroClicked(HeroConfig heroConfig);
        }

        public interface HeroPickerView : View<HeroPickerPresenter>
        {
            void Bind(List<HeroInfoData> dataList);
            void SetVisibility(bool isVisible);
        }
    }
}