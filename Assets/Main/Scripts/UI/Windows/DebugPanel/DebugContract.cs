using System.Collections.Generic;
using Main.Scripts.Core.Mvp;
using Main.Scripts.UI.Windows.DebugPanel.Data;

namespace Main.Scripts.UI.Windows.DebugPanel
{
    public interface DebugContract : MvpContract
    {
        public interface DebugPresenter : Presenter
        {
            void OnOpen();
            void OnClose();
            void OnTabClicked(DebugTab tab);
            void OnItemChangeValue(int itemIndex, bool value);

            void OnSpawnEnemiesClicked(int count);
        }

        public interface DebugView : View<DebugPresenter>
        {
            void ShowTab(DebugTab tab, List<ModifierItemData> itemsList);
            void OnItemUpdate(int itemIndex, ModifierItemData itemData);
        }
    }
}