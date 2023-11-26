using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Core.Resources;
using Main.Scripts.Player;
using Main.Scripts.Player.Config;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Scenarios.Missions.Common
{
public class PlayersSpawnTaskController : GameLoopEntityNetworked
{
    [SerializeField]
    private PlayerController playerPrefab = null!;

    private PlayerCamera playerCamera = null!;
    private HeroConfigsBank heroConfigsBank = null!;
    private PlayerDataManager playerDataManager = null!;

    private HashSet<PlayerRef> spawnedPlayers = new();

    private List<Vector3> spawnActions = new();

    private GameLoopPhase[] gameLoopPhases =
    {
        GameLoopPhase.ObjectsSpawnPhase
    };

    public Action? OnAllPlayersSpawned;

    public override void Spawned()
    {
        base.Spawned();

        playerCamera = PlayerCamera.Instance.ThrowWhenNull();
        heroConfigsBank = GlobalResources.Instance.ThrowWhenNull().HeroConfigsBank;
        playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        playerCamera = null!;
        heroConfigsBank = null!;
        playerDataManager = null!;
    }

    public void SpawnPlayers(List<Vector3> spawnPositions)
    {
        foreach (var playerRef in Runner.ActivePlayers)
        {
            RPC_AddSpawnPlayerAction(playerRef, spawnPositions[playerRef % spawnPositions.Count]);
        }
    }

    public override void OnGameLoopPhase(GameLoopPhase phase)
    {
        foreach (var spawnPosition in spawnActions)
        {
            SpawnLocalPlayer(spawnPosition);
        }

        spawnActions.Clear();
    }

    public override IEnumerable<GameLoopPhase> GetSubscribePhases()
    {
        return gameLoopPhases;
    }

    private void SpawnLocalPlayer(Vector3 spawnPosition)
    {
        var playerController = Runner.Spawn(
            prefab: playerPrefab,
            position: spawnPosition,
            rotation: Quaternion.identity,
            inputAuthority: Runner.LocalPlayer,
            onBeforeSpawned: (networkRunner, playerObject) =>
            {
                var playerController = playerObject.GetComponent<PlayerController>();

                playerController.Init(heroConfigsBank.GetHeroConfigKey(playerDataManager.SelectedHeroId));
                playerController.OnPlayerStateChangedEvent.AddListener(OnLocalPlayerStateChanged);
            }
        );
        playerCamera.SetTarget(playerController.GetComponent<NetworkTransform>().InterpolationTarget.transform);
    }

    private void OnLocalPlayerStateChanged(
        PlayerRef playerRef,
        PlayerController playerController,
        PlayerState playerState
    )
    {
        switch (playerState)
        {
            case PlayerState.Spawning:
                playerController.Active();
                break;
            case PlayerState.Active:
                RPC_OnPlayerSpawned(Runner.LocalPlayer);
                break;
            case PlayerState.Dead:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerState), playerState, null);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AddSpawnPlayerAction([RpcTarget] PlayerRef playerRef, Vector3 spawnPosition)
    {
        spawnActions.Add(spawnPosition);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_OnPlayerSpawned(PlayerRef spawnedPlayerRef)
    {
        spawnedPlayers.Add(spawnedPlayerRef);

        var isAllPlayersSpawned = true;
        foreach (var playerRef in Runner.ActivePlayers)
        {
            isAllPlayersSpawned &= spawnedPlayers.Contains(playerRef);
        }

        if (isAllPlayersSpawned)
        {
            OnAllPlayersSpawned.Invoke();
        }
    }
}
}