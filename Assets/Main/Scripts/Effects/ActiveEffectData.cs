using Fusion;

namespace Main.Scripts.Effects
{
    public struct ActiveEffectData : INetworkStruct
    {
        public readonly int EffectId;
        public readonly int EndTick;

        public ActiveEffectData(int effectId, int endTick)
        {
            EffectId = effectId;
            EndTick = endTick;
        }
    }
}