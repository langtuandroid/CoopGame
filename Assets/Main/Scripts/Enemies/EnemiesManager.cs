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
        public static EnemiesManager? Instance { get; private set; }

        [SerializeField]
        private EnemyController enemyPrefab = default!;

        public UnityEvent<EnemyController> OnEnemySpawnedEvent = default!;
        public UnityEvent<EnemyController> OnEnemyDeadEvent = default!;

        private Dictionary<NetworkId, NetworkObject> localEnemiesMap = new();

        private List<Vector3> spawnActions = new();

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.ObjectsSpawnPhase
        };

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        public override void Spawned()
        {
            base.Spawned();
            spawnActions.Clear();
        }

        private void OnDestroy()
        {
            Instance = null;
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
                        OnEnemySpawnedEvent.Invoke(enemyController);
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

        public IEnumerable<NetworkObject> GetEnemies()
        {
            return localEnemiesMap.Values;
        }

        public void RegisterEnemy(NetworkObject enemy)
        {
            localEnemiesMap.Add(enemy.Id, enemy);
        }

        public void UnregisterEnemy(NetworkObject enemy)
        {
            localEnemiesMap.Remove(enemy.Id);
        }

        private void OnEnemyDead(EnemyController enemyController)
        {
            enemyController.OnDeadEvent.RemoveListener(OnEnemyDead);
            OnEnemyDeadEvent.Invoke(enemyController);
        }
    }
}