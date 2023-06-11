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
                && config is ModifiableSkillActionTriggerPack modifiableSkillActionTriggerPack)
            {
                foreach (var modifiableActionsTriggersList in modifiableSkillActionTriggerPack.ModifiablePacks)
                {
                    var isEnabled = playerData != null && playerData.Value.Modifiers.Values[
                        bank.GetModifierIdToken(modifiableActionsTriggersList.ModifierId)];
                    if (isEnabled)
                    {
                        ResolveEnabledModifiers(
                            bank,
                            ref playerData,
                            modifiableActionsTriggersList.ItemToApply,
                            out resolvedConfig
                        );
                        if (resolvedConfig == null)
                        {
                            throw new Exception(
                                "Resolved config cannot be null. Check ModifiableActionTriggerPack configs");
                        }

                        return;
                    }
                }
            }

            resolvedConfig = config;
        }
    }
}