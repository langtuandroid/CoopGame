using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace Main.Scripts.Component {
    [RequireComponent(typeof(NavMeshAgent))]
    [OrderBefore(typeof(NetworkTransform))]
    [DisallowMultipleComponent]
    public class NetworkNavMeshAgent : NetworkTransform
    {
        /// <summary>
        /// Sets the default teleport interpolation velocity to be the CC's current velocity.
        /// For more details on how this field is used, see <see cref="NetworkTransform.TeleportToPosition"/>.
        /// </summary>
        // protected override Vector3 DefaultTeleportInterpolationVelocity => Velocity;

        public NavMeshAgent NavMeshAgentComponent { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            CacheController();
        }

        public override void Spawned()
        {
            base.Spawned();
            CacheController();
        }

        private void CacheController()
        {
            if (NavMeshAgentComponent == null)
            {
                NavMeshAgentComponent = GetComponent<NavMeshAgent>();

                Assert.Check(NavMeshAgentComponent != null, $"An object with {nameof(NetworkNavMeshAgent)} must also have a {nameof(NavMeshAgent)} component.");
            }
        }

        protected override void CopyFromBufferToEngine()
        {
            // Trick: CC must be disabled before resetting the transform state
            NavMeshAgentComponent.enabled = false;

            // Pull base (NetworkTransform) state from networked data buffer
            base.CopyFromBufferToEngine();

            // Re-enable CC
            NavMeshAgentComponent.enabled = true;
        }

        public virtual void Move(Vector3 direction)
        {
            var velocity = direction.normalized * NavMeshAgentComponent.speed;

            NavMeshAgentComponent.Move(Runner.DeltaTime * velocity);
        }
    }
}