using Main.Scripts.Customization.Configs;

namespace Main.Scripts.Customization.Banks
{
    public class CustomizationLegsConfigsBank : CustomizationConfigsBankBase<CustomizationLegsItemConfig>
    {
        protected override string GetResourcesPath()
        {
            return "Scriptable/Customizations/Legs";
        }
    }
}