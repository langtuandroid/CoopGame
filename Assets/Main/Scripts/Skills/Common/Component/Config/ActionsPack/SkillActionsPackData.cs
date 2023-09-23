using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using Main.Scripts.Skills.Common.Component.Config.Trigger;

namespace Main.Scripts.Skills.Common.Component.Config.ActionsPack
{
    public class SkillActionsPackData
    {
        public SkillActionTriggerBase ActionTrigger = default!;
        public readonly List<SkillFindTargetsStrategyBase> FindTargetsStrategies = new();
        public readonly List<SkillActionBase> Actions = new();
    }
}