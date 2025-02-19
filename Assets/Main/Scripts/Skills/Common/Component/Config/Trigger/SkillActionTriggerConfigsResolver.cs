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
            int heatLevel,
            int stackCount,
            int powerChargeLevel,
            int executionChargeLevel,
            int clicksCount,
            SkillActionTriggerBase config,
            out SkillActionTriggerBase resolvedConfig
        )
        {
            if (config is ModifiableSkillActionTriggers modifiableConfig)
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
                        case ClicksCountModifier:
                            modifierLevel = clicksCount;
                            break;
                    }
                }

                var trigger =
                    modifiableConfig.TriggerByModifierLevel[modifierLevel];

                ResolveEnabledModifiers(
                    bank,
                    ref heroData,
                    heatLevel,
                    stackCount,
                    powerChargeLevel,
                    executionChargeLevel,
                    clicksCount,
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
        
        public static void ResolveEnabledModifiers(
            int stackCount,
            int powerChargeLevel,
            int executionChargeLevel,
            SkillActionTriggerBase config,
            out SkillActionTriggerBase resolvedConfig
        )
        {
            if (config is ModifiableSkillActionTriggers modifiableConfig)
            {
                var modifierLevel = modifiableConfig.Modifier switch
                {
                    ExecutionChargeModifier => executionChargeLevel,
                    PowerChargeModifier => powerChargeLevel,
                    StackCountModifier => stackCount,
                    _ => 0
                };

                var trigger =
                    modifiableConfig.TriggerByModifierLevel[modifierLevel];

                ResolveEnabledModifiers(
                    stackCount,
                    powerChargeLevel,
                    executionChargeLevel,
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