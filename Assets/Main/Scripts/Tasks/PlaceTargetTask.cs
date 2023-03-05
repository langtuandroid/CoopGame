using System.Collections.Generic;
using Fusion;
using Main.Scripts.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Tasks
{
    public class PlaceTargetTask : NetworkBehaviour
    {
        [SerializeField]
        private PlayersHolder playersHolder = default!;
        [SerializeField]
        private LayerMask layerMask;

        [Networked, Capacity(16)]
        private NetworkDictionary<PlayerRef, bool> playersInPlace => default;
        [Networked]
        private bool isTargetChecked { get; set; }

        public UnityEvent<bool> OnTaskCheckChangedEvent = default!;

        private List<LagCompensatedHit> hits = new();
        private Vector3 placeExtents;

        public override void Spawned()
        {
            placeExtents = transform.localScale / 2;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            OnTaskCheckChangedEvent.RemoveAllListeners();
        }

        public override void FixedUpdateNetwork()
        {
            Runner.LagCompensation.OverlapBox(
                center: transform.position,
                extents: placeExtents,
                orientation: transform.rotation,
                player: Object.StateAuthority,
                hits: hits,
                layerMask: layerMask
            );

            playersInPlace.Clear();
            foreach (var hit in hits)
            {
                var playerController = hit.GameObject.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playersInPlace.Add(playerController.Object.InputAuthority, true);
                }
            }

            var hasAllAlivePlayersInPlace = false;
            foreach (var playerRef in playersHolder.GetKeys())
            {
                var playerController = playersHolder.Get(playerRef);
                if (playerController.state != PlayerController.State.Dead)
                {
                    if (!playersInPlace.ContainsKey(playerRef))
                    {
                        return;
                    }

                    if (playersInPlace.ContainsKey(playerRef))
                    {
                        hasAllAlivePlayersInPlace = true;
                    }
                }
            }

            UpdateTaskStatus(hasAllAlivePlayersInPlace);
        }

        private void UpdateTaskStatus(bool isCompleted)
        {
            if (isTargetChecked == isCompleted) return;
            isTargetChecked = isCompleted;
            OnTaskCheckChangedEvent.Invoke(isTargetChecked);
        }
    }
}