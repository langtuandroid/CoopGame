using Fusion;
using Main.Scripts.Room;
using UnityEngine;

namespace Main.Scripts.UI.Windows.LevelResults
{
    public class LevelResultsPresenter : NetworkBehaviour, WindowObject
    {
        private LevelResultsWindow levelResultsWindow;
        private RoomManager roomManager;

        private void Awake()
        {
            levelResultsWindow = GetComponent<LevelResultsWindow>();
            roomManager = FindObjectOfType<RoomManager>();
        }

        public void Show()
        {
            levelResultsWindow.SetVisibility(true);
            var levelResults = roomManager.GetLevelResults(Runner.LocalPlayer);
            if (levelResults == null)
            {
                Debug.LogError("levelResults is null");
                return;
            }

            levelResultsWindow.Bind(levelResults.Value);
        }

        public void Hide()
        {
            levelResultsWindow.SetVisibility(false);
        }
    }
}