using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.LevelGeneration.Configs;
using Main.Scripts.Levels.Map;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player;
using Main.Scripts.Scenarios;
using Main.Scripts.Scenarios.Missions;
using Main.Scripts.Scenarios.Missions.Common;
using Main.Scripts.Tasks;
using Main.Scripts.Utils;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Main.Scripts.Levels.Missions
{
    public class MissionLevelController : LevelControllerBase
    {
        [SerializeField]
        private AstarPath pathfinder = null!;
        [SerializeField]
        private MapGenerationConfig mapGenerationConfig = null!;
        [SerializeField]
        private LevelStyleConfig levelStyleConfig = null!;
        [SerializeField]
        private ScenarioGeneratorConfig scenarioGeneratorConfig = null!;

        private PlayerCamera playerCamera = null!;
        private LevelMapController levelMapController = null!;
        
        private PlaceTargetTask? finishPlaceTargetTask;
        private Scenario? missionScenario;

        private CompositeDisposable compositeDisposable = new();

        private MissionState missionState;

        private HashSet<PlayerRef> playersReady = new();

        private bool isPlayersReady;

        private void Awake()
        {
            levelMapController = new LevelMapController(
                mapGenerationConfig: mapGenerationConfig,
                levelStyleConfig: levelStyleConfig,
                pathfinder: pathfinder,
                collidersParent: pathfinder.transform
            );
        }

        public override void Spawned()
        {
            base.Spawned();
            missionState = MissionState.Loading;
            playersReady.Clear();
            isPlayersReady = false;
            
            playerCamera = PlayerCamera.Instance.ThrowWhenNull();

            levelMapController
                .GenerateMap((int)(Random.value * int.MaxValue))
                .Do(mapData =>
                {
                    if (HasStateAuthority)
                    {
                        MissionScenarioGeneratorHelper.GenerateMissionScenario(
                                scenarioGeneratorConfig: scenarioGeneratorConfig,
                                runner: Runner,
                                playersHolder: playersHolder,
                                mapData: mapData
                            )
                            .ObserveOnMainThread()
                            .Do(scenario => { missionScenario = scenario; })
                            .Subscribe()
                            .AddTo(compositeDisposable);
                    }
                })
                .Subscribe()
                .AddTo(compositeDisposable);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            compositeDisposable.Clear();
            
            base.Despawned(runner, hasState);
        }

        private void LateUpdate()
        {
            if (playerCamera != null && playersHolder.Contains(Runner.LocalPlayer))
            {
                var visibleBoundsSize = playerCamera.GetVisibleBoundsSize();
                var halfXSize = visibleBoundsSize.x / 2;
                var halfYSize = visibleBoundsSize.y / 2;
                
                var playerPosition = playersHolder.Get(Runner.LocalPlayer).transform.position;

                levelMapController.UpdateChunksVisibilityBounds(
                    playerPosition.x - halfXSize,
                    playerPosition.x + halfXSize,
                    playerPosition.z - halfYSize,
                    playerPosition.z + halfYSize
                );
            }
        }

        protected override void OnPlayerInitialized(PlayerRef playerRef)
        {
            
        }

        protected override void OnPlayerDisconnected(PlayerRef playerRef)
        {
            
        }

        protected override void OnLocalPlayerLoaded()
        {
            RPC_OnPlayerReady(Runner.LocalPlayer);
        }

        protected override void OnSpawnPhase()
        {
            
        }

        protected override void OnDespawnPhase()
        {
            
        }

        protected override void OnPhysicsCheckCollisionsPhase()
        {
            finishPlaceTargetTask?.OnPhysicsCheckCollisionsPhase();
        }

        protected override void OnLevelStrategyPhase()
        {
            if (!HasStateAuthority) return;

            switch (missionState)
            {
                case MissionState.Loading:
                    if (isPlayersReady)
                    {
                        missionState = MissionState.Active;

                        missionScenario?.Start();
                    }
                    break;
                case MissionState.Active:
                    if (missionScenario != null)
                    {
                        missionScenario.Update();
                        switch (missionScenario.GetStatus())
                        {
                            case ScenarioStatus.Success:
                                missionState = MissionState.Success;
                                missionScenario.Stop();
                                break;
                            case ScenarioStatus.Failed:
                                missionState = MissionState.Failed;
                                missionScenario.Stop();
                                break;
                        }
                    }
                    break;
                case MissionState.Failed:
                    OnMissionFailed();
                    break;
                case MissionState.Success:
                    OnMissionSuccess();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override bool IsMapReady()
        {
            return levelMapController.IsMapReady
                   && (!HasStateAuthority || missionScenario != null);
        }

        protected override bool IsLevelReady()
        {
            return Runner.IsSceneReady() && isPlayersReady;
        }

        private void OnMissionSuccess()
        {
            var levelResults = new Dictionary<PlayerRef, LevelResultsData>();
            //todo use playerDataManager
            foreach (var playerRef in playersHolder.GetKeys())
            {
                levelResults.Add(playerRef, new LevelResultsData
                {
                    IsSuccess = true
                });
            }

            roomManager.OnLevelFinished(levelResults);
        }

        private void OnMissionFailed()
        {
            var levelResults = new Dictionary<PlayerRef, LevelResultsData>();
            foreach (var playerRef in Runner.ActivePlayers)
            {
                levelResults.Add(playerRef, new LevelResultsData
                {
                    IsSuccess = false
                });
            }

            roomManager.OnLevelFinished(levelResults);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_OnPlayerReady(PlayerRef playerRefReady)
        {
            if (!HasStateAuthority) return;
            
            playersReady.Add(playerRefReady);

            if (isPlayersReady)
            {
                //Disconnect new players if mission is started
                Runner.Disconnect(playerRefReady);
                return;
            }
            
            foreach (var playerRef in Runner.ActivePlayers)
            {
                if (!playersReady.Contains(playerRef))
                {
                    return;
                }
            }

            RPC_OnPlayersReady();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_OnPlayersReady()
        {
            isPlayersReady = true;
        }
    }
}