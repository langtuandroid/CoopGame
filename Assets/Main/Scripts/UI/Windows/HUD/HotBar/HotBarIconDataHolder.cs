using System;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.UI.Windows.HUD.HotBar.HotBarIcons;
using UnityEngine;

namespace Main.Scripts.UI.Windows.HUD.HotBar
{
    public class HotBarIconDataHolder : MonoBehaviour
    {
        public static HotBarIconDataHolder? Instance { get; private set; }

        [SerializeField]
        private HotBarIconData attackIcon = default!;
        [SerializeField]
        private HotBarIconData skillOneIcon = default!;
        [SerializeField]
        private HotBarIconData skillTwoIcon = default!;
        [SerializeField]
        private HotBarIconData skillThreeIcon = default!;
        [SerializeField]
        private HotBarIconData dashIcon = default!;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public HotBarIconData GetIconData(ActiveSkillType activeSkillType)
        {
            return activeSkillType switch
            {
                ActiveSkillType.PRIMARY => attackIcon,
                ActiveSkillType.FIRST_SKILL => skillOneIcon,
                ActiveSkillType.SECOND_SKILL => skillTwoIcon,
                ActiveSkillType.THIRD_SKILL => skillThreeIcon,
                ActiveSkillType.DASH => dashIcon,
                _ => throw new ArgumentOutOfRangeException(nameof(activeSkillType), activeSkillType, null)
            };
        }
    }
}