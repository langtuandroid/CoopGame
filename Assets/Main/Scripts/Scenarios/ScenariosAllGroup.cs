using System.Collections.Generic;

namespace Main.Scripts.Scenarios
{
public class ScenariosAllGroup : Scenario
{
    private List<Scenario> scenarios;
    private ScenarioStatus status;

    public ScenariosAllGroup(List<Scenario> scenarios)
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

        var isAllCompleted = true;
        var isAnyFailed = false;
        foreach (var scenario in scenarios)
        {
            scenario.Update();
            var scenarioStatus = scenario.GetStatus();
            isAllCompleted &= scenarioStatus == ScenarioStatus.Success;
            isAllCompleted |= scenarioStatus == ScenarioStatus.Failed;
        }

        if (isAllCompleted)
        {
            status = ScenarioStatus.Success;
            Stop();
        }
        else if (isAnyFailed)
        {
            status = ScenarioStatus.Failed;
            Stop();
        }
    }

    public ScenarioStatus GetStatus()
    {
        return status;
    }
}
}