using Main.Scripts.Core.Mvp;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.UI.Windows.HUD.HotBar;

namespace Main.Scripts.UI.Windows.HUD
{
    public interface HUDContract : MvpContract
    {
        public interface HotBarPresenter : Presenter
        {
            void OnOpen();
            void OnClose();
        }

        public interface HotBarView : View<HotBarPresenter>
        {
            void Bind(ref HotBarData hotBarData);
            void SetVisibility(bool isVisible);
            void UpdateSkillCooldown(ActiveSkillType skillType, int cooldownLeftSec);
        }
    }
}