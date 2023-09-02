using Fusion;
using UnityEngine;

namespace Main.Scripts.Enemies
{
    public struct EnemyData : INetworkStruct
    {
        [Networked]
        public float health { get; set; }
        [Networked]
        public float maxHealth { get; set; }
        [Networked]
        public float speed { get; set; }
        [Networked]
        public NetworkBool isDead { get; set; }
        [Networked]
        public Vector3 navigationTarget { get; set; }
        [Networked]
        public TickTimer stunTimer { get; set; }
        [Networked]
        public TickTimer knockBackTimer { get; set; }
        [Networked]
        public Vector3 knockBackDirection { get; set; }
        [Networked]
        public PlayerRef targetPlayerRef { get; set; }
        [Networked]
        public int animationTriggerId { get; set; }
    }
}