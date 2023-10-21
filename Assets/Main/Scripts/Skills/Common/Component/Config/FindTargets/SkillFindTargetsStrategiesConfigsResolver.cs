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
            int chargeLevel,
            int stackCount,
            int powerChargeLevel,
            List<SkillFindTargetsStrategyBase> configs,
            List<SkillFindTargetsStrategyBase> resolvedConfigs
        )
        {
            foreach (var config in configs)
            {
                if (config is ModifiableSkillFindTargetsStrategies modifiableConfig)
                {
                    var modifierLevel = 0;
                    if (chargeLevel >= modifiableConfig.Modifier.HeatLevel)
                    {
                        switch (modifiableConfig.Modifier)
                        {
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
                        chargeLevel,
                        stackCount,
                        powerChargeLevel,
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