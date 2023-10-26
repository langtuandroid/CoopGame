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
            ref HeroData heroData,
            int heatLevel,
            int stackCount,
            int powerChargeLevel,
            int executionChargeLevel,
            SkillActionsPack actionsPackConfig,
            SkillActionsPackData resolvedDataOut
        )
        {
            resolvedDataOut.FindTargetsStrategies.Clear();
            SkillFindTargetsStrategiesConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref heroData,
                heatLevel,
                stackCount,
                powerChargeLevel,
                executionChargeLevel,
                actionsPackConfig.FindTargetsStrategies,
                resolvedDataOut.FindTargetsStrategies
            );
            
            resolvedDataOut.Actions.Clear();
            SkillActionConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref heroData,
                heatLevel,
                stackCount,
                powerChargeLevel,
                executionChargeLevel,
                actionsPackConfig.Actions,
                resolvedDataOut.Actions
            );
        }
        
        public static void ResolveEnabledModifiers(
            int stackCount,
            int powerChargeLevel,
            int executionChargeLevel,
            SkillActionsPack actionsPackConfig,
            SkillActionsPackData resolvedDataOut
        )
        {
            resolvedDataOut.FindTargetsStrategies.Clear();
            SkillFindTargetsStrategiesConfigsResolver.ResolveEnabledModifiers(
                stackCount,
                powerChargeLevel,
                executionChargeLevel,
                actionsPackConfig.FindTargetsStrategies,
                resolvedDataOut.FindTargetsStrategies
            );
            
            resolvedDataOut.Actions.Clear();
            SkillActionConfigsResolver.ResolveEnabledModifiers(
                stackCount,
                powerChargeLevel,
                executionChargeLevel,
                actionsPackConfig.Actions,
                resolvedDataOut.Actions
            );
        }
    }
}