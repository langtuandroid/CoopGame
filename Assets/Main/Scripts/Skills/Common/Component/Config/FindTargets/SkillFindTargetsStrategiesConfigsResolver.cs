using System.Collections.Generic;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    public static class SkillFindTargetsStrategiesConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref HeroData heroData,
            int heatLevel,
            int stackCount,
            int powerChargeLevel,
            int executionChargeLevel,
            List<SkillFindTargetsStrategyBase> configs,
            List<SkillFindTargetsStrategyBase> resolvedConfigs
        )
        {
            foreach (var config in configs)
            {
                if (config is ModifiableSkillFindTargetsStrategies modifiableConfig)
                {
                    var modifierLevel = 0;
                    if (heatLevel >= modifiableConfig.Modifier.HeatLevel)
                    {
                        switch (modifiableConfig.Modifier)
                        {
                            case ExecutionChargeModifier:
                                modifierLevel = executionChargeLevel;
                                break;
                            case ModifierId modifierId:
                                var modifierKey = bank.GetModifierIdToken(modifierId);
                                modifierLevel = heroData.Modifiers.ModifiersLevel[modifierKey];
                                break;
                            case PowerChargeModifier:
                                modifierLevel = powerChargeLevel;
                                break;
                            case StackCountModifier:
                                modifierLevel = stackCount;
                                break;
                        }
                    }
                    
                    var findTargetsStrategies =
                        modifiableConfig
                            .FindTargetsStrategiesByModifierLevel[modifierLevel];

                    ResolveEnabledModifiers(
                        bank,
                        ref heroData,
                        heatLevel,
                        stackCount,
                        powerChargeLevel,
                        executionChargeLevel,
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