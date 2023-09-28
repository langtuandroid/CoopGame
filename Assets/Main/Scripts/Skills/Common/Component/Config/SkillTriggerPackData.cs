using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config.ActionsPack;
using Main.Scripts.Skills.Common.Component.Config.Trigger;

namespace Main.Scripts.Skills.Common.Component.Config
{
    public struct SkillTriggerPackData
    {
        public SkillActionTriggerBase ActionTrigger;
        public List<SkillActionsPackData> ActionsPackList;
    }
}