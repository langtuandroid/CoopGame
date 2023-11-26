using UnityEngine;

namespace Main.Scripts.Scenarios.Missions.Common
{
public abstract class ScenarioGeneratorConfig : ScriptableObject
{
    [SerializeField]
    private PlayersSpawnTaskController playersSpawnTaskPrefab = null!;

    public PlayersSpawnTaskController PlayersSpawnTaskPrefab => playersSpawnTaskPrefab;
}
}