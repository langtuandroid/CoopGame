using Fusion;

namespace Main.Scripts.Levels.Results
{
    public struct LevelResultsData : INetworkStruct
    {
        public NetworkBool IsSuccess;
        public uint Experience;
    }
}