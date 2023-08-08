using Fusion;

namespace Main.Scripts.Effects
{
    public struct ActiveEffectData : INetworkStruct
    {
        public readonly int EffectId;
        public readonly int StartTick;
        public readonly int EndTick;
        public readonly int StackCount;

        public ActiveEffectData(int effectId, int startTick, int endTick, int stackCount)
        {
            EffectId = effectId;
            StartTick = startTick;
            EndTick = endTick;
            StackCount = stackCount;
        }
    }
}