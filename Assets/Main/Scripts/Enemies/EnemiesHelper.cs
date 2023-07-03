using Fusion;
using Main.Scripts.Player;
using UnityEngine;
using UnityEngine.Profiling;

namespace Main.Scripts.Enemies
{
    public class EnemiesHelper : MonoBehaviour
    {
        public static EnemiesHelper? Instance { get; private set; }
        
        [SerializeField]
        private PlayersHolder playersHolder = default!;
        [SerializeField]
        private NavigationManager navigationManager = default!;

        private int[]? playerRefs;
        private Vector3[]? playerPositions;
        private Tick cachedTick;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
            
            playersHolder.OnChangedEvent.AddListener(OnPlayerHolderChanged);
        }

        private void OnDestroy()
        {
            Instance = null;
            playersHolder.OnChangedEvent.RemoveListener(OnPlayerHolderChanged);
        }

        public PlayerRef? FindPlayerTarget(NetworkRunner runner, Vector3 fromPosition, out Vector3 targetPosition)
        {
            targetPosition = Vector3.zero;
            var targetRef = (PlayerRef?)null;
            playerRefs ??= playersHolder.GetKeys().Map(it => it.PlayerId);
            if (cachedTick != runner.Tick)
            {
                cachedTick = runner.Tick;
                playerPositions = playerRefs.Map(playerRef => playersHolder.Get(playerRef).transform.position);
            }

            if (playerPositions == null) return null;

            for (var i = 0; i < playerRefs.Length; i++)
            {
                var playerRef = playerRefs[i];
                var playerPosition = playerPositions[i];
                if (i == 0 || Vector3.SqrMagnitude(fromPosition -  playerPosition) < Vector3.SqrMagnitude(fromPosition - targetPosition))
                {
                    targetPosition = playerPosition;
                    targetRef = playerRef;
                }
            }

            return targetRef;
        }

        public void StartCalculatePath(ref NetworkId id, Vector3 fromPosition, Vector3 toPosition)
        {
            navigationManager.StartCalculatePath(ref id, fromPosition, toPosition);
        }

        public Vector3[] GetPathCorners(ref NetworkId id)
        {
            return navigationManager.GetPathCorners(ref id);
        }

        public void StopCalculatePath(NetworkId id)
        {
            navigationManager.StopCalculatePath(ref id);
        }

        private void OnPlayerHolderChanged()
        {
            playerRefs = null;
        }
    }
}