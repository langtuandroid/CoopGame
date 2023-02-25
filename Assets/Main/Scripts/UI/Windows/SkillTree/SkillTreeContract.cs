using Main.Scripts.Core.Mvp;
using Main.Scripts.Player.Data;
using Main.Scripts.Skills;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public interface SkillTreeContract : MvpContract
    {
        interface SkillTreePresenter : Presenter
        {
            void Show();
            void Hide();
            void ResetSkillPoints();
            void IncreaseSkillLevel(SkillType skillType);
            void DecreaseSkillLevel(SkillType skillType);
        }

        public interface SkillTreeView : View<SkillTreePresenter>
        {
            void Bind(PlayerData playerData);
            void SetVisibility(bool isVisible);
        }
    }
}