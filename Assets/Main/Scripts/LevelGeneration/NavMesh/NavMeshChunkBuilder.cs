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

    public Mesh GenerateNavMesh(IChunk[][] map)
    {
        var chunkSize = 10;
        
        var centerOffset = new Vector2(
            -map.Length * chunkSize / 2 - chunkSize / 2,
            -map[0].Length * chunkSize / 2 - chunkSize / 2
        );
        
        var polygon = new Polygon();

        for (var i = 0; i < map.Length; i++)
        {
            for (var j = 0; j < map[i].Length; j++)
            {
                map[i][j].AddChunkNavMesh(
                    new Vector2(i * chunkSize, j * chunkSize) + centerOffset,
                    chunkSize,
                    polygon
                );
            }
        }

        var triangleNetMesh = (TriangleNetMesh)polygon.Triangulate();

        var mesh = GenerateNavMesh(triangleNetMesh);

        return mesh;
    }

    private Mesh GenerateNavMesh(TriangleNetMesh triangleNetMesh, QualityOptions? options = null)
    {
        if (options != null)
        {
            triangleNetMesh.Refine(options);
        }
         
        Mesh mesh = new Mesh();
        var triangleNetVerts = triangleNetMesh.Vertices.ToList();
  
        var triangles = triangleNetMesh.Triangles;
       
        Vector3[] verts = new Vector3[triangleNetVerts.Count];
        int[] trisIndex = new int[triangles.Count * 3];

        for (int i = 0; i < verts.Length; i++)
        {
            var point = (Vector3) triangleNetVerts[i];
            verts[i] = new Vector3(point.x, 0, point.y);
        }
            
        int k = 0;
         
        foreach (var triangle in triangles)
        {
            for (int i = 2; i >= 0; i--)
            {
                trisIndex[k] = triangleNetVerts.IndexOf(triangle.GetVertex(i));
                k++;
            }
        }

        mesh.vertices = verts;
        mesh.triangles = trisIndex;

        mesh.RecalculateBounds();
        mesh.Optimize();
        return mesh;
    }
}
}