using System;
using System.Linq;
using Fusion;
using Main.Scripts.Connection;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player.Experience;
using Main.Scripts.Skills;
using Main.Scripts.Utils;
using Main.Scripts.Utils.Save;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Player.Data
{
    public class PlayerDataManager : NetworkBehaviour
    {
        private SkillInfoHolder skillInfoHolder = default!;

        public UserId LocalUserId { get; private set; }
        public PlayerData LocalPlayerData { get; private set; }
        public AwardsData? LocalAwardsData { get; private set; }

        public UnityEvent<UserId, PlayerData> OnPlayerDataChangedEvent = default!;

        public override void Spawned()
        {
            Debug.Log("PlayerDataManager is spawned");
            DontDestroyOnLoad(this);
            LocalUserId = FindObjectOfType<SessionManager>().ThrowWhenNull().LocalUserId;
            skillInfoHolder = FindObjectOfType<SkillInfoHolder>().ThrowWhenNull();
            LocalPlayerData = SaveLoadUtils.Load(LocalUserId.Id.Value);
        }

        public void ResetSkillPoints()
        {
            var playerData = LocalPlayerData;
            playerData.SkillLevels.Clear();
            foreach (var skillType in Enum.GetValues(typeof(SkillType)).Cast<SkillType>())
            {
                playerData.SkillLevels.Set(skillType, 0);
            }

            playerData.UsedSkillPoints = 0;
            UpdatePlayerData(playerData);
        }

        public void IncreaseSkillLevel(SkillType skillType)
        {
            var playerData = LocalPlayerData;
            if (playerData.GetAvailableSkillPoints() > 0)
            {
                var currentSkillLevel = playerData.SkillLevels.Get(skillType);
                if (currentSkillLevel < skillInfoHolder.GetSkillInfo(skillType).MaxLevel)
                {
                    playerData.SkillLevels.Set(skillType, currentSkillLevel + 1);
                    playerData.UsedSkillPoints++;
                }
            }

            UpdatePlayerData(playerData);
        }

        public void DecreaseSkillLevel(SkillType skillType)
        {
            var playerData = LocalPlayerData;
            var currentSkillLevel = playerData.SkillLevels.Get(skillType);
            if (currentSkillLevel > 0)
            {
                playerData.SkillLevels.Set(skillType, currentSkillLevel - 1);
                playerData.UsedSkillPoints--;
            }

            UpdatePlayerData(playerData);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ApplyPlayerRewards([RpcTarget] PlayerRef playerRef, LevelResultsData levelResultsData)
        {
            var awardsData = GetAwardsData(levelResultsData);
            LocalAwardsData = awardsData;
            var playerData = LocalPlayerData;

            var experienceForNextLevel = ExperienceHelper.GetExperienceForNextLevel(playerData.Level);
            if (playerData.Experience + awardsData.Experience >= experienceForNextLevel)
            {
                playerData.Experience = playerData.Experience + awardsData.Experience - experienceForNextLevel;
                playerData.Level = Math.Clamp(playerData.Level + 1, 1, ExperienceHelper.MAX_LEVEL);

                playerData.MaxSkillPoints = ExperienceHelper.GetMaxSkillPointsByLevel(playerData.Level);
            }

            playerData.Experience += awardsData.Experience;
            UpdatePlayerData(playerData);
        }

        private AwardsData GetAwardsData(LevelResultsData levelResultsData)
        {
            var awardsData = new AwardsData();
            awardsData.IsSuccess = levelResultsData.IsSuccess;
            awardsData.Experience = levelResultsData.IsSuccess ? 200u : 50u;
            return awardsData;
        }

        private void UpdatePlayerData(PlayerData playerData)
        {
            LocalPlayerData = playerData;
            SaveLoadUtils.Save(LocalUserId.Id.Value, playerData);
            RPC_OnPlayerDataChanged(LocalUserId, LocalPlayerData);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_OnPlayerDataChanged(UserId userId, PlayerData playerData)
        {
            OnPlayerDataChangedEvent.Invoke(userId, playerData);
        }
    }
}