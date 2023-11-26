using UnityEngine;

namespace Main.Scripts.LevelGeneration.Configs
{
[CreateAssetMenu(fileName = "EscortMapGenerationConfig", menuName = "Level/Generation/EscortMapGeneration")]
public class EscortMapGenerationConfig : MapGenerationConfig
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