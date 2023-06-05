using Main.Scripts.Player.Data;
using Main.Scripts.Skills;

namespace Main.Scripts.UI.Windows.SkillTree
{
    public class SkillTreePresenterImpl : SkillTreeContract.SkillTreePresenter
    {
        private SkillTreeContract.SkillTreeView view;
        private PlayerDataManager playerDataManager;

        public SkillTreePresenterImpl(
            PlayerDataManager playerDataManager,
            SkillTreeContract.SkillTreeView view
        )
        {
            this.view = view;
            this.playerDataManager = playerDataManager;
        }

        private void OnPlayerDataChanged(UserId userId, PlayerData playerData, PlayerData oldPlayerData)
        {
            view.Bind(playerData);
        }

        public void Show()
        {
            playerDataManager.OnPlayerDataChangedEvent.AddListener(OnPlayerDataChanged);
            view.Bind(playerDataManager.LocalPlayerData);
            view.SetVisibility(true);
        }

        public void Hide()
        {
            playerDataManager.OnPlayerDataChangedEvent.RemoveListener(OnPlayerDataChanged);
            view.SetVisibility(false);
        }

        public void ResetSkillPoints()
        {
            playerDataManager.ResetSkillPoints();
        }

        public void IncreaseSkillLevel(SkillType skillType)
        {
            playerDataManager.IncreaseSkillLevel(skillType);
        }

        public void DecreaseSkillLevel(SkillType skillType)
        {
            playerDataManager.DecreaseSkillLevel(skillType);
        }
    }
}