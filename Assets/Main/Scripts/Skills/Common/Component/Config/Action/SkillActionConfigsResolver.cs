using System.Collections.Generic;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    public static class SkillActionConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref PlayerData playerData,
            int chargeLevel,
            List<SkillActionBase> configs,
            List<SkillActionBase> resolvedConfigs
        )
        {
            foreach (var config in configs)
            {
                if (config is ModifiableSkillActions modifiableConfig)
                {
                    var modifierLevel = 0;
                    if (chargeLevel >= modifiableConfig.ModifierId.ChargeLevel)
                    {
                        var modifierKey = bank.GetModifierIdToken(modifiableConfig.ModifierId);
                        modifierLevel = playerData.Modifiers.ModifiersLevel[modifierKey];
                    }
                    
                    var actions =
                        modifiableConfig.ActionsByModifierLevel[modifierLevel];
                    
                    ResolveEnabledModifiers(
                        bank,
                        ref playerData,
                        chargeLevel,
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