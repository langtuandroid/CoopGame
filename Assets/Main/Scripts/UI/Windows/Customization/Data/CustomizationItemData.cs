namespace Main.Scripts.UI.Windows.Customization.Data
{
    public class CustomizationItemData
    {
        public readonly string Name;
        public readonly bool IsSelected;

        public CustomizationItemData(string name, bool isSelected)
        {
            Name = name;
            IsSelected = isSelected;
        }
    }
}