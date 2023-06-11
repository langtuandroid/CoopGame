using System;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    public static class SkillFollowStrategyConfigsResolver
    {
        public static void ResolveEnabledModifiers(
            ModifierIdsBank bank,
            ref PlayerData? playerData,
            SkillFollowStrategyBase config,
            out SkillFollowStrategyBase resolvedConfig
        )
        {
            if (playerData != null
                && config is ModifiableSkillFollowStrategyPack modifiableSkillFollowStrategyPack)
            {
                foreach (var modifiableFollowStrategiesPack in modifiableSkillFollowStrategyPack.ModifiablePacks)
                {
                    var isEnabled = playerData != null && playerData.Value.Modifiers.Values[
                        bank.GetModifierIdToken(modifiableFollowStrategiesPack.ModifierId)];
                    if (isEnabled)
                    {
                        ResolveEnabledModifiers(
                            bank,
                            ref playerData,
                            modifiableFollowStrategiesPack.ItemToApply,
                            out resolvedConfig
                        );
                        if (resolvedConfig == null)
                        {
                            throw new Exception(
                                "Resolved config cannot be null. Check ModifiableFollowStrategyPack configs");
                        }

                        return;
                    }
                }
            }

            resolvedConfig = config;
        }
    }
}