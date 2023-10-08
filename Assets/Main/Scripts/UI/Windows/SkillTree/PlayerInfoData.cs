using System.Collections.Generic;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class PlayerInfoData
    {
        public uint Level;
        public uint Experience;
        public uint AvailableSkillPoints;
        public uint MaxSkillPoints;
        public List<SkillInfoData> SkillInfoDataList = null!;
    }
}