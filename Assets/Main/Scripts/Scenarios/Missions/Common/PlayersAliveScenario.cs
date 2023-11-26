using Main.Scripts.Player;

namespace Main.Scripts.Scenarios.Missions.Common
{
public class PlayersAliveScenario : Scenario
{
    private PlayersHolder playersHolder;
    private ScenarioStatus status;

    public PlayersAliveScenario(
        PlayersHolder playersHolder
    )
    {
        this.playersHolder = playersHolder;
    }

    public void Start()
    {
        status = ScenarioStatus.InProgress;
    }

    public void Stop() { }

    public void Update()
    {
        if (status != ScenarioStatus.InProgress) return;

        var isAllPlayersDead = true;

        foreach (var playerRef in playersHolder.GetKeys())
        {
            var playerController = playersHolder.Get(playerRef);
            isAllPlayersDead &= playerController.GetPlayerState() == PlayerState.Dead;
        }

        if (isAllPlayersDead)
        {
            status = ScenarioStatus.Failed;
        }
    }

    public ScenarioStatus GetStatus()
    {
        return status;
    }
}
}