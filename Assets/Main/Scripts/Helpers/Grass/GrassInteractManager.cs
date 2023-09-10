using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Main.Scripts.Helpers.Grass
{
    public class GrassInteractManager : MonoBehaviour
    {
        public static GrassInteractManager? Instance { get; private set; }
        
        [SerializeField]
        private ComputeShader painterShader = default!;
        [SerializeField]
        private RenderTexture painterTexture = default!;
        [SerializeField]
        private int targetTextureSize = 1024;
        [SerializeField]
        private float mapSize = 100;

        private GrassInteractionPainter grassInteractionPainter = default!;

        private Dictionary<Transform, GrassPainterConfig> activePainters = new();

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;

            grassInteractionPainter = new GrassInteractionPainter(
                shader: painterShader,
                renderTexture: painterTexture,
                targetTextureSize: targetTextureSize,
                mapSize: mapSize,
                initialCapacity: 64
            );
        }

        private void LateUpdate()
        {
            grassInteractionPainter.Draw(activePainters);
        }

        private void OnDestroy()
        {
            Instance = null;
            grassInteractionPainter.Release();
        }

        public void AddPainter(Transform painterTransform, GrassPainterConfig config)
        {
            activePainters.Add(painterTransform, config);
        }

        public void RemovePainter(Transform painterTransform)
        {
            if (activePainters.ContainsKey(painterTransform))
            {
                activePainters.Remove(painterTransform);
            }
        }
    }
}