using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Effects
{
    [Serializable]
    public struct EffectsConfig
    {
        public LayerMask AlliesLayerMask;
        public LayerMask OpponentsLayerMask;
        public List<EffectsCombination> InitialEffects;
    }
}