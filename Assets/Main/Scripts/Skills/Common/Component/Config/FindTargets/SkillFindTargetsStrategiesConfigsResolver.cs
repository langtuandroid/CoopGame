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
                    foreach (var modifiableFindTargetsStrategies in modifiableSkillFindTargetsStrategiesPack
                                 .ModifiableTargetsStrategies)
                    {
                        var isEnabled = playerData.Value.Modifiers.Values[
                            bank.GetModifierIdToken(modifiableFindTargetsStrategies.ModifierId)];
                        if (isEnabled)
                        {
                            ResolveEnabledModifiers(
                                bank,
                                ref playerData,
                                modifiableFindTargetsStrategies.ItemsToApply,
                                resolvedConfigs
                            );
                        }
                    }
                }
                else
                {
                    resolvedConfigs.Add(config);
                }
            }
        }
    }
}