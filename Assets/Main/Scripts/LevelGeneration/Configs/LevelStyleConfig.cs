using Main.Scripts.LevelGeneration.Places.Crossroads;
using Main.Scripts.LevelGeneration.Places.Outside;
using Main.Scripts.LevelGeneration.Places.Road;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Configs
{
[CreateAssetMenu(fileName = "LevelStyleConfig", menuName = "Level/Generation/LevelStyle")]
public class LevelStyleConfig : ScriptableObject
{
    [SerializeField]
    private RoadChunkController roadChunkPrefab = null!;
    [SerializeField]
    private CrossroadsChunkController crossroadsChunkPrefab = null!;
    [SerializeField]
    private HillOutsideChunkController hillOutsideChunkPrefab = null!;
    [SerializeField]
    private WaterOutsideChunkController waterOutsideChunkPrefab = null!;
    [SerializeField]
    private DecorationsPack decorationsPack = null!;

    public RoadChunkController RoadChunkPrefab => roadChunkPrefab;
    public CrossroadsChunkController CrossroadsChunkPrefab => crossroadsChunkPrefab;
    public HillOutsideChunkController HillOutsideChunkPrefab => hillOutsideChunkPrefab;
    public WaterOutsideChunkController WaterOutsideChunkPrefab => waterOutsideChunkPrefab;
    public DecorationsPack DecorationsPack => decorationsPack;
}
}