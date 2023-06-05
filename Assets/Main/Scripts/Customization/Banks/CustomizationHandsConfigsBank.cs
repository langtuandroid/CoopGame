using Main.Scripts.Customization.Configs;

namespace Main.Scripts.Customization.Banks
{
    public class CustomizationHandsConfigsBank : CustomizationConfigsBankBase<CustomizationHandsItemConfig>
    {
        protected override string GetResourcesPath()
        {
            return "Scriptable/Customizations/Hands";
        }
    }
}