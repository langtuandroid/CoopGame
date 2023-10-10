using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;

namespace Main.Scripts.Skills.Common.Component.Config.ActionsPack
{
    public static class SkillActionsPackResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank modifierIdsBank,
            ref PlayerData playerData,
            int chargeLevel,
            SkillActionsPack actionsPackConfig,
            SkillActionsPackData resolvedDataOut
        )
        {
            resolvedDataOut.FindTargetsStrategies.Clear();
            SkillFindTargetsStrategiesConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref playerData,
                chargeLevel,
                actionsPackConfig.FindTargetsStrategies,
                resolvedDataOut.FindTargetsStrategies
            );
            
            resolvedDataOut.Actions.Clear();
            SkillActionConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref playerData,
                chargeLevel,
                actionsPackConfig.Actions,
                resolvedDataOut.Actions
            );
        }
    }
}