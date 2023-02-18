using System;
using System.Linq;
using Fusion;
using Main.Scripts.Player;
using Main.Scripts.Skills;
using UnityEngine;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillTreePresenter : NetworkBehaviour, WindowObject
    {
        private PlayersHolder playersHolder;
        [SerializeField]
        public SkillInfoHolder skillInfoHolder;
        
        private SkillTreeWindow skillTreeWindow;

        public override void Spawned()
        {
            playersHolder = FindObjectOfType<PlayersHolder>();
            skillTreeWindow = GetComponent<SkillTreeWindow>();

            skillTreeWindow.OnResetSkillPoints.AddListener(() =>
            {
                RPC_ResetSkillPoints(Runner.LocalPlayer);
            });
            skillTreeWindow.OnIncreaseSkillLevel.AddListener((skillType) =>
            {
                RPC_IncreaseSkillLevel(Runner.LocalPlayer, skillType);
            });
            skillTreeWindow.OnDecreaseSkillLevel.AddListener((skillType) =>
            {
                RPC_DecreaseSkillLevel(Runner.LocalPlayer, skillType);
            });
        }

        public void Show()
        {
            skillTreeWindow.SetVisibility(true);
            UpdateSkillTree();
        }

        public void Hide()
        {
            skillTreeWindow.SetVisibility(false);
        }
        
        private void UpdateSkillTree()
        {
            skillTreeWindow.Bind(playersHolder.players.Get(Runner.LocalPlayer).PlayerData, skillInfoHolder);
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
        private void RPC_ResetSkillPoints(PlayerRef playerRef)
        {
            var playerData = playersHolder.players.Get(playerRef).PlayerData;
            playerData.SkillLevels.Clear();
            foreach (var skillType in Enum.GetValues(typeof(SkillType)).Cast<SkillType>())
            {
                playerData.SkillLevels.Set(skillType, 0);
            }
            playerData.AvailableSkillPoints = playerData.MaxSkillPoints;
            //todo subcribe to changing playerData
            UpdateSkillTree();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_IncreaseSkillLevel(PlayerRef playerRef, SkillType skillType)
        {
            var playerData = playersHolder.players.Get(playerRef).PlayerData;
            if (playerData.AvailableSkillPoints > 0)
            {
                var currentSkillLevel = playerData.SkillLevels.Get(skillType);
                if (currentSkillLevel < skillInfoHolder.GetSkillInfo(skillType).MaxLevel)
                {
                    playerData.SkillLevels.Set(skillType, currentSkillLevel + 1);
                    playerData.AvailableSkillPoints--;
                }
            }
            //todo subcribe to changing playerData
            UpdateSkillTree();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_DecreaseSkillLevel(PlayerRef playerRef, SkillType skillType)
        {
            var playerData = playersHolder.players.Get(playerRef).PlayerData;
            var currentSkillLevel = playerData.SkillLevels.Get(skillType);
            if (currentSkillLevel > 0)
            {
                playerData.SkillLevels.Set(skillType, currentSkillLevel - 1);
                playerData.AvailableSkillPoints++;
            }
            //todo subcribe to changing playerData
            UpdateSkillTree();
        }
    }
}