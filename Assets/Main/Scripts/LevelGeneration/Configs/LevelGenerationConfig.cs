using UnityEngine;

namespace Main.Scripts.LevelGeneration.Configs
{
public abstract class LevelGenerationConfig : ScriptableObject
{
    [SerializeField]
    [Min(1)]
    private int chunkSize = 10;
    [SerializeField]
    [Min(1)]
    private int minRoadWidth = 1;
    [SerializeField]
    private int maxRoadWidth = 1;
    [SerializeField]
    [Min(1)]
    private int outlineOffset = 1;
    [SerializeField]
    private BoxCollider boxCollider = null!;
    [SerializeField]
    private SphereCollider sphereCollider = null!;

    public int ChunkSize => chunkSize;
    public int MinRoadWidth => minRoadWidth;
    public int MaxRoadWidth => maxRoadWidth;
    public int OutlineOffset => outlineOffset;
    public BoxCollider BoxCollider => boxCollider;
    public SphereCollider SphereCollider => sphereCollider;
}
}