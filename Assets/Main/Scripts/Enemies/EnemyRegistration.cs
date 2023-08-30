using Fusion;
using Main.Scripts.Utils;

namespace Main.Scripts.Enemies
{
    [SimulationBehaviour(
        Stages = (SimulationStages) 8,
        Modes  = (SimulationModes) 8
    )]
    public class EnemyRegistration : NetworkBehaviour
    {
        private EnemiesManager enemiesManager = default!;
        private EnemyController enemyController = default!;

        public override void Spawned()
        {
            enemyController = GetComponent<EnemyController>();
            enemiesManager = EnemiesManager.Instance.ThrowWhenNull();
            enemiesManager.RegisterEnemy(enemyController);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            enemiesManager.UnregisterEnemy(enemyController);
            enemiesManager = default!;
        }
    }
}