using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Main.Scripts.Enemies
{
    public class EnemiesManager : GameLoopEntity
    {
        [SerializeField]
        private EnemyController enemyPrefab = default!;

        public UnityEvent<EnemyController> OnEnemyDeadEvent = default!;

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
        }

        public void UnregisterEnemy(EnemyController enemy)
        {
            localEnemiesMap.Remove(enemy.Object.Id);
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