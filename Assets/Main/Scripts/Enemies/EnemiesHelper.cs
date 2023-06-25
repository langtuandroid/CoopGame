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
        [SerializeField]
        private NavigationManager navigationManager = default!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public PlayerRef? FindPlayerTarget(Vector3 fromPosition)
        {
            var targetPosition = (Vector3?)null;
            var targetRef = (PlayerRef?)null;
            foreach (var playerRef in playersHolder.GetKeys())
            {
                var playerPosition = playersHolder.Get(playerRef).transform.position;
                if (targetPosition == null || Vector3.Distance(fromPosition, playerPosition) < Vector3.Distance(fromPosition, targetPosition.Value))
                {
                    targetPosition = playerPosition;
                    targetRef = playerRef;
                }
            }

            return targetRef;
        }

        public void StartCalculatePath(NetworkId id, Vector3 fromPosition, Vector3 toPosition)
        {
            navigationManager.StartCalculatePath(id, fromPosition, toPosition);
        }

        public Vector3[]? GetPathCorners(NetworkId id)
        {
            return navigationManager.GetPathCorners(id);
        }

        public void StopCalculatePath(NetworkId id)
        {
            navigationManager.StopCalculatePath(id);
        }
    }
}