using System.Collections.Generic;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using Main.Scripts.Skills.Common.Component.Config.Trigger;

namespace Main.Scripts.Skills.Common.Component.Config.ActionsPack
{
    public static class SkillActionsPackResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank modifierIdsBank,
            ref PlayerData? playerData,
            SkillActionsPack actionsPackConfig,
            SkillActionsPackData resolvedDataOut
        )
        {
            SkillActionTriggerConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref playerData,
                actionsPackConfig.ActionTrigger,
                out resolvedDataOut.ActionTrigger
            );
            
            resolvedDataOut.FindTargetsStrategies.Clear();
            SkillFindTargetsStrategiesConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref playerData,
                actionsPackConfig.FindTargetsStrategies,
                resolvedDataOut.FindTargetsStrategies
            );
            
            resolvedDataOut.Actions.Clear();
            SkillActionConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref playerData,
                actionsPackConfig.Actions,
                resolvedDataOut.Actions
            );
        }
    }
}