using System;
using System.Collections.Generic;
using Main.Scripts.Player.Experience;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillTreeViewContainer : SkillTreeContract.SkillTreeView, SkillInfoViewHolder.InteractionCallback
    {
        private UIDocument doc;
        private Label playerLevelCountLabel;
        private Label xpCountLabel;
        private Label skillPointsCountLabel;
        private ListView skillInfoListView;

        private List<SkillInfoData> itemsDataList = new();

        public Action? OnResetSkillPoints;
        public Action<SkillInfoData>? OnIncreaseSkillLevelClicked;
        public Action<SkillInfoData>? OnDecreaseSkillLevelClicked;

        public SkillTreeViewContainer(UIDocument doc, VisualTreeAsset skillInfoLayout)
        {
            this.doc = doc;
            var root = doc.rootVisualElement;
            SetVisibility(false);
            playerLevelCountLabel = root.Q<Label>("PlayerLevelCount");
            xpCountLabel = root.Q<Label>("XpCount");
            var resetButton = root.Q<Button>("SkillResetButton");
            resetButton.clicked += () => { OnResetSkillPoints?.Invoke(); };
            skillPointsCountLabel = root.Q<Label>("SkillPointsCount");
            skillInfoListView = root.Q<ListView>("SkillList");

            skillInfoListView.makeItem = () =>
            {
                var itemView = skillInfoLayout.Instantiate();

                var itemViewHolder = new SkillInfoViewHolder(itemView);
                itemViewHolder.SetInteractionCallback(this);
                itemView.userData = itemViewHolder;

                return itemView;
            };
            skillInfoListView.bindItem = (item, index) =>
            {
                (item.userData as SkillInfoViewHolder)?.Bind(itemsDataList[index]);
            };
            skillInfoListView.itemsSource = itemsDataList;
        }

        public void SetVisibility(bool isVisible)
        {
            doc.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Bind(PlayerInfoData playerInfoData)
        {
            playerLevelCountLabel.text = $"{playerInfoData.Level}";
            var experienceForNextLevel = ExperienceHelper.GetExperienceForNextLevel(playerInfoData.Level);
            xpCountLabel.text = $"{playerInfoData.Experience}/{experienceForNextLevel}";
            skillPointsCountLabel.text = $"{playerInfoData.AvailableSkillPoints}/{playerInfoData.MaxSkillPoints}";

            itemsDataList.Clear();
            itemsDataList.AddRange(playerInfoData.SkillInfoDataList);
            skillInfoListView.RefreshItems();
        }

        public void OnIncreaseClicked(SkillInfoData skillInfoData)
        {
            OnIncreaseSkillLevelClicked?.Invoke(skillInfoData);
        }

        public void OnDecreaseClicked(SkillInfoData skillInfoData)
        {
            OnDecreaseSkillLevelClicked?.Invoke(skillInfoData);
        }
    }
}