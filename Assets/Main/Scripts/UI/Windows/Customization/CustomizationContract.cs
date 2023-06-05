using System.Collections.Generic;
using Main.Scripts.Core.Mvp;
using Main.Scripts.UI.Windows.Customization.Data;

namespace Main.Scripts.UI.Windows.Customization
{
    public interface CustomizationContract : MvpContract
    {
        public interface CustomizationPresenter : Presenter
        {
            void OnOpen();
            void OnClose();
            void OnTabClicked(CustomizationTab tab);
            void OnItemApplyClicked(int itemIndex);
        }

        public interface CustomizationView : View<CustomizationPresenter>
        {
            void ShowTab(CustomizationTab tab, List<CustomizationItemData> itemsList);
            void OnItemUpdate(int itemIndex, CustomizationItemData itemData);
        }
    }
}