using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Core.Resources;
using Main.Scripts.Player;
using Main.Scripts.Skills.Common.Component;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Visual;
using Main.Scripts.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Main.Scripts.Skills.Common
{
    public class SkillVisualManager : GameLoopEntityNetworked
    {
        private PlayersHolder playersHolder = default!;
        private SkillConfigsBank skillConfigsBank = default!;

        private Dictionary<int, Object> despawnWaitingObjects = new();
        private Dictionary<PlayerRef, HashSet<int>> tokensByPlayer = new();
        private int nextToken;

        private List<SpawnActionData> spawnActions = new();
        private List<int> tokensForDespawnActions = new();

        private PlayersHolderListener playersHolderListener = default!;

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.SkillVisualSpawnPhase,
            GameLoopPhase.DespawnPhase
        };

        private void Awake()
        {
            playersHolderListener = new PlayersHolderListener(this);
        }

        public override void Spawned()
        {
            base.Spawned();
            playersHolder = levelContext.PlayersHolder;
            playersHolder.AddListener(playersHolderListener);
            skillConfigsBank = GlobalResources.Instance.ThrowWhenNull().SkillConfigsBank;

            var playerRefs = playersHolder.GetKeys();
            foreach (var playerRef in playerRefs)
            {
                tokensByPlayer[playerRef] = new HashSet<int>();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            playersHolder.RemoveListener(playersHolderListener);
            playersHolder = default!;
            skillConfigsBank = default!;

            foreach (var (_, visualObject) in despawnWaitingObjects)
            {
                if (visualObject)
                {
                    Destroy(visualObject);
                }
            }


            despawnWaitingObjects.Clear();
            tokensByPlayer.Clear();
            nextToken = 0;
        }

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.SkillVisualSpawnPhase:
                    OnSkillVisualSpawnPhase();
                    break;
                case GameLoopPhase.DespawnPhase:
                    OnDespawnPhase();
                    break;
            }
        }

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
        }

        private void OnSkillVisualSpawnPhase()
        {
            foreach (var spawnActionData in spawnActions)
            {
                var spawnedObject = Instantiate(
                    original: spawnActionData.skillVisualConfig.PrefabToSpawn,
                    position: spawnActionData.spawnPosition,
                    rotation: Quaternion.LookRotation(spawnActionData.spawnDirection)
                );
                if (spawnActionData.skillVisualConfig.WaitDespawnByAction)
                {
                    despawnWaitingObjects[spawnActionData.token] = spawnedObject;
                }

                if (spawnedObject.TryGetComponent(out SkillVisualMovementComponent component))
                {
                    component.Init(spawnActionData.skillVisualConfig);
                }
            }

            spawnActions.Clear();
        }

        private void OnDespawnPhase()
        {
            foreach (var token in tokensForDespawnActions)
            {
                if (despawnWaitingObjects.ContainsKey(token))
                {
                    Destroy(despawnWaitingObjects[token]);
                    despawnWaitingObjects.Remove(token);
                }
            }

            tokensForDespawnActions.Clear();
        }

        public int StartVisual(SpawnSkillVisualAction spawnSkillConfig, Vector3 spawnPosition, Vector3 spawnDirection)
        {
            var token = -1;
            if (spawnSkillConfig.WaitDespawnByAction)
            {
                token = GetNewToken();
            }

            var configKey = skillConfigsBank.GetSkillVisualConfigKey(spawnSkillConfig);

            RPC_StartVisual(configKey, token, spawnPosition, spawnDirection);
            return token;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_StartVisual(int configKey, int token, Vector3 spawnPosition, Vector3 spawnDirection)
        {
            var config = skillConfigsBank.GetSkillVisualConfig(configKey);
            spawnActions.Add(new SpawnActionData
            {
                skillVisualConfig = config,
                spawnPosition = spawnPosition,
                spawnDirection = spawnDirection,
                token = token
            });
        }

        public void FinishVisual(int token)
        {
            RPC_FinishVisual(token);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_FinishVisual(int token)
        {
            tokensForDespawnActions.Add(token);
        }

        private int GetNewToken()
        {
            return nextToken++ << 4 + Runner.LocalPlayer;
        }

        private class PlayersHolderListener : PlayersHolder.Listener
        {
            private SkillVisualManager skillVisualManager;

            public PlayersHolderListener(SkillVisualManager skillVisualManager)
            {
                this.skillVisualManager = skillVisualManager;
            }

            public void OnAdded(PlayerRef playerRef)
            {
                skillVisualManager.tokensByPlayer[playerRef] = new HashSet<int>();
            }

            public void OnRemoved(PlayerRef playerRef)
            {
                if (skillVisualManager.tokensByPlayer.ContainsKey(playerRef))
                {
                    foreach (var token in skillVisualManager.tokensByPlayer[playerRef])
                    {
                        if (skillVisualManager.despawnWaitingObjects.ContainsKey(token))
                        {
                            Destroy(skillVisualManager.despawnWaitingObjects[token]);
                        }
                    }
                }

                skillVisualManager.tokensByPlayer.Remove(playerRef);
            }
        }

        private struct SpawnActionData
        {
            public SpawnSkillVisualAction skillVisualConfig;
            public Vector3 spawnPosition;
            public Vector3 spawnDirection;
            public int token;
        }
    }
}