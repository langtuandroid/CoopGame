using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Main.Scripts.Utils
{
    public class DebugHelper : MonoBehaviour
    {
        private static DebugHelper? instance;

        [SerializeField]
        private Material transparentMaterial = default!;
        private Dictionary<Object, float> drawingObjects = new();
        
        public static void DrawSphere(Vector3 position, float radius, Color color, float destroyDelay = 3f)
        {
            instance.ThrowWhenNull();
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale *= radius * 2f;
            sphere.GetComponent<MeshRenderer>().material = instance.transparentMaterial;
            sphere.GetComponent<Renderer>().material.color = color;
            sphere.GetComponent<SphereCollider>().enabled = false;
            instance.drawingObjects.Add(sphere, Time.time + destroyDelay);
        }

        void Awake()
        {
            if (instance)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        public void Update()
        {
            var copyDict = new Dictionary<Object, float>(drawingObjects);
            foreach (var debugGizmo in copyDict)
            {
                if (debugGizmo.Value < Time.time)
                {
                    Destroy(debugGizmo.Key);
                    drawingObjects.Remove(debugGizmo.Key);
                }
            }
        }

        private void OnDestroy()
        {
            instance = null;
        }
    }
}