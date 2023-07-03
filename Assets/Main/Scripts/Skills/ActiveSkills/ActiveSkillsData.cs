using Fusion;
using UnityEngine;

namespace Main.Scripts.Skills.ActiveSkills
{
    public struct ActiveSkillsData : INetworkStruct
    {
        [Networked]
        public ActiveSkillState currentSkillState { get; set; }
        [Networked]
        public ActiveSkillType currentSkillType { get; set; }
        [Networked]
        public Vector3 targetMapPosition { get; set; }
        [Networked]
        public NetworkId unitTargetId { get; set; }
    }
}