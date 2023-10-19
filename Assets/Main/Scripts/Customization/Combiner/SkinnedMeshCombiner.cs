using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Customization.Combiner
{
    public static class SkinnedMeshCombiner
    {
        public static Mesh Combine(Matrix4x4 rootTransformMatrix, List<CombineMeshData> combineMeshes)
        {
            var meshesBindPoses = new Matrix4x4[combineMeshes.Count];
            var skinnedBindPoses = new Matrix4x4[combineMeshes.Count];
            var combine = new CombineInstance[combineMeshes.Count];
            for (var i = 0; i < combineMeshes.Count; i++)
            {
                combine[i] = new CombineInstance();
                combine[i].mesh = combineMeshes[i].mesh;

                skinnedBindPoses[i] = combineMeshes[i].transform.worldToLocalMatrix * rootTransformMatrix;
                meshesBindPoses[i] = skinnedBindPoses[i].inverse * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(combineMeshes[i].rotation), Vector3.one);

                
                combine[i].transform = meshesBindPoses[i];
            }
            var skinnedMesh = new Mesh();
            skinnedMesh.CombineMeshes(combine);

            var skinnedBoneWeights = new BoneWeight[skinnedMesh.vertexCount];
            
            var offset = 0;
            for (var i = 0; i < combine.Length; i++)
            {
                for (var k = 0; k < combine[i].mesh.vertexCount; k++)
                {
                    skinnedBoneWeights[offset + k].boneIndex0 = i;
                    skinnedBoneWeights[offset + k].weight0 = 1;
                }
            
                offset += combine[i].mesh.vertexCount;
            }
            
            skinnedMesh.bindposes = skinnedBindPoses;
            skinnedMesh.boneWeights = skinnedBoneWeights;

            return skinnedMesh;
        }
    }
}