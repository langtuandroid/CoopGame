using Fusion;
using Main.Scripts.Enemies;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Scenarios.Missions
{
    public class KillTargetsCountMissionScenario : NetworkBehaviour
    {
        [SerializeField]
        private int targetKillsCount = 10;
        
        private EnemiesManager enemiesManager = default!;
        
        [Networked]
        private int killsCount { get; set; }

        public UnityEvent OnScenarioFinishedEvent = default!;

        public bool IsFinished => killsCount >= targetKillsCount;

        public override void Spawned()
        {
            enemiesManager = EnemiesManager.Instance.ThrowWhenNull();
            enemiesManager.OnEnemyDeadEvent.AddListener(OnEnemyDead);
        }

        private void OnEnemyDead(EnemyController enemyController)
        {
            killsCount++;
            Debug.Log($"Enemies kills count: {killsCount}");
            if (IsFinished)
            {
                enemiesManager.OnEnemyDeadEvent.RemoveListener(OnEnemyDead);
                OnScenarioFinishedEvent.Invoke();
            }
        }
    }
}