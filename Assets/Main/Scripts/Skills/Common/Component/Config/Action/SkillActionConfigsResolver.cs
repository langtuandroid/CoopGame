using System.Collections.Generic;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    public static class SkillActionConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref PlayerData? playerData,
            List<SkillActionBase> configs,
            List<SkillActionBase> resolvedConfigs
        )
        {
            foreach (var config in configs)
            {
                if (playerData != null
                    && config is ModifiableSkillActionsPack modifiableSkillActionsPack)
                {
                    foreach (var modifiableActions in modifiableSkillActionsPack.ModifiablePacks)
                    {
                        var isEnabled = playerData.Value.Modifiers.Values[
                            bank.GetModifierIdToken(modifiableActions.ModifierId)];
                        if (isEnabled)
                        {
                            ResolveEnabledModifiers(
                                bank,
                                ref playerData,
                                modifiableActions.ItemsToApply,
                                resolvedConfigs
                            );
                        }
                    }
                }
                else
                {
                    resolvedConfigs.Add(config);
                }
            }
        }
    }
}