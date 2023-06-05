using Main.Scripts.Customization.Configs;

namespace Main.Scripts.Customization.Banks
{
    public class CustomizationBodyConfigsBank : CustomizationConfigsBankBase<CustomizationBodyItemConfig>
    {
        protected override string GetResourcesPath()
        {
            return "Scriptable/Customizations/Body";
        }
    }
}