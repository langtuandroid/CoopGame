using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.HUD.HotBar.HotBarIcons
{
    public class HotBarItemView
    {
        private Label cooldownLeftLabel;

        public HotBarItemView(VisualElement view)
        {
            cooldownLeftLabel = view.Q<Label>("CooldownLeftText");
        }

        public void Bind(HotBarItemData data)
        {
            cooldownLeftLabel.style.backgroundImage = new StyleBackground(data.Icon.Image);
            UpdateCooldown(data.CooldownLeftSec);
        }

        public void UpdateCooldown(int cooldownLeftSec)
        {
            if (cooldownLeftSec == 0)
            {
                cooldownLeftLabel.text = "";
                cooldownLeftLabel.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
                return;
            }

            cooldownLeftLabel.text = cooldownLeftSec.ToString();
            cooldownLeftLabel.style.unityBackgroundImageTintColor = new StyleColor(Color.grey);
        }
    }
}