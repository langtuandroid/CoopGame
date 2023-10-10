using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;

namespace Main.Scripts.Skills.Common.Component.Config.ActionsPack
{
    public class SkillActionsPackData
    {
        public List<SkillFindTargetsStrategyBase> FindTargetsStrategies = null!;
        public List<SkillActionBase> Actions = null!;
    }
}