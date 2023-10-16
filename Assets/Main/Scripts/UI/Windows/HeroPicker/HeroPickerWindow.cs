using System.Collections.Generic;
using Main.Scripts.Core.Mvp;
using Main.Scripts.Core.Resources;
using Main.Scripts.Player.Config;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.HeroPicker
{
    public class HeroPickerWindow : MvpMonoBehavior<HeroPickerContract.HeroPickerPresenter>,
        UIScreen,
        HeroPickerContract.HeroPickerView,
        HeroInfoViewHolder.Callback
    {
        [SerializeField]
        private VisualTreeAsset heroInfoLayout = null!;
        [SerializeField]
        private UIDocument doc = null!;

        private ListView heroInfoList = default!;
        private List<HeroInfoData> itemsDataList = new();
        protected override HeroPickerContract.HeroPickerPresenter? presenter { get; set; }

        private void Awake()
        {
            var root = doc.rootVisualElement;
            SetVisibility(false);
            heroInfoList = root.Q<ListView>("HeroInfoList");
            heroInfoList.makeItem = () =>
            {
                var itemView = heroInfoLayout.Instantiate();
                var itemViewHolder = new HeroInfoViewHolder(itemView, this);
                itemView.userData = itemViewHolder;

                return itemView;
            };
            heroInfoList.bindItem = (item, index) =>
            {
                (item.userData as HeroInfoViewHolder)?.Bind(itemsDataList[index]);
            };
            heroInfoList.itemsSource = itemsDataList;
        }

        public void Open()
        {
            if (presenter == null)
            {
                var globalResources = GlobalResources.Instance.ThrowWhenNull();
                presenter = new HeroPickerPresenterImpl(
                    playerDataManager: PlayerDataManager.Instance.ThrowWhenNull(),
                    heroConfigsBank: globalResources.HeroConfigsBank,
                    view: this
                );
            }

            presenter.Show();
        }

        public void Close()
        {
            presenter?.Hide();
        }

        public void SetVisibility(bool isVisible)
        {
            doc.SetVisibility(isVisible);
        }

        public void Bind(List<HeroInfoData> dataList)
        {
            itemsDataList.Clear();
            itemsDataList.AddRange(dataList);

            heroInfoList.RefreshItems();
        }

        public void OnSelectHeroClicked(HeroConfig heroConfig)
        {
            presenter?.OnSelectHeroClicked(heroConfig);
        }
    }
}