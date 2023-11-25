using System.Collections.Generic;
using Main.Scripts.LevelGeneration.Configs;
using Main.Scripts.LevelGeneration.Data.Colliders;
using TriangleNet.Geometry;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Chunk
{
public interface IChunk
{
    public void AddChunkNavMesh(
        Vector2 position,
        float chunkSize,
        Polygon polygon
    );

    public void AddDecoration(
        DecorationConfig decorationConfig,
        Vector2 Position
    );

    public DecorationConfig? GetDecorationConfig();
    public Vector2 GetDecorationPosition();
    public void SetOccupiedByDecoration(bool occupied);

    public bool CanAddDecoration();

    public void GetColliders(
        Vector2 chunkPosition,
        LevelGenerationConfig levelGenerationConfig,
        List<ColliderData> colliders
    );
}
}