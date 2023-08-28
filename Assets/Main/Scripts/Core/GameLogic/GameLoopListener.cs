using System.Collections.Generic;
using Main.Scripts.Core.GameLogic.Phases;

namespace Main.Scripts.Core.GameLogic
{
    public interface GameLoopListener
    {
        void OnGameLoopPhase(GameLoopPhase phase);
        IEnumerable<GameLoopPhase> GetSubscribePhases();
    }
}