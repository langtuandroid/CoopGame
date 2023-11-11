using UnityEngine;

namespace Main.Scripts.LevelGeneration.Places.Crossroads
{
public class CrossroadsChunkController : MonoBehaviour
{
    private static readonly int FromPointRoad = Shader.PropertyToID("_FromPointRoad");
    private static readonly int HasRoad1 = Shader.PropertyToID("_HasRoad1");
    private static readonly int ToPointRoad1 = Shader.PropertyToID("_ToPointRoad1");
    private static readonly int HasRoad2 = Shader.PropertyToID("_HasRoad2");
    private static readonly int ToPointRoad2 = Shader.PropertyToID("_ToPointRoad2");
    private static readonly int HasRoad3 = Shader.PropertyToID("_HasRoad3");
    private static readonly int ToPointRoad3 = Shader.PropertyToID("_ToPointRoad3");
    private static readonly int HasRoad4 = Shader.PropertyToID("_HasRoad4");
    private static readonly int ToPointRoad4 = Shader.PropertyToID("_ToPointRoad4");

    [SerializeField]
    private MeshRenderer groundRenderer = null!;

    private MaterialPropertyBlock propertyBlock = null!;

    public void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    public void Init(
        CrossroadsChunk crossroadsChunk,
        int chunkSize,
        int offsetX,
        int offsetY
    )
    {
        var crossroadsPlace = crossroadsChunk.CrossroadsPlace;

        var roadsList = crossroadsPlace.GetRoadsToPlacesList();
        var crossroadsPosition = crossroadsPlace.Position;

        propertyBlock.SetVector(
            FromPointRoad,
            new Vector4(
                crossroadsPosition.x * chunkSize - offsetX,
                0,
                crossroadsPosition.y * chunkSize - offsetY,
                0
            )
        );

        propertyBlock.SetFloat(HasRoad1, 1f);
        propertyBlock.SetVector(
            ToPointRoad1,
            new Vector4(
                roadsList[0].Position.x * chunkSize - offsetX,
                0,
                roadsList[0].Position.y * chunkSize - offsetY,
                0
            )
        );

        var hasRoad2 = roadsList.Count > 1;
        propertyBlock.SetFloat(HasRoad2, hasRoad2 ? 1f : 0f);
        if (hasRoad2)
        {
            propertyBlock.SetVector(
                ToPointRoad2,
                new Vector4(
                    roadsList[1].Position.x * chunkSize - offsetX,
                    0,
                    roadsList[1].Position.y * chunkSize - offsetY,
                    0
                )
            );
        }

        var hasRoad3 = roadsList.Count > 2;
        propertyBlock.SetFloat(HasRoad3, hasRoad3 ? 1f : 0f);
        if (hasRoad3)
        {
            propertyBlock.SetVector(
                ToPointRoad3,
                new Vector4(
                    roadsList[2].Position.x * chunkSize - offsetX,
                    0,
                    roadsList[2].Position.y * chunkSize - offsetY,
                    0
                )
            );
        }

        var hasRoad4 = roadsList.Count > 3;
        propertyBlock.SetFloat(HasRoad4, hasRoad4 ? 1f : 0f);
        if (hasRoad4)
        {
            propertyBlock.SetVector(
                ToPointRoad4,
                new Vector4(
                    roadsList[3].Position.x * chunkSize - offsetX,
                    0,
                    roadsList[3].Position.y * chunkSize - offsetY,
                    0
                )
            );
        }


        groundRenderer.SetPropertyBlock(propertyBlock);
    }
}
}