using System;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    public static class SkillActionTriggerConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref PlayerData playerData,
            int chargeLevel,
            SkillActionTriggerBase config,
            out SkillActionTriggerBase resolvedConfig
        )
        {
            if (config is ModifiableSkillActionTriggers modifiableConfig)
            {
                var modifierLevel = 0;
                if (chargeLevel >= modifiableConfig.ModifierId.ChargeLevel)
                {
                    var modifierKey = bank.GetModifierIdToken(modifiableConfig.ModifierId);
                    modifierLevel = playerData.Modifiers.ModifiersLevel[modifierKey];
                }

                var trigger =
                    modifiableConfig.TriggerByModifierLevel[modifierLevel];

                ResolveEnabledModifiers(
                    bank,
                    ref playerData,
                    chargeLevel,
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