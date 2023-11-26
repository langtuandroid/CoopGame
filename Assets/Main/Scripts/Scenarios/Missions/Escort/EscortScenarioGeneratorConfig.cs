using Main.Scripts.Scenarios.Missions.Common;
using UnityEngine;

namespace Main.Scripts.Scenarios.Missions.Escort
{
[CreateAssetMenu(fileName = "EscortScenarioGeneratorConfig", menuName = "Level/EscortScenarioGenerator")]
public class EscortScenarioGeneratorConfig : ScenarioGeneratorConfig
{
    [SerializeField]
    private EscortTaskController escortTaskPrefab = null!;

    public EscortTaskController EscortTaskPrefab => escortTaskPrefab;
}
}