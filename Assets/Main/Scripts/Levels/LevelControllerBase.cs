using Fusion;
using Main.Scripts.Player.Data;
using Main.Scripts.Room;
using Main.Scripts.Utils;

namespace Main.Scripts.Levels
{
    public abstract class LevelControllerBase : NetworkBehaviour
    {
        protected RoomManager roomManager = default!;
        protected PlayerDataManager playerDataManager = default!;

        [Networked]
        private NetworkBool isInitialized { get; set; }

        public override void Spawned()
        {
            roomManager = RoomManager.Instance.ThrowWhenNull();
            playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
            if (HasStateAuthority)
            {
                roomManager.OnPlayerInitializedEvent.AddListener(OnPlayerInitialized);
                roomManager.OnPlayerDisconnectedEvent.AddListener(OnPlayerDisconnected);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!isInitialized)
            {
                var connectedPlayers = Runner.ActivePlayers;
                foreach (var playerRef in connectedPlayers)
                {
                    if (roomManager.IsPlayerInitialized(playerRef))
                    {
                        OnPlayerInitialized(playerRef);
                    }
                }

                isInitialized = true;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (HasStateAuthority)
            {
                roomManager.OnPlayerInitializedEvent.RemoveListener(OnPlayerInitialized);
            }
        }

        protected abstract void OnPlayerInitialized(PlayerRef playerRef);

        protected abstract void OnPlayerDisconnected(PlayerRef playerRef);
    }
}