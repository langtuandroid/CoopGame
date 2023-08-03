using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Levels;
using Main.Scripts.Player;
using Main.Scripts.Utils;

namespace Main.Scripts.Core.Simulation
{
    [OrderBefore(typeof(GameLoopManager))]
    public class ReceiveTicksManager : NetworkBehaviour,
        IAfterSpawned,
        IBeforeTick
    {
        private const int MAX_PLAYERS_COUNT = 4;

        private PlayersHolder playersHolder = default!;

        [Networked, Capacity(MAX_PLAYERS_COUNT)]
        private NetworkArray<Tick> lastReceiveTickFromClients => default;

        public void AfterSpawned()
        {
            playersHolder = LevelContext.Instance.ThrowWhenNull().PlayersHolder;
        }

        public void BeforeTick()
        {
            if (!HasStateAuthority) return;

            foreach (var playerRef in playersHolder.GetKeys())
            {
                if (playerRef == Object.StateAuthority)
                {
                    lastReceiveTickFromClients.Set(playerRef.PlayerId, Runner.Tick);
                }
                else
                {
                    var receiveTicksManager = playersHolder.Get(playerRef)
                        .GetInterface<ReceiveTicksManager>()
                        .ThrowWhenNull();
                    lastReceiveTickFromClients.Set(playerRef.PlayerId,
                        receiveTicksManager.GetLastReceiveTick(playerRef));
                }
            }
        }

        public Tick GetLastReceiveTick(PlayerRef playerRef)
        {
            return lastReceiveTickFromClients[playerRef.PlayerId];
        }
    }
}