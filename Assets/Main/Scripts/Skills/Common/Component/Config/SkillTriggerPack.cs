using System;
using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config.ActionsPack;
using Main.Scripts.Skills.Common.Component.Config.Trigger;

namespace Main.Scripts.Skills.Common.Component.Config
{
    [Serializable]
    public struct SkillTriggerPack
    {
        public SkillActionTriggerBase ActionTrigger;
        public List<SkillActionsPack> ActionsPackList;
    }
}