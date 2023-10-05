using Fusion;
using UnityEngine;

namespace Main.Scripts.Mobs.Component
{
    public struct MobBlockContext
    {
        public NetworkObject SelfUnit;
        public NetworkObject? TargetUnit;
        public int Tick;

        public LayerMask AlliesLayerMask;
        public LayerMask OpponentsLayerMask;
    }
}