using Fusion;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Helpers.Grass
{
    public class GrassPainter : NetworkBehaviour
    {
        [SerializeField]
        private GrassPainterConfig painterConfig = default!;

        private GrassInteractManager grassInteractManager = default!;
        
        public override void Spawned()
        {
            grassInteractManager = GrassInteractManager.Instance.ThrowWhenNull();
            grassInteractManager.AddPainter(transform, painterConfig);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (grassInteractManager != null)
            {
                grassInteractManager.RemovePainter(transform);
            }
        }
    }
}