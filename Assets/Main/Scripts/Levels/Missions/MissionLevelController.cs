using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player;
using Main.Scripts.Player.Experience;
using Main.Scripts.Room;
using Main.Scripts.Skills;
using Main.Scripts.Tasks;
using UnityEngine;

namespace Main.Scripts.Levels.Missions
{
    public class MissionLevelController : NetworkBehaviour
    {
        [SerializeField]
        private PlayerController playerPrefab;
        [SerializeField]
        private PlayersHolder playersHolder;
        [SerializeField]
        private PlaceTargetTask placeTargetTask;

        private RoomManager roomManager;

        private PlayerCamera playerCamera;

        public void Awake()
        {
            roomManager = FindObjectOfType<RoomManager>();
            playerCamera = FindObjectOfType<PlayerCamera>();
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                var connectedPlayers = roomManager.GetConnectedPlayers();
                foreach (var playerRef in connectedPlayers)
                {
                    SpawnPlayer(Runner, playerRef);
                }

                placeTargetTask.OnTaskCompleted.AddListener(OnPlaceTargetTaskCompleted);
            }
        }

        public override void Render()
        {
            if (playersHolder.players.ContainsKey(Runner.LocalPlayer))
            {
                playerCamera.SetTarget(playersHolder.players.Get(Runner.LocalPlayer).transform);
            }
        }

        private void SpawnPlayer(NetworkRunner runner, PlayerRef playerRef)
        {
            //todo добавить спавн поинты
            Runner.Spawn(
                prefab: playerPrefab,
                position: Vector3.zero,
                rotation: Quaternion.identity,
                inputAuthority: playerRef,
                onBeforeSpawned: (networkRunner, playerObject) =>
                {
                    var playerController = playerObject.GetComponent<PlayerController>();

                    playersHolder.players.Add(playerRef, playerController);
                    playerController.OnPlayerDeadEvent.AddListener(OnPlayerDead);

                    //todo load player data from storage
                    playerController.PlayerData.Level = 1;
                    playerController.PlayerData.Experience = 0;
                    playerController.PlayerData.MaxSkillPoints = ExperienceHelper.GetMaxSkillPointsByLevel(playerController.PlayerData.Level);
                    playerController.PlayerData.AvailableSkillPoints = playerController.PlayerData.MaxSkillPoints;
                    foreach (var skill in Enum.GetValues(typeof(SkillType)).Cast<SkillType>())
                    {
                        playerController.PlayerData.SkillLevels.Set(skill, 0);
                    }
                }
            );
        }

        private void OnPlayerDead(PlayerRef deadPlayerRef)
        {
            foreach (var (_, playerController) in playersHolder.players)
            {
                if (playerController.state != PlayerController.State.Dead)
                {
                    return;
                }
            }

            OnMissionFailed();
        }

        private void OnMissionFailed()
        {
            var levelResults = new Dictionary<PlayerRef, LevelResultsData>();
            foreach (var (playerRef, _) in playersHolder.players)
            {
                levelResults.Add(playerRef, new LevelResultsData {IsSuccess = false});
            }

            roomManager.OnLevelFinished(levelResults);
        }

        private void OnPlaceTargetTaskCompleted()
        {
            var levelResults = new Dictionary<PlayerRef, LevelResultsData>();
            foreach (var (playerRef, _) in playersHolder.players)
            {
                levelResults.Add(playerRef, new LevelResultsData {IsSuccess = true});
            }

            roomManager.OnLevelFinished(levelResults);
        }
    }
}