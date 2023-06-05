using Main.Scripts.Customization.Configs;

namespace Main.Scripts.Customization.Banks
{
    public class CustomizationFullSetConfigsBank : CustomizationConfigsBankBase<CustomizationFullSetItemConfig>
    {
        protected override string GetResourcesPath()
        {
            return "Scriptable/Customizations/FullSet";
        }
    }
}