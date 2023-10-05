using Main.Scripts.Mobs.Config.Condition;
using Main.Scripts.Skills;
using Main.Scripts.Utils;

namespace Main.Scripts.Mobs.Component.Delegate.Condition
{
    public class CanActivateSkillConditionDelegate : ConditionDelegate
    {
        private CanActivateSkillMobCondition conditionConfig = null!;
        
        public void Init(CanActivateSkillMobCondition conditionConfig)
        {
            this.conditionConfig = conditionConfig;
        }
        
        public bool Check(ref MobBlockContext context)
        {
            if (context.SelfUnit.TryGetInterface<SkillsOwner>(out var skillsOwner))
            {
                return skillsOwner.CanActivateSkill(conditionConfig.SkillType);
            }

            return false;
        }

        public void Reset()
        {
            conditionConfig = null!;
        }
    }
}