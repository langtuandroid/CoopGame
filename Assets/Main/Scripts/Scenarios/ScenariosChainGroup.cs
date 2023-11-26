using System.Collections.Generic;

namespace Main.Scripts.Scenarios
{
public class ScenariosChainGroup : Scenario
{
    private List<Scenario> scenariosChain;
    private int currentScenarioIndex;
    private ScenarioStatus status;

    public ScenariosChainGroup(
        List<Scenario> scenariosChain
    )
    {
        this.scenariosChain = scenariosChain;
    }

    public void Start()
    {
        status = ScenarioStatus.InProgress;
        StartNextScenario();
    }

    public void Stop()
    {
        StopCurrentScenario();
    }

    public void Update()
    {
        if (status != ScenarioStatus.InProgress) return;

        var currentScenario = scenariosChain[currentScenarioIndex];
        currentScenario.Update();
        var scenarioStatus = currentScenario.GetStatus();
        if (scenarioStatus == ScenarioStatus.Success)
        {
            StopCurrentScenario();
            currentScenarioIndex++;

            if (currentScenarioIndex == scenariosChain.Count)
            {
                status = ScenarioStatus.Success;
            }
            else
            {
                StartNextScenario();
            }
        }
        else if (scenarioStatus == ScenarioStatus.Failed)
        {
            StopCurrentScenario();
            status = ScenarioStatus.Failed;
        }
    }

    private void StartNextScenario()
    {
        if (status != ScenarioStatus.InProgress) return;

        scenariosChain[currentScenarioIndex].Start();
    }

    private void StopCurrentScenario()
    {
        if (status != ScenarioStatus.InProgress) return;

        scenariosChain[currentScenarioIndex].Stop();
    }

    public ScenarioStatus GetStatus()
    {
        return status;
    }
}
}