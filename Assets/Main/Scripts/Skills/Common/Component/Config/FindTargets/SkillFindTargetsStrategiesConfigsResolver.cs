using System.Collections.Generic;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    public static class SkillFindTargetsStrategiesConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref PlayerData? playerData,
            List<SkillFindTargetsStrategyBase> configs,
            List<SkillFindTargetsStrategyBase> resolvedConfigs
        )
        {
            foreach (var config in configs)
            {
                if (playerData != null
                    && config is ModifiableSkillFindTargetsStrategies modifiableSkillFindTargetsStrategiesPack)
                {
                    var modifierLevel =
                        playerData.Value.Modifiers.ModifiersLevel[
                            bank.GetModifierIdToken(modifiableSkillFindTargetsStrategiesPack.ModifierId)];
                    var findTargetsStrategies =
                        modifiableSkillFindTargetsStrategiesPack.FindTargetsStrategiesByModifierLevel[modifierLevel];
                    
                    ResolveEnabledModifiers(
                        bank,
                        ref playerData,
                        findTargetsStrategies.Value,
                        resolvedConfigs
                    );
                }
                else
                {
                    resolvedConfigs.Add(config);
                }
            }
        }
    }
}