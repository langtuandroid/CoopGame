using System;

namespace Main.Scripts.UI.Windows.DebugPanel.Data
{
    public class ModifierItemData
    {
        public readonly string Name;
        public readonly ushort Level;
        public readonly ushort MaxLevel;

        public ModifierItemData(string name, ushort level, ushort maxLevel)
        {
            Name = name;
            Level = level;
            MaxLevel = maxLevel;
        }
    }
}