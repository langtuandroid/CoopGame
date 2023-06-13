using Fusion;
using Main.Scripts.Enemies;
using Main.Scripts.Player;
using UnityEngine;

namespace Main.Scripts.Levels
{
    public class LevelContext : MonoBehaviour
    {
        public static LevelContext? Instance { get; private set; }

        [SerializeField]
        private PlayersHolder playersHolder = default!;
        [SerializeField]
        private EnemiesManager enemiesManager = default!;

        public PlayersHolder PlayersHolder => playersHolder;
        public EnemiesManager EnemiesManager => enemiesManager;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}