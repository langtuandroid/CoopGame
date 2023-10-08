using Main.Scripts.Core.Mvp;
using Main.Scripts.Core.Resources;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillTreeWindow : MvpMonoBehavior<SkillTreeContract.SkillTreePresenter>, UIScreen, SkillTreeContract.SkillTreeView
    {
        [SerializeField]
        private VisualTreeAsset skillInfoLayout = null!;
        
        private SkillTreeViewContainer skillTreeViewContainer = default!;
        protected override SkillTreeContract.SkillTreePresenter? presenter { get; set; }

        private void Awake()
        {
            skillTreeViewContainer = new SkillTreeViewContainer(
                doc: GetComponent<UIDocument>(),
                skillInfoLayout: skillInfoLayout
            );
        }

        public void Open()
        {
            if (presenter == null)
            {
                var globalResources = GlobalResources.Instance.ThrowWhenNull();
                presenter = new SkillTreePresenterImpl(
                    playerDataManager: PlayerDataManager.Instance.ThrowWhenNull(),
                    skillInfoBank: globalResources.SkillInfoBank,
                    modifierIdsBank: globalResources.ModifierIdsBank,
                    view: this
                );

                skillTreeViewContainer.OnResetSkillPoints = presenter.ResetSkillPoints;
                skillTreeViewContainer.OnIncreaseSkillLevelClicked = presenter.OnIncreaseSkillLevelClicked;
                skillTreeViewContainer.OnDecreaseSkillLevelClicked = presenter.OnDecreaseSkillLevelClicked;
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

        public void Bind(PlayerInfoData playerInfoData)
        {
            skillTreeViewContainer.Bind(playerInfoData);
        }
    }
}