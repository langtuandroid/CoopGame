using System;
using System.Linq;
using Fusion;
using Main.Scripts.Room;
using Main.Scripts.Skills;
using UnityEngine;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillTreePresenter : NetworkBehaviour, WindowObject
    {
        private RoomManager roomManager;
        [SerializeField]
        public SkillInfoHolder skillInfoHolder;
        
        private SkillTreeWindow skillTreeWindow;

        public override void Spawned()
        {
            roomManager = RoomManager.instance;
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
            skillTreeWindow.Bind(roomManager.GetPlayerData(Runner.LocalPlayer), skillInfoHolder);
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
        private void RPC_ResetSkillPoints(PlayerRef playerRef)
        {
            var playerData = roomManager.GetPlayerData(playerRef);
            playerData.SkillLevels.Clear();
            foreach (var skillType in Enum.GetValues(typeof(SkillType)).Cast<SkillType>())
            {
                playerData.SkillLevels.Set(skillType, 0);
            }
            playerData.UsedSkillPoints = 0;
            
            roomManager.SetPlayerData(playerRef, playerData);
            //todo subcribe to changing playerData
            UpdateSkillTree();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_IncreaseSkillLevel(PlayerRef playerRef, SkillType skillType)
        {
            var playerData = roomManager.GetPlayerData(playerRef);
            if (playerData.GetAvailableSkillPoints() > 0)
            {
                var currentSkillLevel = playerData.SkillLevels.Get(skillType);
                if (currentSkillLevel < skillInfoHolder.GetSkillInfo(skillType).MaxLevel)
                {
                    playerData.SkillLevels.Set(skillType, currentSkillLevel + 1);
                    playerData.UsedSkillPoints++;
                }
            }
            
            roomManager.SetPlayerData(playerRef, playerData);
            //todo subcribe to changing playerData
            UpdateSkillTree();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_DecreaseSkillLevel(PlayerRef playerRef, SkillType skillType)
        {
            var playerData = roomManager.GetPlayerData(playerRef);
            var currentSkillLevel = playerData.SkillLevels.Get(skillType);
            if (currentSkillLevel > 0)
            {
                playerData.SkillLevels.Set(skillType, currentSkillLevel - 1);
                playerData.UsedSkillPoints--;
            }

            roomManager.SetPlayerData(playerRef, playerData);
            //todo subcribe to changing playerData
            UpdateSkillTree();
        }
    }
}