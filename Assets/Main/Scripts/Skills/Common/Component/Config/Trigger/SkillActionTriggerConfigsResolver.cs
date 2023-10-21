using System;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    public static class SkillActionTriggerConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref HeroData heroData,
            int chargeLevel,
            int stackCount,
            int powerChargeLevel,
            SkillActionTriggerBase config,
            out SkillActionTriggerBase resolvedConfig
        )
        {
            if (config is ModifiableSkillActionTriggers modifiableConfig)
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

                var trigger =
                    modifiableConfig.TriggerByModifierLevel[modifierLevel];

                ResolveEnabledModifiers(
                    bank,
                    ref heroData,
                    chargeLevel,
                    stackCount,
                    powerChargeLevel,
                    trigger,
                    out resolvedConfig
                );

                if (resolvedConfig == null)
                {
                    throw new Exception(
                        $"Resolved config cannot be null. Check {modifiableConfig.name} config");
                }
            }
            else
            {
                resolvedConfig = config;
            }
        }
    }
}