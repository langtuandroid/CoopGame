using Fusion;
using Main.Scripts.Skills;

namespace Main.Scripts.Player
{
    public class PlayerData: NetworkBehaviour
    {
        [Networked, Capacity(16)]
        public NetworkDictionary<SkillType, int> SkillLevels => default;

        [Networked]
        public int MaxSkillPoints { get; set; }
        
        [Networked]
        public int AvailableSkillPoints { get; set; }
    }
}