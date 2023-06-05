using Main.Scripts.Customization.Configs;

namespace Main.Scripts.Customization.Banks
{
    public class CustomizationFootsConfigsBank : CustomizationConfigsBankBase<CustomizationFootsItemConfig>
    {
        protected override string GetResourcesPath()
        {
            return "Scriptable/Customizations/Foots";
        }
    }
}