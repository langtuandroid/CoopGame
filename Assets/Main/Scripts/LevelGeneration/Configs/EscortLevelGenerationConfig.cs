using UnityEngine;

namespace Main.Scripts.LevelGeneration.Configs
{
[CreateAssetMenu(fileName = "EscortLevelGenerationConfig", menuName = "Level/Generation/EscortLevelGeneration")]
public class EscortLevelGenerationConfig : ScriptableObject
{
    [SerializeField]
    [Min(1)]
    private int roadLength = 1;
    [SerializeField]
    [Min(1)]
    private int dividersStep = 1;
    [SerializeField]
    [Min(1)]
    private int minRoadWidth = 1;
    [SerializeField]
    private int maxRoadWidth = 1;
    [SerializeField]
    [Min(0)]
    private int nextPointMaxOffset;
    [SerializeField]
    [Min(1)]
    private int outlineOffset = 1;

    public int RoadLength => roadLength;
    public int DividersStep => dividersStep;
    public int MinRoadWidth => minRoadWidth;
    public int MaxRoadWidth => maxRoadWidth;
    public int NextPointMaxOffset => nextPointMaxOffset;
    public int OutlineOffset => outlineOffset;
}
}