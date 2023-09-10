using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Helpers.Grass
{
    public class GrassInteractionPainter
    {
        private ComputeShader shader;
        private RenderTexture renderTexture;
        private int targetTextureSize;
        private float mapSize;

        private Painter[] paintersData;
        private ComputeBuffer paintersBuffer;

        public GrassInteractionPainter(
            ComputeShader shader,
            RenderTexture renderTexture,
            int targetTextureSize,
            float mapSize,
            int initialCapacity
        )
        {
            this.shader = shader;
            this.renderTexture = renderTexture;
            this.targetTextureSize = targetTextureSize;
            this.mapSize = mapSize;

            paintersData = new Painter[initialCapacity];
            paintersBuffer = new ComputeBuffer(initialCapacity, sizeof(float) * 4);

            renderTexture.Release();
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
            shader.SetTexture(0, "resultTexture", renderTexture);
        }

        public void Draw(Dictionary<Transform, GrassPainterConfig> activePainters)
        {
            if (paintersData.Length < activePainters.Count)
            {
                paintersData = new Painter[paintersData.Length * 2];
                paintersBuffer.Release();
                paintersBuffer = new ComputeBuffer(paintersData.Length, sizeof(float) * 4);
            }

            var painterIndex = 0;
            var density = targetTextureSize / mapSize;
            var mapHalfSize = mapSize / 2f;
            foreach (var (painterTransform, config) in activePainters)
            {
                paintersData[painterIndex] = new Painter {
                    position = new Vector2(painterTransform.position.x + mapHalfSize, painterTransform.position.z + mapHalfSize) * density,
                    size = painterTransform.lossyScale.x * density,
                    force = config.Force
                };
                painterIndex++;
            }
            
            paintersBuffer.SetData(paintersData, 0, 0, activePainters.Count);
            
            shader.SetBuffer(0, "painters", paintersBuffer);
            shader.SetInt("paintersCount", activePainters.Count);
            shader.SetFloat("deltaTime", Time.deltaTime);
            shader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
        }

        public void Release()
        {
            renderTexture.Release();
            paintersBuffer.Release();
        }

        private struct Painter
        {
            public Vector2 position;
            public float size;
            public float force;
        }
    }
}