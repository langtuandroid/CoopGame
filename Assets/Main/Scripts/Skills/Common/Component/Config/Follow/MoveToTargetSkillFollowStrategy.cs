using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    [CreateAssetMenu(fileName = "MoveToTargetFollowStrategy", menuName = "Skill/Follow/MoveToTarget")]
    public class MoveToTargetSkillFollowStrategy : SkillFollowStrategyBase
    {
        [SerializeField]
        private SkillPointType moveTo;
        [SerializeField]
        private SkillValue speed = null!;

        public SkillPointType MoveTo => moveTo;
        public SkillValue Speed => speed;
        
                
        private void OnValidate()
        {
            Assert.IsTrue(speed != null, $"{name}: Speed value must be not null");
        }
    }
}