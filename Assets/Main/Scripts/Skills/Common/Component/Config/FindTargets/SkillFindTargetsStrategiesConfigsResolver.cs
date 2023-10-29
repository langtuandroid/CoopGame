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
            int clicksCount,
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
                            case ClicksCountModifier:
                                modifierLevel = clicksCount;
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
                        clicksCount,
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
        
        public static void ResolveEnabledModifiers(
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
                    var modifierLevel = modifiableConfig.Modifier switch
                    {
                        ExecutionChargeModifier => executionChargeLevel,
                        PowerChargeModifier => powerChargeLevel,
                        StackCountModifier => stackCount,
                        _ => 0
                    };

                    var findTargetsStrategies =
                        modifiableConfig
                            .FindTargetsStrategiesByModifierLevel[modifierLevel];

                    ResolveEnabledModifiers(
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