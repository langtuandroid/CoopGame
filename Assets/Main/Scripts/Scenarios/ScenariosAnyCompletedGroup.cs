using System.Collections.Generic;

namespace Main.Scripts.Scenarios
{
public class ScenariosAnyCompletedGroup : Scenario
{
    private List<Scenario> scenarios;
    private ScenarioStatus status;

    public ScenariosAnyCompletedGroup(List<Scenario> scenarios)
    {
        this.scenarios = scenarios;
    }

    public void Start()
    {
        status = ScenarioStatus.InProgress;
        foreach (var scenario in scenarios)
        {
            scenario.Start();
        }
    }

    public void Stop()
    {
        foreach (var scenario in scenarios)
        {
            scenario.Stop();
        }
    }

    public void Update()
    {
        if (status != ScenarioStatus.InProgress) return;

        var isAnyCompleted = false;
        var isAnyFailed = false;
        foreach (var scenario in scenarios)
        {
            scenario.Update();
            var scenarioStatus = scenario.GetStatus();
            isAnyCompleted |= scenarioStatus == ScenarioStatus.Success;
            isAnyFailed |= scenarioStatus == ScenarioStatus.Failed;
        }

        if (isAnyFailed)
        {
            status = ScenarioStatus.Failed;
            Stop();
        }
        else if (isAnyCompleted)
        {
            status = ScenarioStatus.Success;
            Stop();
        }
    }

    public ScenarioStatus GetStatus()
    {
        return status;
    }
}
}