namespace Main.Scripts.UI.Windows.DebugPanel.Data
{
    public class ModifierItemData
    {
        public readonly string Name;
        public readonly bool IsEnabled;

        public ModifierItemData(string name, bool isEnabled)
        {
            Name = name;
            IsEnabled = isEnabled;
        }
    }
}