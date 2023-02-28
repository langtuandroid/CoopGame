using System;
using Fusion;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;

namespace Main.Scripts.UI.Windows.LevelResults
{
    public class LevelResultsPresenter : NetworkBehaviour, UIScreen
    {
        private LevelResultsView levelResultsView = default!;
        private Lazy<PlayerDataManager> playerDataManagerLazy = new(() => FindObjectOfType<PlayerDataManager>().ThrowWhenNull());
        private PlayerDataManager playerDataManager => playerDataManagerLazy.Value;

        private void Awake()
        {
            levelResultsView = GetComponent<LevelResultsView>();
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