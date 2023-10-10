using System;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    public static class SkillFollowStrategyConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref PlayerData playerData,
            int chargeLevel,
            SkillFollowStrategyBase config,
            out SkillFollowStrategyBase resolvedConfig
        )
        {
            if (config is ModifiableSkillFollowStrategies modifiableConfig)
            {
                var modifierLevel = 0;
                if (chargeLevel >= modifiableConfig.ModifierId.ChargeLevel)
                {
                    var modifierKey = bank.GetModifierIdToken(modifiableConfig.ModifierId);
                    modifierLevel = playerData.Modifiers.ModifiersLevel[modifierKey];
                }
                
                var followStrategy =
                    modifiableConfig.FollowStrategyByModifierLevel[modifierLevel];

                ResolveEnabledModifiers(
                    bank,
                    ref playerData,
                    chargeLevel,
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