using System;
using Fusion;
using Main.Scripts.Room;
using Main.Scripts.Utils;

namespace Main.Scripts.UI.Windows.LevelResults
{
    public class LevelResultsPresenter : NetworkBehaviour, UIScreen
    {
        private LevelResultsView levelResultsView = default!;
        private Lazy<RoomManager> roomManagerLazy = new(() => FindObjectOfType<RoomManager>().ThrowWhenNull());
        private RoomManager roomManager => roomManagerLazy.Value;

        private void Awake()
        {
            levelResultsView = GetComponent<LevelResultsView>();
        }

        public void Show()
        {
            var userId = roomManager.GetUserId(Runner.LocalPlayer);
            var levelResults = roomManager.GetLevelResults(userId).ThrowWhenNull();

            levelResultsView.Bind(levelResults.Value);
            levelResultsView.SetVisibility(true);
        }

        public void Hide()
        {
            levelResultsView.SetVisibility(false);
        }
    }
}