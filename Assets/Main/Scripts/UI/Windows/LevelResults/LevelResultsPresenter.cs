using Main.Scripts.Player.Data;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.UI.Windows.LevelResults
{
    public class LevelResultsPresenter : MonoBehaviour, UIScreen
    {
        private LevelResultsView levelResultsView = default!;

        private void Awake()
        {
            levelResultsView = GetComponent<LevelResultsView>();
        }

        public void Show()
        {
            var awardsData = PlayerDataManager.Instance.ThrowWhenNull().LocalAwardsData;
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