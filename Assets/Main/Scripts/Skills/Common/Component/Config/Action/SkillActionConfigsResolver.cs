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
                    && config is ModifiableSkillActions modifiableSkillActionsPack)
                {
                    var modifierLevel =
                        playerData.Value.Modifiers.ModifiersLevel[
                            bank.GetModifierIdToken(modifiableSkillActionsPack.ModifierId)];
                    var actions =
                        modifiableSkillActionsPack.ActionsByModifierLevel[modifierLevel];
                    
                    ResolveEnabledModifiers(
                        bank,
                        ref playerData,
                        actions.Value,
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