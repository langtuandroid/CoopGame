using Main.Scripts.Levels.Results;
using Main.Scripts.Player;

namespace Main.Scripts.UI.Windows.LevelResults
{
    public interface LevelResultsRepository
    {
        LevelResultsData GetLevelResultsData(string userId);
    }
}