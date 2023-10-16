using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Connection;
using Main.Scripts.Core.Resources;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player.Experience;
using Main.Scripts.Room;
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
        private Dictionary<PlayerRef, HeroData> playersHeroDataMap = new();

        private CompositeDisposable compositeDisposable = new();

        public UserId LocalUserId { get; private set; }
        private UserData userData = null!;
        private string selectedHeroId = "";
        public string SelectedHeroId => selectedHeroId;
        public AwardsData? LocalAwardsData { get; private set; }

        public UnityEvent<PlayerRef> OnHeroDataChangedEvent = default!;
        public UnityEvent OnLocalHeroChangedEvent = default!;
        public UnityEvent OnLocalUserDataReadyEvent = default!;

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
                    userData = result.UserData;
                    if (result.IsCreatedNew)
                    {
                        SaveLoadUtils.Save(resources, LocalUserId.Id.Value, result.UserData)
                            .ObserveOnMainThread()
                            .DoOnCompleted(() =>
                            {
                                LocalUserId = localUserId;
                                OnLocalUserDataReadyEvent.Invoke();
                            })
                            .DoOnError(Debug.LogError)
                            .Subscribe()
                            .AddTo(compositeDisposable);
                    }
                    else
                    {
                        LocalUserId = localUserId;
                        OnLocalUserDataReadyEvent.Invoke();
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

        public bool HasUser(PlayerRef playerRef)
        {
            return userIdsMap.ContainsKey(playerRef);
        }

        public bool HasUser(UserId userId)
        {
            return playerRefsMap.ContainsKey(userId);
        }

        public void RemovePlayer(PlayerRef playerRef)
        {
            userIdsMap.Remove(playerRef, out var userId);
            playerRefsMap.Remove(userId);

            playersHeroDataMap.Remove(playerRef);
        }

        public void AddUserData(PlayerRef playerRef, ref UserId userId)
        {
            RPC_AddUserData(playerRef, userId);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_AddUserData(PlayerRef playerRef, UserId userId)
        {
            OnAddUserData(playerRef, ref userId);
        }

        public bool HasHeroData(PlayerRef playerRef)
        {
            return playersHeroDataMap.ContainsKey(playerRef);
        }

        public HeroData GetHeroData(PlayerRef playerRef)
        {
            return playersHeroDataMap[playerRef];
        }

        public HeroData GetLocalHeroData()
        {
            return playersHeroDataMap[Runner.LocalPlayer];
        }

        public void SelectHero(string id)
        {
            selectedHeroId = id;
            RPC_AddHeroData(Runner.LocalPlayer, userData.GetHeroData(id));
            OnLocalHeroChangedEvent.Invoke();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_AddHeroData(PlayerRef playerRef, HeroData heroData)
        {
            OnAddHeroData(playerRef, ref heroData);
        }
        
        public void SendAllUsersDataToClient(PlayerRef target)
        {
            foreach (var (userId, playerRef) in playerRefsMap)
            {
                if (playerRef != target)
                {
                    RPC_AddUserDataToClient(target, playerRef, userId);
                }
            }
        }
        
        public void SendAllHeroesDataToClient(PlayerRef target)
        {
            foreach (var (playerRef, heroData) in playersHeroDataMap)
            {
                if (playerRef != target)
                {
                    RPC_AddHeroDataToClient(target, playerRef, heroData);
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_AddUserDataToClient([RpcTarget] PlayerRef target, PlayerRef playerRef, UserId userId)
        {
            OnAddUserData(playerRef, ref userId);
        }

        private void OnAddUserData(PlayerRef playerRef, ref UserId userId)
        {
            playerRefsMap[userId] = playerRef;
            userIdsMap[playerRef] = userId;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_AddHeroDataToClient([RpcTarget] PlayerRef target, PlayerRef playerRef, HeroData heroData)
        {
            OnAddHeroData(playerRef, ref heroData);
        }

        private void OnAddHeroData(PlayerRef playerRef, ref HeroData heroData)
        {
            playersHeroDataMap[playerRef] = heroData;
            OnHeroDataChangedEvent.Invoke(playerRef);
        }

        public void ResetAllModifiersLevel()
        {
            var heroData = userData.GetHeroData(selectedHeroId);
            heroData.Modifiers = ModifiersData.GetDefault();

            heroData.UsedSkillPoints = 0;
            UpdatePlayerData(ref heroData);
        }

        public void SetModifierLevel(int modifierToken, ushort level)
        {
            var heroData = userData.GetHeroData(selectedHeroId);
            var currentLevel = heroData.Modifiers.ModifiersLevel[modifierToken];
            if (currentLevel != level)
            {
                heroData.UsedSkillPoints = (uint)((int)heroData.UsedSkillPoints - currentLevel + level);
                heroData.Modifiers.ModifiersLevel.Set(modifierToken, level);
                UpdatePlayerData(ref heroData);
            }
        }

        public void ApplyCustomizationData(CustomizationData customizationData)
        {
            var heroData = userData.GetHeroData(selectedHeroId);
            heroData.Customization = customizationData;

            UpdatePlayerData(ref heroData);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ApplyPlayerRewards([RpcTarget] PlayerRef playerRef, LevelResultsData levelResultsData)
        {
            var awardsData = GetAwardsData(levelResultsData);
            LocalAwardsData = awardsData;
            var heroData = userData.GetHeroData(selectedHeroId);

            var experienceForNextLevel = ExperienceHelper.GetExperienceForNextLevel(heroData.Level);
            if (heroData.Experience + awardsData.Experience >= experienceForNextLevel)
            {
                heroData.Experience = heroData.Experience + awardsData.Experience - experienceForNextLevel;
                heroData.Level = Math.Clamp(heroData.Level + 1, 1, ExperienceHelper.MAX_LEVEL);

                heroData.MaxSkillPoints = ExperienceHelper.GetMaxSkillPointsByLevel(heroData.Level);
            }
            else
            {
                heroData.Experience += awardsData.Experience;
            }

            UpdatePlayerData(ref heroData);
        }

        private AwardsData GetAwardsData(LevelResultsData levelResultsData)
        {
            var awardsData = new AwardsData();
            awardsData.IsSuccess = levelResultsData.IsSuccess;
            awardsData.Experience = levelResultsData.IsSuccess ? 200u : 50u;
            return awardsData;
        }

        private void UpdatePlayerData(ref HeroData heroData)
        {
            userData.UpdateHeroData(selectedHeroId, ref heroData);
            SaveLoadUtils.Save(resources, LocalUserId.Id.Value, userData)
                .ObserveOnMainThread()
                .DoOnCompleted(() => {
                    RPC_OnHeroDataChanged(Runner.LocalPlayer, userData.GetHeroData(selectedHeroId));
                })
                .DoOnError(Debug.LogError)
                .Subscribe()
                .AddTo(compositeDisposable);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_OnHeroDataChanged(PlayerRef playerRef, HeroData heroData)
        {
            if (!playersHeroDataMap.ContainsKey(playerRef))
            {
                throw new Exception("heroData is not exist");
            }
            playersHeroDataMap[playerRef] = heroData;
            OnHeroDataChangedEvent.Invoke(playerRef);
        }
    }
}