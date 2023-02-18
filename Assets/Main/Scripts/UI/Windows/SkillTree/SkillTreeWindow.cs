using Main.Scripts.Player;
using Main.Scripts.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillTreeWindow : MonoBehaviour, SkillInfoFrame.InteractionCallback
    {
        private UIDocument doc;
        private Button resetButton;
        private Label skillPointsCountLabel;
        private SkillInfoFrame healthBoostSkill;
        private SkillInfoFrame damageBoostSkill;
        private SkillInfoFrame speedBoostSkill;

        public UnityEvent OnResetSkillPoints;
        public UnityEvent<SkillType> OnIncreaseSkillLevel;
        public UnityEvent<SkillType> OnDecreaseSkillLevel;

        private void Awake()
        {
            doc = GetComponent <UIDocument>();
            var root = doc.rootVisualElement;
            root.visible = false;
            resetButton = root.Q<Button>("SkillResetButton");
            resetButton.clicked += OnResetSkillPoints.Invoke;
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
            doc.rootVisualElement.visible = isVisible;
        }

        public void Bind(PlayerData state, SkillInfoHolder skillInfoHolder)
        {
            skillPointsCountLabel.text = $"{state.AvailableSkillPoints}/{state.MaxSkillPoints}";
            healthBoostSkill.Bind(
                skillInfo: skillInfoHolder.GetSkillInfo(SkillType.HEALTH_BOOST_PASSIVE),
                currentLevel: state.SkillLevels.Get(SkillType.HEALTH_BOOST_PASSIVE)
            );
            damageBoostSkill.Bind(
                skillInfo: skillInfoHolder.GetSkillInfo(SkillType.DAMAGE_BOOST_PASSIVE),
                currentLevel: state.SkillLevels.Get(SkillType.DAMAGE_BOOST_PASSIVE)
            );
            speedBoostSkill.Bind(
                skillInfo: skillInfoHolder.GetSkillInfo(SkillType.SPEED_BOOST_PASSIVE),
                currentLevel: state.SkillLevels.Get(SkillType.SPEED_BOOST_PASSIVE)
            );
        }

        public void OnIncreaseClicked(SkillType skillType)
        {
            OnIncreaseSkillLevel.Invoke(skillType);
        }

        public void OnDecreaseClicked(SkillType skillType)
        {
            OnDecreaseSkillLevel.Invoke(skillType);
        }
    }
}