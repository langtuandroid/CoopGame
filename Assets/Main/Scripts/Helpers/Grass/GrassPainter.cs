using Fusion;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Helpers.Grass
{
    public class GrassPainter : NetworkBehaviour
    {
        [SerializeField]
        private GrassPainterConfig painterConfig = default!;
        
        public override void Spawned()
        {
            GrassInteractManager.Instance.ThrowWhenNull().AddPainter(transform, painterConfig);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            var grassInteractHelper = GrassInteractManager.Instance;
            if (grassInteractHelper != null)
            {
                grassInteractHelper.RemovePainter(transform);
            }
        }
    }
}