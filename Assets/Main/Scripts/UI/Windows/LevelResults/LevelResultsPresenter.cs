using Fusion;
using Main.Scripts.Room;
using UnityEngine;

namespace Main.Scripts.UI.Windows.LevelResults
{
    public class LevelResultsPresenter : NetworkBehaviour, WindowObject
    {
        private LevelResultsView levelResultsView;
        private RoomManager roomManager;

        private void Awake()
        {
            levelResultsView = GetComponent<LevelResultsView>();
            roomManager = FindObjectOfType<RoomManager>();
        }

        public void Show()
        {
            levelResultsView.SetVisibility(true);
            var levelResults = roomManager.GetLevelResults(Runner.LocalPlayer);
            if (levelResults == null)
            {
                Debug.LogError("levelResults is null");
                return;
            }

            levelResultsView.Bind(levelResults.Value);
        }

        public void Hide()
        {
            levelResultsView.SetVisibility(false);
        }
    }
}