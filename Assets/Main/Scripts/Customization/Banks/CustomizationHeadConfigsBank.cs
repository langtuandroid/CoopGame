using Main.Scripts.Customization.Configs;

namespace Main.Scripts.Customization.Banks
{
    public class CustomizationHeadConfigsBank : CustomizationConfigsBankBase<CustomizationHeadItemConfig>
    {
        protected override string GetResourcesPath()
        {
            return "Scriptable/Customizations/Head";
        }
    }
}