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
                && config is ModifiableSkillFollowStrategies modifiableSkillFollowStrategyPack)
            {
                foreach (var modifiableFollowStrategiesPack in modifiableSkillFollowStrategyPack.ModifiableFollowStrategies)
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