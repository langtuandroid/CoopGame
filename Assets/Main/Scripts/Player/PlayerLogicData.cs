using Fusion;
using UnityEngine;

namespace Main.Scripts.Player
{
    public struct PlayerLogicData : INetworkStruct
    {
        [Networked]
        public int heroConfigKey { get; set; }
        [Networked]
        public PlayerState state { get; set; }
        [Networked]
        public float maxHealth { get; set; }
        [Networked]
        public float health { get; set; }
        [Networked]
        public float speed { get; set; }
        [Networked]
        public int gold { get; set; }
        [Networked]
        public Vector2 moveDirection { get; set; }
        [Networked]
        public Vector2 aimDirection { get; set; }
        [Networked]
        public TickTimer dashTimer { get; set; }
        [Networked]
        public float dashSpeed { get; set; }
        [Networked]
        public Vector3 dashDirection { get; set; }
        [Networked]
        public PlayerAnimationState lastAnimationState { get; set; }
        [Networked]
        public int animationTriggerId { get; set; }
    }
}