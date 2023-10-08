using Main.Scripts.Core.Mvp;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public interface SkillTreeContract : MvpContract
    {
        interface SkillTreePresenter : Presenter
        {
            void Show();
            void Hide();
            void ResetSkillPoints();
            void OnIncreaseSkillLevelClicked(SkillInfoData skillInfoData);
            void OnDecreaseSkillLevelClicked(SkillInfoData skillInfoData);
        }

        public interface SkillTreeView : View<SkillTreePresenter>
        {
            void Bind(PlayerInfoData playerInfoData);
            void SetVisibility(bool isVisible);
        }
    }
}