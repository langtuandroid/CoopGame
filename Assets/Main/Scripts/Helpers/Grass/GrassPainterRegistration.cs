using Fusion;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Main.Scripts.Helpers.Grass
{
    public class GrassPainterRegistration : NetworkBehaviour
    {
        [FormerlySerializedAs("painterType")]
        [SerializeField]
        private GrassPainterForce painterForce;
        
        public override void Spawned()
        {
            GrassInteractHelper.Instance.ThrowWhenNull().AddPainter(transform, painterForce);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            var grassInteractHelper = GrassInteractHelper.Instance;
            if (grassInteractHelper != null)
            {
                grassInteractHelper.RemovePainter(transform);
            }
        }
    }
}