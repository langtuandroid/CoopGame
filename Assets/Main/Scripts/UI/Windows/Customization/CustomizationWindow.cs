using System.Collections.Generic;
using Main.Scripts.Core.Mvp;
using Main.Scripts.Core.Resources;
using Main.Scripts.Player.Data;
using Main.Scripts.UI.Windows.Customization.Data;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.Customization
{
    public class CustomizationWindow : MvpMonoBehavior<CustomizationContract.CustomizationPresenter>,
        CustomizationContract.CustomizationView,
        UIScreen
    {
        [SerializeField]
        private VisualTreeAsset itemLayout = default!;

        protected override CustomizationContract.CustomizationPresenter? presenter { get; set; }

        private UIDocument doc = default!;
        private Button headTab = default!;
        private Button bodyTab = default!;
        private Button handsTab = default!;
        private Button legsTab = default!;
        private Button footsTab = default!;
        private Button fullSetTab = default!;
        private ListView itemsListView = default!;

        private List<CustomizationItemData> itemsDataList = new();

        private void Awake()
        {
            
            doc = GetComponent<UIDocument>();
            doc.SetVisibility(false);

            var root = doc.rootVisualElement;
            headTab = root.Q<Button>("HeadTab");
            bodyTab = root.Q<Button>("BodyTab");
            handsTab = root.Q<Button>("HandsTab");
            legsTab = root.Q<Button>("LegsTab");
            footsTab = root.Q<Button>("FootsTab");
            fullSetTab = root.Q<Button>("FullSetTab");
            itemsListView = root.Q<ListView>("ItemsList");

            headTab.clicked += () => { OnTabClicked(CustomizationTab.HEAD); };
            bodyTab.clicked += () => { OnTabClicked(CustomizationTab.BODY); };
            handsTab.clicked += () => { OnTabClicked(CustomizationTab.HANDS); };
            legsTab.clicked += () => { OnTabClicked(CustomizationTab.LEGS); };
            footsTab.clicked += () => { OnTabClicked(CustomizationTab.FOOTS); };
            fullSetTab.clicked += () => { OnTabClicked(CustomizationTab.FULL_SET); };

            itemsListView.makeItem = () =>
            {
                var itemView = itemLayout.Instantiate();

                var itemViewHolder = new CustomizationItemViewHolder(itemView);
                itemView.userData = itemViewHolder;

                return itemView;
            };
            itemsListView.bindItem = (item, index) =>
            {
                (item.userData as CustomizationItemViewHolder)?.Bind(
                    data: itemsDataList[index],
                    onApplyCallback: () => { presenter?.OnItemApplyClicked(index); });
            };

            itemsListView.itemsSource = itemsDataList;
        }

        public void Open()
        {
            presenter ??= new CustomizationPresenterImpl(
                view: this,
                bank: GlobalResources.Instance.ThrowWhenNull().CustomizationConfigsBank,
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

        public void ShowTab(CustomizationTab tab, List<CustomizationItemData> itemsList)
        {
            headTab.SetEnabled(tab != CustomizationTab.HEAD);
            bodyTab.SetEnabled(tab != CustomizationTab.BODY);
            handsTab.SetEnabled(tab != CustomizationTab.HANDS);
            legsTab.SetEnabled(tab != CustomizationTab.LEGS);
            footsTab.SetEnabled(tab != CustomizationTab.FOOTS);
            fullSetTab.SetEnabled(tab != CustomizationTab.FULL_SET);

            itemsDataList.Clear();
            itemsDataList.AddRange(itemsList);
            itemsListView.RefreshItems();
        }

        public void OnItemUpdate(int itemIndex, CustomizationItemData itemData)
        {
            itemsListView.itemsSource[itemIndex] = itemData;
            itemsListView.RefreshItem(itemIndex);
        }

        private void OnTabClicked(CustomizationTab tab)
        {
            presenter?.OnTabClicked(tab);
        }
    }
}