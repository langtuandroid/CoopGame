using System.Collections.Generic;
using Main.Scripts.Core.Mvp;
using Main.Scripts.Core.Resources;
using Main.Scripts.Player.Data;
using Main.Scripts.UI.Windows.DebugPanel.Data;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.DebugPanel
{
    public class DebugWindow : MvpMonoBehavior<DebugContract.DebugPresenter>,
        DebugContract.DebugView,
        UIScreen
    {
        [SerializeField]
        private VisualTreeAsset modifierItemLayout = default!;

        protected override DebugContract.DebugPresenter? presenter { get; set; }

        private UIDocument doc = default!;
        private ListView modifiersListView = default!;

        private List<ModifierItemData> itemsDataList = new();

        private void Awake()
        {
            
            doc = GetComponent<UIDocument>();
            doc.SetVisibility(false);

            var root = doc.rootVisualElement;
            modifiersListView = root.Q<ListView>("ModifiersList");


            modifiersListView.makeItem = () =>
            {
                var itemView = modifierItemLayout.Instantiate();

                var itemViewHolder = new DebugItemViewHolder(itemView);
                itemView.userData = itemViewHolder;

                return itemView;
            };
            modifiersListView.bindItem = (item, index) =>
            {
                (item.userData as DebugItemViewHolder)?.Bind(
                    data: itemsDataList[index],
                    onChangeValueCallback: value => { presenter?.OnItemChangeValue(index, value); });
            };

            modifiersListView.itemsSource = itemsDataList;
        }

        public void Open()
        {
            presenter ??= new DebugPresenterImpl(
                view: this,
                modifiersBank: GlobalResources.Instance.ThrowWhenNull().ModifierIdsBank,
                playerDataManager: PlayerDataManager.Instance.ThrowWhenNull()
            );
            doc.SetVisibility(true);
            presenter?.OnOpen();
        }

        public void Close()
        {
            doc.SetVisibility(false);
            presenter?.OnClose();
        }

        public void ShowTab(DebugTab tab, List<ModifierItemData> itemsList)
        {

            itemsDataList.Clear();
            itemsDataList.AddRange(itemsList);
            modifiersListView.RefreshItems();
        }

        public void OnItemUpdate(int itemIndex, ModifierItemData itemData)
        {
            modifiersListView.itemsSource[itemIndex] = itemData;
            modifiersListView.RefreshItem(itemIndex);
        }

        private void OnTabClicked(DebugTab tab)
        {
            presenter?.OnTabClicked(tab);
        }
    }
}