using Fusion;

namespace Main.Scripts.Player.Data
{
    public struct AwardsData : INetworkStruct
    {
        public NetworkBool IsSuccess;
        public uint Experience;
    }
}