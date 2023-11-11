using UnityEngine;

namespace Main.Scripts.LevelGeneration.Places.Road
{
public class RoadChunkController : MonoBehaviour
{
    private static readonly int FromPointRoad = Shader.PropertyToID("_FromPointRoad");
    private static readonly int HasRoad1 = Shader.PropertyToID("_HasRoad1");
    private static readonly int ToPointRoad = Shader.PropertyToID("_ToPointRoad1");

    [SerializeField]
    private MeshRenderer groundRenderer = null!;

    private MaterialPropertyBlock propertyBlock = null!;

    public void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    public void Init(
        RoadChunk roadChunk,
        int chunkSize,
        int offsetX,
        int offsetY
    )
    {
        propertyBlock.SetVector(
            FromPointRoad,
            new Vector4(
                roadChunk.FromPoint.x * chunkSize - offsetX,
                0,
                roadChunk.FromPoint.y * chunkSize - offsetY,
                0
            )
        );
        propertyBlock.SetFloat(HasRoad1, 1f);
        propertyBlock.SetVector(
            ToPointRoad,
            new Vector4(
                roadChunk.ToPoint.x * chunkSize - offsetX,
                0,
                roadChunk.ToPoint.y * chunkSize - offsetY,
                0
            )
        );

        groundRenderer.SetPropertyBlock(propertyBlock);
    }
}
}