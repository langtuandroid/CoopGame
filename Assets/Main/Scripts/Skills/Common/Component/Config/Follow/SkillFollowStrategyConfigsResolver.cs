using System;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    public static class SkillFollowStrategyConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref HeroData heroData,
            int heatLevel,
            int stackCount,
            int powerChargeLevel,
            int executionChargeLevel,
            SkillFollowStrategyBase config,
            out SkillFollowStrategyBase resolvedConfig
        )
        {
            if (config is ModifiableSkillFollowStrategies modifiableConfig)
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
                
                var followStrategy =
                    modifiableConfig.FollowStrategyByModifierLevel[modifierLevel];

                ResolveEnabledModifiers(
                    bank,
                    ref heroData,
                    heatLevel,
                    stackCount,
                    powerChargeLevel,
                    executionChargeLevel,
                    followStrategy,
                    out resolvedConfig
                );

                if (resolvedConfig == null)
                {
                    throw new Exception(
                        $"Resolved config cannot be null. Check {modifiableConfig.name} config.");
                }
            }
            else
            {
                resolvedConfig = config;
            }
        }
        
        public static void ResolveEnabledModifiers(
            int stackCount,
            int powerChargeLevel,
            int executionChargeLevel,
            SkillFollowStrategyBase config,
            out SkillFollowStrategyBase resolvedConfig
        )
        {
            if (config is ModifiableSkillFollowStrategies modifiableConfig)
            {
                var modifierLevel = modifiableConfig.Modifier switch
                {
                    ExecutionChargeModifier => executionChargeLevel,
                    PowerChargeModifier => powerChargeLevel,
                    StackCountModifier => stackCount,
                    _ => 0
                };

                var followStrategy =
                    modifiableConfig.FollowStrategyByModifierLevel[modifierLevel];

                ResolveEnabledModifiers(
                    stackCount,
                    powerChargeLevel,
                    executionChargeLevel,
                    followStrategy,
                    out resolvedConfig
                );

                if (resolvedConfig == null)
                {
                    throw new Exception(
                        $"Resolved config cannot be null. Check {modifiableConfig.name} config.");
                }
            }
            else
            {
                resolvedConfig = config;
            }
        }
    }
}