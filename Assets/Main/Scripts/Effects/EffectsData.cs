using Fusion;
using Main.Scripts.Effects.Stats;

namespace Main.Scripts.Effects
{
    public struct EffectsData : INetworkStruct
    {
        [Networked]
        [Capacity(EffectsBank.UNLIMITED_EFFECTS_COUNT)]
        public NetworkDictionary<int, ActiveEffectData> unlimitedEffectDataMap => default;

        [Networked]
        [Capacity(EffectsBank.LIMITED_EFFECTS_COUNT)]
        public NetworkDictionary<int, ActiveEffectData> limitedEffectDataMap => default;

        [Networked]
        [Capacity((int)StatType.ReservedDoNotUse)]
        public NetworkArray<float> statConstAdditiveSums => default;

        [Networked]
        [Capacity((int)StatType.ReservedDoNotUse)]
        public NetworkArray<float> statPercentAdditiveSums => default;
    }
}