using Fusion;
using Main.Scripts.Skills;

namespace Main.Scripts.Player
{
    public class PlayerData: NetworkBehaviour
    {
        [Networked]
        public int Level { get; set; }
        
        [Networked]
        public int Experience { get; set; }
        
        [Networked, Capacity(16)]
        public NetworkDictionary<SkillType, int> SkillLevels => default;

        [Networked]
        public int MaxSkillPoints { get; set; }
        
        [Networked]
        public int AvailableSkillPoints { get; set; }
    }
}