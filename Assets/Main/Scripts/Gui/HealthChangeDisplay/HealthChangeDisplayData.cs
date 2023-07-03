using Fusion;

namespace Main.Scripts.Gui.HealthChangeDisplay
{
    public struct HealthChangeDisplayData : INetworkStruct
    {
        public const int TICK_BUFFER_LENGTH = 10;

        [Networked]
        [Capacity(TICK_BUFFER_LENGTH)]
        public NetworkArray<float> damageSumBuffer => default!;
        [Networked]
        [Capacity(TICK_BUFFER_LENGTH)]
        public NetworkArray<float> healSumBuffer => default!;
    }
}