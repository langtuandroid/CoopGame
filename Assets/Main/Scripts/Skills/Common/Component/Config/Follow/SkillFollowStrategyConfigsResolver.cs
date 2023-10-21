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
            int chargeLevel,
            int stackCount,
            int powerChargeLevel,
            SkillFollowStrategyBase config,
            out SkillFollowStrategyBase resolvedConfig
        )
        {
            if (config is ModifiableSkillFollowStrategies modifiableConfig)
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
                
                var followStrategy =
                    modifiableConfig.FollowStrategyByModifierLevel[modifierLevel];

                ResolveEnabledModifiers(
                    bank,
                    ref heroData,
                    chargeLevel,
                    stackCount,
                    powerChargeLevel,
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