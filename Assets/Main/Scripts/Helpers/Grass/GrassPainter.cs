using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Helpers.Grass
{
    public class GrassPainter : MonoBehaviour
    {
        [SerializeField]
        private GrassPainterConfig painterConfig = default!;

        private GrassInteractManager grassInteractManager = default!;
        
        private void Start()
        {
            grassInteractManager = GrassInteractManager.Instance.ThrowWhenNull();
            grassInteractManager.AddPainter(transform, painterConfig);
        }

        private void OnDestroy()
        {
            if (grassInteractManager != null)
            {
                grassInteractManager.RemovePainter(transform);
            }
        }
    }
}