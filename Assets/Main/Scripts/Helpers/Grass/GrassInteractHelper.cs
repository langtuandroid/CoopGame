using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.Helpers.Grass
{
    public class GrassInteractHelper : MonoBehaviour
    {
        public static GrassInteractHelper? Instance { get; private set; }
        
        [SerializeField]
        private UIDocument doc = default!;
        [SerializeField]
        private RenderTexture painterTextureF1 = default!;
        [SerializeField]
        private RenderTexture painterTextureF2 = default!;
        [SerializeField]
        private RenderTexture painterTextureF3 = default!;
        [SerializeField]
        private RenderTexture painterTextureF4 = default!;
        [SerializeField]
        private RenderTexture painterTextureF5 = default!;
        [SerializeField]
        private int painterTextureSize = 256;
        [SerializeField]
        private int targetTextureSize = 1024;
        [SerializeField]
        private float size = 100;

        private Dictionary<Transform, GrassPainterData> activePainters = new();
        private Stack<VisualElement> pool = new();

        private Texture2D texture2D_F1 = default!;
        private Texture2D texture2D_F2 = default!;
        private Texture2D texture2D_F3 = default!;
        private Texture2D texture2D_F4 = default!;
        private Texture2D texture2D_F5 = default!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
            
            // ReadPixels looks at the active RenderTexture.
            texture2D_F1 = CreateTexture2D(painterTextureF1);
            texture2D_F2 = CreateTexture2D(painterTextureF2);
            texture2D_F3 = CreateTexture2D(painterTextureF3);
            texture2D_F4 = CreateTexture2D(painterTextureF4);
            texture2D_F5 = CreateTexture2D(painterTextureF5);
        }

        private void LateUpdate()
        {
            foreach (var painterTransform in activePainters.Keys)
            {
                var painter = activePainters[painterTransform];
                var element = painter.element;
                
                var scale = painterTransform.lossyScale;
                var density = targetTextureSize / size;
                
                var width = scale.x * density;
                var height = scale.z * density;
                
                element.style.width = width;
                element.style.height = height;
                
                element.style.left = (painterTransform.position.x + size / 2f) * density - width / 2f;
                element.style.top = (-painterTransform.position.z + size / 2f) * density - height / 2f;
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void AddPainter(Transform painterTransform, GrassPainterForce force)
        {
            VisualElement painter;
            
            if (pool.Count > 0)
            {
                painter = pool.Pop();
            }
            else
            {
                painter = new VisualElement();
                painter.style.position = Position.Absolute;
            }
            painter.style.backgroundImage = new StyleBackground(GetTexture2DByType(force));
            
            painter.style.display = DisplayStyle.Flex;
            activePainters.Add(painterTransform, new GrassPainterData(force, painter));
            
            doc.rootVisualElement.Add(painter);
        }

        public void RemovePainter(Transform painterTransform)
        {
            if (activePainters.ContainsKey(painterTransform))
            {
                var painter = activePainters[painterTransform];
                painter.element.style.display = DisplayStyle.None;
                activePainters.Remove(painterTransform);
                pool.Push(painter.element);
            }
        }

        private Texture2D CreateTexture2D(RenderTexture renderTexture)
        {
            var texture2D = new Texture2D(painterTextureSize, painterTextureSize, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            return texture2D;
        }

        private Texture2D GetTexture2DByType(GrassPainterForce force)
        {
            return force switch
            {
                GrassPainterForce.FORCE_1 => texture2D_F1,
                GrassPainterForce.FORCE_2 => texture2D_F2,
                GrassPainterForce.FORCE_3 => texture2D_F3,
                GrassPainterForce.FORCE_4 => texture2D_F4,
                GrassPainterForce.FORCE_5 => texture2D_F5,
                _ => throw new ArgumentOutOfRangeException(nameof(force), force, null)
            };
        }
    }
}