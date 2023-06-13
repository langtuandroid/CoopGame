using Fusion;
using Main.Scripts.Utils;

namespace Main.Scripts.Enemies
{
    public class EnemyRegistration : NetworkBehaviour
    {
        private EnemiesManager enemiesManager = default!;

        public override void Spawned()
        {
            enemiesManager = EnemiesManager.Instance.ThrowWhenNull();
            enemiesManager.RegisterEnemy(Object);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            enemiesManager.UnregisterEnemy(Object);
            enemiesManager = default!;
        }
    }
}