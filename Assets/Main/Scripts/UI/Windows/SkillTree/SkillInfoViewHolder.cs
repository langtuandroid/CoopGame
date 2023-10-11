using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillInfoViewHolder
    {
        private Label titleLabel;
        private Label descriptionLabel;
        private Label skillLevelLabel;
        private Button increaseLevelButton;
        private Button decreaseLevelButton;

        private SkillInfoData skillInfoData = null!;
        private InteractionCallback callback = null!;

        public SkillInfoViewHolder(VisualElement view)
        {
            titleLabel = view.Q<Label>("SkillTitle");
            descriptionLabel = view.Q<Label>("SkillDescription");
            skillLevelLabel = view.Q<Label>("SkillLevelText");
            increaseLevelButton = view.Q<Button>("SkillIncreasePoint");
            decreaseLevelButton = view.Q<Button>("SkillDecreasePoint");
            
            increaseLevelButton.clicked += OnIncreaseClicked;
            decreaseLevelButton.clicked += OnDecreaseClicked;
        }

        public void Bind(SkillInfoData skillInfoData)
        {
            this.skillInfoData = skillInfoData;
            var skillInfo = skillInfoData.SkillInfo;
            var currentLevel = skillInfoData.CurrentLevel;
            var maxLevel = skillInfo.ModifierId.UpgradeLevels;
            
            titleLabel.text = skillInfo.Title;
            descriptionLabel.text = skillInfo.Description;
            skillLevelLabel.text = $"{skillInfoData.CurrentLevel}/{maxLevel}";

            increaseLevelButton.SetEnabled(currentLevel < maxLevel);
            decreaseLevelButton.SetEnabled(currentLevel > 0);
        }

        public void SetInteractionCallback(InteractionCallback callback)
        {
            this.callback = callback;
        }

        private void OnIncreaseClicked()
        {
            callback.OnIncreaseClicked(skillInfoData);
        }

        private void OnDecreaseClicked()
        {
            callback.OnDecreaseClicked(skillInfoData);
        }

        public interface InteractionCallback
        {
            void OnIncreaseClicked(SkillInfoData skillInfo);
            void OnDecreaseClicked(SkillInfoData skillInfo);
        }
    }
}