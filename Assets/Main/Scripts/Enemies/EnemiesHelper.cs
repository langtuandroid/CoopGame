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

        private int[]? playerRefs;
        private Vector3[]? playerPositions;
        private Tick cachedTick;

        private PlayersHolderListener playersHolderListener = default!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
            
            playersHolderListener = new PlayersHolderListener(this);
            
            playersHolder.AddListener(playersHolderListener);
        }

        private void OnDestroy()
        {
            Instance = null;
            playersHolder.RemoveListener(playersHolderListener);
        }

        public PlayerRef? FindPlayerTarget(NetworkRunner runner, Vector3 fromPosition, out Vector3 targetPosition)
        {
            targetPosition = Vector3.zero;
            var targetRef = (PlayerRef?)null;
            //todo allocating in Map
            playerRefs ??= playersHolder.GetKeys().Map(it => it.PlayerId);
            if (cachedTick != runner.Tick)
            {
                cachedTick = runner.Tick;
                //todo allocating in Map
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

        private class PlayersHolderListener : PlayersHolder.Listener
        {
            private EnemiesHelper enemiesHelper;

            public PlayersHolderListener(EnemiesHelper enemiesHelper)
            {
                this.enemiesHelper = enemiesHelper;
            }

            public void OnAdded(PlayerRef playerRef)
            {
                enemiesHelper.playerRefs = null;
            }

            public void OnRemoved(PlayerRef playerRef)
            {
                enemiesHelper.playerRefs = null;
            }
        }
    }
}