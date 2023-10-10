using System.Collections.Generic;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    public static class SkillFindTargetsStrategiesConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref PlayerData playerData,
            int chargeLevel,
            List<SkillFindTargetsStrategyBase> configs,
            List<SkillFindTargetsStrategyBase> resolvedConfigs
        )
        {
            foreach (var config in configs)
            {
                if (config is ModifiableSkillFindTargetsStrategies modifiableConfig)
                {
                    var modifierLevel = 0;
                    if (chargeLevel >= modifiableConfig.ModifierId.ChargeLevel)
                    {
                        var modifierKey = bank.GetModifierIdToken(modifiableConfig.ModifierId);
                        modifierLevel = playerData.Modifiers.ModifiersLevel[modifierKey];
                    }
                    
                    var findTargetsStrategies =
                        modifiableConfig
                            .FindTargetsStrategiesByModifierLevel[modifierLevel];

                    ResolveEnabledModifiers(
                        bank,
                        ref playerData,
                        chargeLevel,
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