using Fusion;
using Main.Scripts.Player;
using UnityEngine;

namespace Main.Scripts.Enemies
{
    public class EnemiesHelper : MonoBehaviour
    {
        public static EnemiesHelper? Instance { get; private set; }
        
        [SerializeField]
        private PlayersHolder playersHolder = default!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public Vector3? findPlayerTarget(Vector3 fromPosition)
        {
            var target = (Vector3?)null;
            foreach (var playerRef in playersHolder.GetKeys())
            {
                var playerPosition = playersHolder.Get(playerRef).transform.position;
                if (target == null || Vector3.Distance(fromPosition, playerPosition) < Vector3.Distance(fromPosition, target.Value))
                {
                    target = playerPosition;
                }
            }

            return target;
        }
    }
}