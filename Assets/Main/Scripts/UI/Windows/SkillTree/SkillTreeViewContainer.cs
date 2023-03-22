using System;
using Main.Scripts.Player.Data;
using Main.Scripts.Player.Experience;
using Main.Scripts.Skills;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillTreeViewContainer : SkillTreeContract.SkillTreeView, SkillInfoFrame.InteractionCallback
    {
        private UIDocument doc;
        private Label playerLevelCountLabel;
        private Label xpCountLabel;
        private Label skillPointsCountLabel;
        private SkillInfoFrame healthBoostSkill;
        private SkillInfoFrame damageBoostSkill;
        private SkillInfoFrame speedBoostSkill;

        private SkillInfoHolder skillInfoHolder;

        public Action? OnResetSkillPoints;
        public Action<SkillType>? OnIncreaseSkillLevel;
        public Action<SkillType>? OnDecreaseSkillLevel;

        public SkillTreeViewContainer(UIDocument doc, SkillInfoHolder skillInfoHolder)
        {
            this.doc = doc;
            this.skillInfoHolder = skillInfoHolder;
            var root = doc.rootVisualElement;
            SetVisibility(false);
            playerLevelCountLabel = root.Q<Label>("PlayerLevelCount");
            xpCountLabel = root.Q<Label>("XpCount");
            var resetButton = root.Q<Button>("SkillResetButton");
            resetButton.clicked += () => { OnResetSkillPoints?.Invoke(); };
            skillPointsCountLabel = root.Q<Label>("SkillPointsCount");
            healthBoostSkill = SkillInfoFrame.from(root.Q<VisualElement>("SkillHPBoost"));
            healthBoostSkill.SetInteractionCallback(this);
            damageBoostSkill = SkillInfoFrame.from(root.Q<VisualElement>("SkillDamageBoost"));
            damageBoostSkill.SetInteractionCallback(this);
            speedBoostSkill = SkillInfoFrame.from(root.Q<VisualElement>("SkillSpeedBoost"));
            speedBoostSkill.SetInteractionCallback(this);
        }

        public void SetVisibility(bool isVisible)
        {
            doc.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Bind(PlayerData playerData)
        {
            playerLevelCountLabel.text = $"{playerData.Level}";
            var experienceForNextLevel = ExperienceHelper.GetExperienceForNextLevel(playerData.Level);
            xpCountLabel.text = $"{playerData.Experience}/{experienceForNextLevel}";
            skillPointsCountLabel.text = $"{playerData.GetAvailableSkillPoints()}/{playerData.MaxSkillPoints}";
            healthBoostSkill.Bind(
                skillInfo: skillInfoHolder.GetSkillInfo(SkillType.HEALTH_BOOST_PASSIVE),
                currentLevel: playerData.SkillLevels.Get(SkillType.HEALTH_BOOST_PASSIVE)
            );
            damageBoostSkill.Bind(
                skillInfo: skillInfoHolder.GetSkillInfo(SkillType.DAMAGE_BOOST_PASSIVE),
                currentLevel: playerData.SkillLevels.Get(SkillType.DAMAGE_BOOST_PASSIVE)
            );
            speedBoostSkill.Bind(
                skillInfo: skillInfoHolder.GetSkillInfo(SkillType.SPEED_BOOST_PASSIVE),
                currentLevel: playerData.SkillLevels.Get(SkillType.SPEED_BOOST_PASSIVE)
            );
        }

        public void OnIncreaseClicked(SkillType skillType)
        {
            OnIncreaseSkillLevel?.Invoke(skillType);
        }

        public void OnDecreaseClicked(SkillType skillType)
        {
            OnDecreaseSkillLevel?.Invoke(skillType);
        }
    }
}