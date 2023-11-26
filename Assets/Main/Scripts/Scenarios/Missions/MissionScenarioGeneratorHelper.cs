using System;
using Fusion;
using Main.Scripts.LevelGeneration;
using Main.Scripts.Player;
using Main.Scripts.Scenarios.Missions.Common;
using Main.Scripts.Scenarios.Missions.Escort;
using UniRx;

namespace Main.Scripts.Scenarios.Missions
{
public static class MissionScenarioGeneratorHelper
{
    public static IObservable<Scenario> GenerateMissionScenario(
        ScenarioGeneratorConfig scenarioGeneratorConfig,
        NetworkRunner runner,
        PlayersHolder playersHolder,
        MapData mapData)
    {
        return Observable.Start(() =>
        {
            MissionScenarioGenerator escortScenarioGenerator = scenarioGeneratorConfig switch
            {
                EscortScenarioGeneratorConfig escortScenarioGeneratorConfig => new EscortMissionScenarioGenerator(
                    config: escortScenarioGeneratorConfig,
                    runner: runner,
                    playersHolder: playersHolder,
                    mapData: mapData
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(scenarioGeneratorConfig), scenarioGeneratorConfig, null)
            };

            return escortScenarioGenerator.GenerateScenario();
        });
    }
}
}