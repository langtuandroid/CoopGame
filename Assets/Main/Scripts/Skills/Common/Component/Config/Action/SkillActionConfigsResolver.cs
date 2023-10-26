using System.Collections.Generic;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    public static class SkillActionConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref HeroData heroData,
            int heatLevel,
            int stackCount,
            int powerChargeLevel,
            int executionChargeLevel,
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
                            case ExecutionChargeModifier:
                                modifierLevel = executionChargeLevel;
                                break;
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
                    
                    var actions =
                        modifiableConfig.ActionsByModifierLevel[modifierLevel];
                    
                    ResolveEnabledModifiers(
                        bank,
                        ref heroData,
                        heatLevel,
                        stackCount,
                        powerChargeLevel,
                        executionChargeLevel,
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
        
        public static void ResolveEnabledModifiers(
            int stackCount,
            int powerChargeLevel,
            int executionChargeLevel,
            List<SkillActionBase> configs,
            List<SkillActionBase> resolvedConfigs
        )
        {
            foreach (var config in configs)
            {
                if (config is ModifiableSkillActions modifiableConfig)
                {
                    var modifierLevel = modifiableConfig.Modifier switch
                    {
                        ExecutionChargeModifier => executionChargeLevel,
                        PowerChargeModifier => powerChargeLevel,
                        StackCountModifier => stackCount,
                        _ => 0
                    };

                    var actions =
                        modifiableConfig.ActionsByModifierLevel[modifierLevel];
                    
                    ResolveEnabledModifiers(
                        stackCount,
                        powerChargeLevel,
                        executionChargeLevel,
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