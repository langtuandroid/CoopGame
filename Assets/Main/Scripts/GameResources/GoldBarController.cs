using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Drop;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.GameResources
{
    public class GoldBarController : NetworkBehaviour
    {
        private void OnTriggerEnter(Collider collider)
        {
            if (!HasStateAuthority) return;
            
            var pickingUpObject = collider.GetInterface<ObjectWithPickUp>();
            if (pickingUpObject is null) return;
            
            pickingUpObject.OnPickUp(DropType.Gold);
            Runner.Despawn(Object);
        }
    }
}
