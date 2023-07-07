using System.Collections.Generic;
using Fusion;
using UnityEditor;
using UnityEngine;

namespace Main.Scripts.Customization.Combiner
{
    public class SkinnedMeshCombinerTool : MonoBehaviour
    {
        [SerializeField]
        private Mesh generatedMesh = default!;
        [SerializeField]
        private SkinnedMeshRenderer skinnedMeshRenderer = default!;
        [SerializeField]
        private Transform meshRootTransform = default!;
        [SerializeField]
        private List<CombineMeshData> combineMeshes = new();

        private void OnValidate()
        {
            ApplyGeneratedMesh();
        }

        private void Awake()
        {
            ApplyGeneratedMesh();
        }

        private void ApplyGeneratedMesh()
        {
            if (generatedMesh != null)
            {
                skinnedMeshRenderer.bones = combineMeshes.Map(data => data.transform);
                skinnedMeshRenderer.sharedMesh = generatedMesh;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Combine mesh")]
        private void CombineMesh()
        {
            var skinnedMesh = Instantiate(SkinnedMeshCombiner.Combine(meshRootTransform, combineMeshes));

            var path = $"Assets/Main/Meshes/Baked/{name}_mesh.asset";
            
            AssetDatabase.CreateAsset(skinnedMesh, path);
            AssetDatabase.SaveAssets();
            
            Debug.LogWarning($"!!!Success: Don't forget apply generated mesh at {path}!!!");
        }
#endif

    }
}