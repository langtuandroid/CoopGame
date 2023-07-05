using System;
using UnityEngine;

namespace Main.Scripts.Customization.Combiner
{
    [Serializable]
    public struct CombineMeshData
    {
        public Transform transform;
        public Mesh mesh;
        public Vector3 rotation;
    }
}