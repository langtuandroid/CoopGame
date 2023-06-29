using Fusion;
using UnityEngine;

namespace Main.Scripts.Core.CustomPhysics
{
    public class NetworkRigidbody3D : NetworkTransform
    {
        private new Rigidbody rigidbody = default!;

        protected override Vector3 DefaultTeleportInterpolationVelocity => rigidbody.velocity;

        protected override void Awake()
        {
            base.Awake();
            rigidbody = GetComponent<Rigidbody>();
        }
    }
}