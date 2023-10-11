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
            int heatLevel,
            int powerChargeLevel,
            List<SkillActionBase> configs,
            List<SkillActionBase> resolvedConfigs
        )
        {
            foreach (var config in configs)
            {
                if (config is ModifiableSkillActions modifiableConfig)
                {
                    var modifierLevel = 0;
                    if (heatLevel >= modifiableConfig.Modifier.HeatLevel)
                    {
                        switch (modifiableConfig.Modifier)
                        {
                            case ModifierId modifierId:
                                var modifierKey = bank.GetModifierIdToken(modifierId);
                                modifierLevel = playerData.Modifiers.ModifiersLevel[modifierKey];
                                break;
                            case PowerChargeModifier:
                                modifierLevel = powerChargeLevel;
                                break;
                        }
                    }
                    
                    var actions =
                        modifiableConfig.ActionsByModifierLevel[modifierLevel];
                    
                    ResolveEnabledModifiers(
                        bank,
                        ref playerData,
                        heatLevel,
                        powerChargeLevel,
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