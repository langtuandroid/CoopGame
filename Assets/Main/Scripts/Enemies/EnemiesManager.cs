using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Core.Resources;
using Main.Scripts.Mobs.Config;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Main.Scripts.Enemies
{
    public class EnemiesManager : GameLoopEntityNetworked
    {
        [SerializeField]
        private EnemyController enemyPrefab = null!;
        [SerializeField]
        private MobConfig mobConfig = null!;

        private NavigationManager navigationManager = null!;
        private MobConfigsBank mobConfigsBank = null!;

        public UnityEvent<EnemyController> OnEnemyDeadEvent = null!;

        private Dictionary<NetworkId, EnemyController> localEnemiesMap = new();

        private List<Vector3> spawnActions = new();

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.ObjectsSpawnPhase
        };

        public override void Spawned()
        {
            base.Spawned();
            spawnActions.Clear();

            navigationManager = levelContext.NavigationManager;
            mobConfigsBank = GlobalResources.Instance.ThrowWhenNull().MobConfigsBank;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            navigationManager = null!;
            mobConfigsBank = null!;
        }

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            foreach (var targetPosition in spawnActions)
            {
                Runner.Spawn(
                    prefab: enemyPrefab,
                    position: targetPosition + new Vector3(Random.Range(-25, 25), 0, Random.Range(-25, 25)),
                    onBeforeSpawned: (runner, networkObject) =>
                    {
                        var enemyController = networkObject.GetComponent<EnemyController>();
                        enemyController.Init(mobConfigsBank.GetMobConfigKey(mobConfig));
                        enemyController.OnDeadEvent.AddListener(OnEnemyDead);
                    }
                );
            }
            spawnActions.Clear();
        }

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
        }

        public void SpawnEnemy(Vector3 targetPosition)
        {
            spawnActions.Add(targetPosition);
        }

        public IEnumerable<EnemyController> GetEnemies()
        {
            return localEnemiesMap.Values;
        }

        public void RegisterEnemy(EnemyController enemy)
        {
            localEnemiesMap.Add(enemy.Object.Id, enemy);
            navigationManager.Add(enemy.Object);
        }

        public void UnregisterEnemy(EnemyController enemy)
        {
            localEnemiesMap.Remove(enemy.Object.Id);
            if (navigationManager != null)
            {
                navigationManager.Remove(enemy.Object);
            }
        }

        private void OnEnemyDead(EnemyController enemyController)
        {
            enemyController.OnDeadEvent.RemoveListener(OnEnemyDead);
            RPC_OnEnemyDead(enemyController.Object.Id);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_OnEnemyDead(NetworkId enemyId)
        {
            if (!localEnemiesMap.ContainsKey(enemyId)) return;
            
            OnEnemyDeadEvent.Invoke(localEnemiesMap[enemyId]);
        }
    }
}