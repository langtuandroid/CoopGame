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
                {
                    var modifierLevel =
                        playerData.Value.Modifiers.ModifiersLevel[
                            bank.GetModifierIdToken(modifiableSkillFollowStrategyPack.ModifierId)];
                    var followStrategy =
                        modifiableSkillFollowStrategyPack.FollowStrategyByModifierLevel[modifierLevel];

                    ResolveEnabledModifiers(
                        bank,
                        ref playerData,
                        followStrategy,
                        out resolvedConfig
                    );

                    if (resolvedConfig == null)
                    {
                        throw new Exception(
                            $"Resolved config cannot be null. Check {modifiableSkillFollowStrategyPack.name} config.");
                    }

                    return;
                }
            }

            resolvedConfig = config;
        }
    }
}