using System;
using System.Linq;
using Fusion;
using Main.Scripts.Connection;
using Main.Scripts.Core.Resources;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player.Experience;
using Main.Scripts.Skills;
using Main.Scripts.Utils;
using Main.Scripts.Utils.Save;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

namespace Main.Scripts.Player.Data
{
    public class PlayerDataManager : NetworkBehaviour
    {
        public static PlayerDataManager? Instance { get; private set; }
        
        private GlobalResources resources = default!;
        
        [Networked, Capacity(16)]
        private NetworkDictionary<UserId, PlayerRef> playerRefsMap => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<PlayerRef, UserId> userIdsMap => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<UserId, PlayerData> playersDataMap => default;

        public UserId LocalUserId { get; private set; }
        public PlayerData LocalPlayerData { get; private set; }
        public AwardsData? LocalAwardsData { get; private set; }

        public UnityEvent<UserId, PlayerData, PlayerData> OnPlayerDataChangedEvent = default!;
        public UnityEvent OnLocalPlayerDataReadyEvent = default!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public override void Spawned()
        {
            Debug.Log("PlayerDataManager is spawned");
            DontDestroyOnLoad(this);
            LocalUserId = SessionManager.Instance.ThrowWhenNull().LocalUserId;
            resources = GlobalResources.Instance.ThrowWhenNull();
            
            //todo вынести загрузку в отдельный поток
            LocalPlayerData = SaveLoadUtils.Load(GlobalResources.Instance.ThrowWhenNull(), LocalUserId.Id.Value);
            OnLocalPlayerDataReadyEvent.Invoke();
        }

        public bool IsReady()
        {
            return !LocalUserId.Id.Value.IsNullOrEmpty();
        }

        public bool HasPlayer(PlayerRef playerRef)
        {
            return userIdsMap.ContainsKey(playerRef);
        }

        public bool HasPlayer(UserId userId)
        {
            return playerRefsMap.ContainsKey(userId);
        }

        public UserId GetUserId(PlayerRef playerRef)
        {
            return userIdsMap.Get(playerRef);
        }

        public PlayerRef GetPlayerRef(UserId userId)
        {
            return playerRefsMap.Get(userId);
        }

        public void RemovePlayer(PlayerRef playerRef, bool keepPlayerData)
        {
            userIdsMap.Remove(playerRef, out var userId);
            playerRefsMap.Remove(userId);

            if (keepPlayerData)
            {
                playersDataMap.Remove(userId);
            }
        }

        public void ClearAllKeepedPlayerData()
        {
            foreach (var (userId, _) in playersDataMap)
            {
                if (!playerRefsMap.ContainsKey(userId))
                {
                    playersDataMap.Remove(userId);
                }
            }
        }
        
        public PlayerData? GetPlayerData(PlayerRef playerRef)
        {
            return userIdsMap.ContainsKey(playerRef) ? GetPlayerData(userIdsMap[playerRef]) : null;
        }

        public PlayerData? GetPlayerData(UserId userId)
        {
            return playersDataMap.ContainsKey(userId) ? playersDataMap[userId] : null;
        }

        public void AddPlayerData(PlayerRef playerRef, UserId userId, PlayerData playerData)
        {
            playerRefsMap.Set(userId, playerRef);
            userIdsMap.Set(playerRef, userId);
            playersDataMap.Set(userId, playerData);
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
                if (currentSkillLevel < resources.SkillInfoHolder.GetSkillInfo(skillType).MaxLevel)
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

        public void ApplyCustomizationData(CustomizationData customizationData)
        {
            var playerData = LocalPlayerData;
            playerData.Customization = customizationData;

            UpdatePlayerData(playerData);
        }

        public void SetModifierEnable(int modifierToken, bool enable)
        {
            var playerData = LocalPlayerData;
            if (playerData.Modifiers.Values[modifierToken] != enable)
            {
                playerData.Modifiers.Values.Set(modifierToken, enable);
                UpdatePlayerData(playerData);
            }
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
            else
            {
                playerData.Experience += awardsData.Experience;
            }

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
            SaveLoadUtils.Save(resources, LocalUserId.Id.Value, playerData);
            RPC_OnPlayerDataChanged(LocalUserId, LocalPlayerData);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_OnPlayerDataChanged(UserId userId, PlayerData playerData)
        {
            if (!playersDataMap.ContainsKey(userId))
            {
                throw new Exception("playerData is not exist");
            }
            var oldPlayerData = playersDataMap[userId];
            playersDataMap.Set(userId, playerData);
            OnPlayerDataChangedEvent.Invoke(userId, playerData, oldPlayerData);
        }
    }
}