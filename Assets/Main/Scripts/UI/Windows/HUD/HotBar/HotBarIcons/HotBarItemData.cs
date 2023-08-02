namespace Main.Scripts.UI.Windows.HUD.HotBar.HotBarIcons
{
    public struct HotBarItemData
    {
        public HotBarIconData Icon;
        public int CooldownLeftSec;

        public HotBarItemData(HotBarIconData icon, int cooldownLeftSec)
        {
            Icon = icon;
            CooldownLeftSec = cooldownLeftSec;
        }
    }
}