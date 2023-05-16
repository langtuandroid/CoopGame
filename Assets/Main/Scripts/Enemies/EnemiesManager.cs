using System;
using Fusion;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Main.Scripts.Enemies
{
    public class EnemiesManager : NetworkBehaviour
    {
        public static EnemiesManager? Instance { get; private set; }

        [SerializeField]
        private EnemyController enemyPrefab = default!;

        public UnityEvent<EnemyController> OnEnemySpawnedEvent = default!;
        public UnityEvent<EnemyController> OnEnemyDeadEvent = default!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void SpawnEnemy(Vector3 targetPosition)
        {
            if (!Runner.IsServer) return;
            
            Runner.Spawn(
                prefab: enemyPrefab,
                position: targetPosition + new Vector3(Random.Range(-25, 25), 0, Random.Range(-25, 25)),
                onBeforeSpawned: (runner, networkObject) =>
                {
                    var enemyController = networkObject.GetComponent<EnemyController>();
                    enemyController.ResetState();
                    enemyController.OnDeadEvent.AddListener(OnEnemyDead);
                    OnEnemySpawnedEvent.Invoke(enemyController);
                });
        }

        private void OnEnemyDead(EnemyController enemyController)
        {
            enemyController.OnDeadEvent.RemoveListener(OnEnemyDead);
            OnEnemyDeadEvent.Invoke(enemyController);
        }
    }
}