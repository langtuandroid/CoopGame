using System;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    public static class SkillActionTriggerConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref PlayerData? playerData,
            SkillActionTriggerBase config,
            out SkillActionTriggerBase resolvedConfig
        )
        {
            if (playerData != null
                && config is ModifiableSkillActionTriggers modifiableSkillActionTrigger)
            {
                var modifierLevel =
                    playerData.Value.Modifiers.ModifiersLevel[
                        bank.GetModifierIdToken(modifiableSkillActionTrigger.ModifierId)];
                var trigger =
                    modifiableSkillActionTrigger.TriggerByModifierLevel[modifierLevel];

                ResolveEnabledModifiers(
                    bank,
                    ref playerData,
                    trigger,
                    out resolvedConfig
                );

                if (resolvedConfig == null)
                {
                    throw new Exception(
                        $"Resolved config cannot be null. Check {modifiableSkillActionTrigger.name} config");
                }

                return;
            }

            resolvedConfig = config;
        }
    }
}