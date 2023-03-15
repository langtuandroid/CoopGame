using Fusion;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;

namespace Main.Scripts.UI.Windows.LevelResults
{
    public class LevelResultsPresenter : NetworkBehaviour, UIScreen
    {
        private LevelResultsView levelResultsView = default!;
        private PlayerDataManager playerDataManager = default!;

        private void Awake()
        {
            levelResultsView = GetComponent<LevelResultsView>();
        }

        public override void Spawned()
        {
            playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
        }

        public void Show()
        {
            var awardsData = playerDataManager.LocalAwardsData;
            awardsData.ThrowWhenNull();

            levelResultsView.Bind(awardsData.Value);
            levelResultsView.SetVisibility(true);
        }

        public void Hide()
        {
            levelResultsView.SetVisibility(false);
        }
    }
}