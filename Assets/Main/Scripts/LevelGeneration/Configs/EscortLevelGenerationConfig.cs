using UnityEngine;

namespace Main.Scripts.LevelGeneration.Configs
{
[CreateAssetMenu(fileName = "EscortLevelGenerationConfig", menuName = "Level/Generation/EscortLevelGeneration")]
public class EscortLevelGenerationConfig : LevelGenerationConfig
{
    [SerializeField]
    [Min(1)]
    private int roadLength = 1;
    [SerializeField]
    [Min(1)]
    private int dividersStep = 1;
    [SerializeField]
    [Min(0)]
    private int nextPointMaxOffset;

    public int RoadLength => roadLength;
    public int DividersStep => dividersStep;
    public int NextPointMaxOffset => nextPointMaxOffset;
}
}