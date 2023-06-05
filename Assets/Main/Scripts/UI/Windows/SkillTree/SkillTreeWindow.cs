using Main.Scripts.Core.Mvp;
using Main.Scripts.Core.Resources;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillTreeWindow : MvpMonoBehavior<SkillTreeContract.SkillTreePresenter>, UIScreen, SkillTreeContract.SkillTreeView
    {
        private SkillTreeViewContainer skillTreeViewContainer = default!;
        protected override SkillTreeContract.SkillTreePresenter? presenter { get; set; }

        private void Awake()
        {
            skillTreeViewContainer = new SkillTreeViewContainer(
                doc: GetComponent<UIDocument>(),
                skillInfoHolder: GlobalResources.Instance.ThrowWhenNull().SkillInfoHolder
            );
        }

        public void Open()
        {
            if (presenter == null)
            {
                presenter = new SkillTreePresenterImpl(
                    playerDataManager: PlayerDataManager.Instance.ThrowWhenNull(),
                    view: this
                );

                skillTreeViewContainer.OnResetSkillPoints = presenter.ResetSkillPoints;
                skillTreeViewContainer.OnIncreaseSkillLevel = presenter.IncreaseSkillLevel;
                skillTreeViewContainer.OnDecreaseSkillLevel = presenter.DecreaseSkillLevel;
            }

            presenter.Show();
        }

        public void Close()
        {
            presenter?.Hide();
        }

        public void SetVisibility(bool isVisible)
        {
            skillTreeViewContainer.SetVisibility(isVisible);
        }

        public void Bind(PlayerData playerData)
        {
            skillTreeViewContainer.Bind(playerData);
        }
    }
}