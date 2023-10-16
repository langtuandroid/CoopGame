using Main.Scripts.Player.Config;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.HeroPicker
{
    public class HeroInfoViewHolder
    {
        private Label heroNameLabel;
        private Button selectHeroBtn;
        private HeroInfoData heroInfoData;

        public HeroInfoViewHolder(VisualElement heroInfoLayout, Callback callback)
        {
            heroNameLabel = heroInfoLayout.Q<Label>("HeroNameLabel");
            selectHeroBtn = heroInfoLayout.Q<Button>("SelectHeroButton");
            selectHeroBtn.clicked += () => { callback.OnSelectHeroClicked(heroInfoData.HeroConfig); };
        }

        public void Bind(HeroInfoData data)
        {
            heroInfoData = data;
            heroNameLabel.text = data.HeroConfig.name;
            selectHeroBtn.SetEnabled(!data.IsSelected);
            selectHeroBtn.text = data.IsSelected ? "Selected" : "Select";
        }

        public interface Callback
        {
            void OnSelectHeroClicked(HeroConfig heroConfig);
        }
    }
}