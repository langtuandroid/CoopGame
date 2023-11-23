using System.Linq;
using Main.Scripts.LevelGeneration.Chunk;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.NavMesh
{
public class NavMeshChunkBuilder
{
    public void GenerateNavMesh(
        IChunk?[][] map,
        int chunkSize,
        out Vector3[] vertices,
        out int[] triangles,
        out Bounds bounds
    )
    {
        var polygon = new Polygon();

        for (var i = 0; i < map.Length; i++)
        {
            for (var j = 0; j < map[i].Length; j++)
            {
                map[i][j]?.AddChunkNavMesh(
                    new Vector2(i * chunkSize - chunkSize / 2f, j * chunkSize - chunkSize / 2f),
                    chunkSize,
                    polygon
                );
            }
        }

        var triangleNetMesh = (TriangleNetMesh)polygon.Triangulate();

        TriangleToMesh(
            triangleNetMesh,
            null,
            out vertices,
            out triangles,
            out bounds
        );
    }

    private void TriangleToMesh(
        TriangleNetMesh triangleNetMesh,
        QualityOptions? options,
        out Vector3[] vertices,
        out int[] triangles,
        out Bounds bounds
    )
    {
        if (options != null)
        {
            triangleNetMesh.Refine(options);
        }

        var triangleNetVerts = triangleNetMesh.Vertices.ToList();

        var trianglesList = triangleNetMesh.Triangles;

        vertices = new Vector3[triangleNetVerts.Count];
        triangles = new int[trianglesList.Count * 3];

        for (int i = 0; i < vertices.Length; i++)
        {
            var point = (Vector3)triangleNetVerts[i];
            vertices[i] = new Vector3(point.x, 0, point.y);
        }

        int k = 0;

        foreach (var triangle in trianglesList)
        {
            for (int i = 2; i >= 0; i--)
            {
                triangles[k] = triangleNetVerts.IndexOf(triangle.GetVertex(i));
                k++;
            }
        }

        var rectangle = triangleNetMesh.Bounds;

        bounds = new Bounds(
            new Vector3((rectangle.Left + rectangle.Right) / 2f, 0, (rectangle.Bottom + rectangle.Top) / 2f),
            new Vector3(rectangle.Right - rectangle.Left, 0, rectangle.Top - rectangle.Bottom)
        );
    }
}
}