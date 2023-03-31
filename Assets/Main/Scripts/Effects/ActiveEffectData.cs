using Fusion;

namespace Main.Scripts.Effects
{
    public struct ActiveEffectData : INetworkStruct
    {
        public readonly int EffectId;
        public readonly int EndTick;
        public readonly int StackCount;

        public ActiveEffectData(int effectId, int endTick, int stackCount)
        {
            EffectId = effectId;
            EndTick = endTick;
            StackCount = stackCount;
        }
    }
}