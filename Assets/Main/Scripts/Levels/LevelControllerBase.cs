using System;
using Fusion;
using Main.Scripts.Room;
using Main.Scripts.Utils;

namespace Main.Scripts.Levels
{
    public abstract class LevelControllerBase : NetworkBehaviour
    {
        private Lazy<RoomManager> roomManagerLazy = new(
            () => FindObjectOfType<RoomManager>().ThrowWhenNull()
        );
        protected RoomManager roomManager => roomManagerLazy.Value;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                var connectedPlayers = roomManager.GetConnectedPlayers();
                foreach (var playerRef in connectedPlayers)
                {
                    if (roomManager.IsPlayerInitialized(playerRef))
                    {
                        OnPlayerInitialized(playerRef);
                    }
                }

                roomManager.OnPlayerInitializedEvent.AddListener(OnPlayerInitialized);
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
    }
}