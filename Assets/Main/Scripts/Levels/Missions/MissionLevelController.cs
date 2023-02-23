using System.Collections.Generic;
using Fusion;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player;
using Main.Scripts.Room;
using Main.Scripts.Tasks;
using UnityEngine;

namespace Main.Scripts.Levels.Missions
{
    public class MissionLevelController : NetworkBehaviour
    {
        [SerializeField]
        private PlayerController playerPrefab = default!;
        [SerializeField]
        private PlayersHolder playersHolder = default!;
        [SerializeField]
        private PlaceTargetTask placeTargetTask = default!;

        private RoomManager roomManager = default!;

        private PlayerCamera playerCamera = default!;

        public override void Spawned()
        {
            roomManager = FindObjectOfType<RoomManager>();
            playerCamera = FindObjectOfType<PlayerCamera>();
            
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
            if (playersHolder.Players.ContainsKey(Runner.LocalPlayer))
            {
                playerCamera.SetTarget(playersHolder.Players.Get(Runner.LocalPlayer).transform);
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

                    playersHolder.Players.Add(playerRef, playerController);
                    playerController.OnPlayerDeadEvent.AddListener(OnPlayerDead);
                }
            );
        }

        private void OnPlayerDead(PlayerRef deadPlayerRef)
        {
            foreach (var (_, playerController) in playersHolder.Players)
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
            foreach (var (playerRef, _) in playersHolder.Players)
            {
                levelResults.Add(playerRef, new LevelResultsData
                {
                    IsSuccess = false,
                    Experience = 50
                });
            }

            roomManager.OnLevelFinished(levelResults);
        }

        private void OnPlaceTargetTaskCompleted()
        {
            var levelResults = new Dictionary<PlayerRef, LevelResultsData>();
            foreach (var (playerRef, _) in playersHolder.Players)
            {
                levelResults.Add(playerRef, new LevelResultsData
                {
                    IsSuccess = true,
                    Experience = 200
                });
            }

            roomManager.OnLevelFinished(levelResults);
        }
    }
}