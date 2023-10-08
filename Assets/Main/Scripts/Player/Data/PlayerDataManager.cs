using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Connection;
using Main.Scripts.Core.Resources;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player.Experience;
using Main.Scripts.Room;
using Main.Scripts.Skills;
using Main.Scripts.Utils;
using Main.Scripts.Utils.Save;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

namespace Main.Scripts.Player.Data
{
    [OrderAfter(typeof(RoomManager))]
    public class PlayerDataManager : NetworkBehaviour
    {
        public static PlayerDataManager? Instance { get; private set; }
        
        private GlobalResources resources = default!;
        
        private Dictionary<UserId, PlayerRef> playerRefsMap = new();
        private Dictionary<PlayerRef, UserId> userIdsMap = new();
        private Dictionary<UserId, PlayerData> playersDataMap = new();

        private CompositeDisposable compositeDisposable = new();

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
            resources = GlobalResources.Instance.ThrowWhenNull();
            var localUserId = SessionManager.Instance.ThrowWhenNull().LocalUserId;

            SaveLoadUtils.Load(GlobalResources.Instance.ThrowWhenNull(), localUserId.Id.Value)
                .ObserveOnMainThread()
                .Do(result =>
                {
                    LocalPlayerData = result.playerData;
                    if (result.IsCreatedNew)
                    {
                        SaveLoadUtils.Save(resources, LocalUserId.Id.Value, result.playerData)
                            .ObserveOnMainThread()
                            .DoOnCompleted(() =>
                            {
                                LocalUserId = localUserId;
                                OnLocalPlayerDataReadyEvent.Invoke();
                            })
                            .DoOnError(Debug.LogError)
                            .Subscribe()
                            .AddTo(compositeDisposable);
                    }
                    else
                    {
                        LocalUserId = localUserId;
                        OnLocalPlayerDataReadyEvent.Invoke();
                    }
                })
                .Subscribe()
                .AddTo(compositeDisposable);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            compositeDisposable.Clear();
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
            return userIdsMap[playerRef];
        }

        public PlayerRef GetPlayerRef(UserId userId)
        {
            return playerRefsMap[userId];
        }

        public void RemovePlayer(PlayerRef playerRef, bool clearPlayerData)
        {
            userIdsMap.Remove(playerRef, out var userId);
            playerRefsMap.Remove(userId);

            if (clearPlayerData)
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

        public void AddPlayerData(PlayerRef playerRef, ref UserId userId, ref PlayerData playerData)
        {
            RPC_AddPlayerData(playerRef, userId, playerData);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_AddPlayerData(PlayerRef playerRef, UserId userId, PlayerData playerData)
        {
            OnAddPlayerData(playerRef, ref userId, ref playerData);
        }
        
        public void SendAllPlayersDataToClient(PlayerRef target)
        {
            foreach (var (userId, playerRef) in playerRefsMap)
            {
                if (playerRef != target)
                {
                    RPC_AddPlayerDataToClient(target, playerRef, userId, playersDataMap[userId]);
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_AddPlayerDataToClient([RpcTarget] PlayerRef target, PlayerRef playerRef, UserId userId, PlayerData playerData)
        {
            OnAddPlayerData(playerRef, ref userId, ref playerData);
        }

        private void OnAddPlayerData(PlayerRef playerRef, ref UserId userId, ref PlayerData playerData)
        {
            playerRefsMap[userId] = playerRef;
            userIdsMap[playerRef] = userId;
            playersDataMap[userId] = playerData;
            OnPlayerDataChangedEvent.Invoke(userId, playerData, default); //todo сделать отдельный интерфейс
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

        public void SetModifierLevel(int modifierToken, ushort level)
        {
            var playerData = LocalPlayerData;
            if (playerData.Modifiers.ModifiersLevel[modifierToken] != level)
            {
                playerData.Modifiers.ModifiersLevel.Set(modifierToken, level);
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
            SaveLoadUtils.Save(resources, LocalUserId.Id.Value, playerData)
                .ObserveOnMainThread()
                .DoOnCompleted(() => {
                    RPC_OnPlayerDataChanged(LocalUserId, LocalPlayerData);
                })
                .DoOnError(Debug.LogError)
                .Subscribe()
                .AddTo(compositeDisposable);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_OnPlayerDataChanged(UserId userId, PlayerData playerData)
        {
            if (!playersDataMap.ContainsKey(userId))
            {
                throw new Exception("playerData is not exist");
            }
            var oldPlayerData = playersDataMap[userId];
            playersDataMap[userId] = playerData;
            OnPlayerDataChangedEvent.Invoke(userId, playerData, oldPlayerData);
        }
    }
}