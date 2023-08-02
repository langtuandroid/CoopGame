using System;
using Main.Scripts.Skills.ActiveSkills;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.HUD.HotBar.HotBarIcons
{
    public class HotBarView
    {
        private UIDocument doc;

        private HotBarItemView attackItemView;
        private HotBarItemView dashItemView;
        private HotBarItemView skillOneItemView;
        private HotBarItemView skillTwoItemView;
        private HotBarItemView skillThreeItemView;


        public HotBarView(UIDocument doc)
        {
            this.doc = doc;
            var root = doc.rootVisualElement;
            attackItemView = new HotBarItemView(root.Q<VisualElement>("AttackSkill"));
            skillOneItemView = new HotBarItemView(root.Q<VisualElement>("FirstSkill"));
            skillTwoItemView = new HotBarItemView(root.Q<VisualElement>("SecondSkill"));
            skillThreeItemView = new HotBarItemView(root.Q<VisualElement>("ThirdSkill"));
            dashItemView = new HotBarItemView(root.Q<VisualElement>("DashSkill"));
        }

        public void Bind(ref HotBarData hotBarData)
        {
            attackItemView.Bind(hotBarData.PrimaryIconData);
            skillOneItemView.Bind(hotBarData.FirstSkillIconData);
            skillTwoItemView.Bind(hotBarData.SecondSkillIconData);
            skillThreeItemView.Bind(hotBarData.ThirdSkillIconData);
            dashItemView.Bind(hotBarData.DashIconData);
        }

        public void SetCooldown(ActiveSkillType skillType, int cooldownLeftSec)
        {
            var icon = GetHotBarIconByActiveSkillType(skillType);
            icon.UpdateCooldown(cooldownLeftSec);
        }

        public void SetVisibility(bool isVisible)
        {
            doc.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private HotBarItemView GetHotBarIconByActiveSkillType(ActiveSkillType skillType)
        {
            return skillType switch
            {
                ActiveSkillType.PRIMARY => attackItemView,
                ActiveSkillType.FIRST_SKILL => skillOneItemView,
                ActiveSkillType.SECOND_SKILL => skillTwoItemView,
                ActiveSkillType.THIRD_SKILL => skillThreeItemView,
                ActiveSkillType.DASH => dashItemView,

                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null)
            };
        }
    }
}