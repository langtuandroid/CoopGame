using System.Collections.Generic;

namespace Main.Scripts.Scenarios
{
public class ScenariosAnyGroup : Scenario
{
    private List<Scenario> scenarios;
    private ScenarioStatus status;

    public ScenariosAnyGroup(List<Scenario> scenarios)
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
        var isAllFailed = true;
        foreach (var scenario in scenarios)
        {
            scenario.Update();
            var scenarioStatus = scenario.GetStatus();
            isAnyCompleted |= scenarioStatus == ScenarioStatus.Success;
            isAllFailed &= scenarioStatus == ScenarioStatus.Failed;
        }

        if (isAnyCompleted)
        {
            status = ScenarioStatus.Success;
            Stop();
        }
        else if (isAllFailed)
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