using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.HUD.PowerChargeInfo
{
    public class PowerChargeInfoView
    {
        private ProgressBar chargeProgressBar;

        public PowerChargeInfoView(UIDocument doc)
        {
            var root = doc.rootVisualElement;
            chargeProgressBar = root.Q<ProgressBar>("PowerChargeInfo");
        }

        public void SetPowerChargeInfo(bool isShow, int level, int progress)
        {
            chargeProgressBar.visible = isShow;
            chargeProgressBar.value = progress;
            chargeProgressBar.title = $"Power {level}";
        }
    }
}