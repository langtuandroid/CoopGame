using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.HUD.ChargeInfo
{
    public class ChargeInfoView
    {
        private Label chargeLevelLabel;
        private ProgressBar chargeProgressBar;

        public ChargeInfoView(UIDocument doc)
        {
            var root = doc.rootVisualElement;
            chargeLevelLabel = root.Q<Label>("ChargeLevel");
            chargeProgressBar = root.Q<ProgressBar>("ChargeProgress");
        }

        public void SetChargeInfo(int level, int progress, int progressTarget, bool isMaxLevel)
        {
            chargeLevelLabel.text = $"Heat lvl: {level}";
            if (isMaxLevel)
            {
                chargeProgressBar.value = 100;
                chargeProgressBar.title = "MAX Heat";
            }
            else
            {
                chargeProgressBar.value = chargeProgressBar.highValue * progress / progressTarget;
                chargeProgressBar.title = $"{progress}/{progressTarget}";
            }
        }
    }
}