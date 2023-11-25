using System.Collections.Generic;
using Fusion;
using Main.Scripts.LevelGeneration.Data;
using Main.Scripts.Player;
using UnityEngine;

namespace Main.Scripts.Tasks
{
    public class PlaceTargetTask
    {
        private PlayersHolder playersHolder;
        private LayerMask targetLayerMask;

        private HashSet<PlayerRef> playersInPlace = new();
        private bool isTargetChecked { get; set; }

        private Listener? listener;

        private Collider[] colliders = new Collider[4];
        private PlaceTargetData placeTargetData;
        private Vector3 halfExtends;

        public bool IsTargetChecked => isTargetChecked;

        public PlaceTargetTask(
            PlayersHolder playersHolder,
            LayerMask targetLayerMask,
            PlaceTargetData placeTargetData
        )
        {
            this.playersHolder = playersHolder;
            this.targetLayerMask = targetLayerMask;
            this.placeTargetData = placeTargetData;

            var size = placeTargetData.ColliderInfo.Size;
            halfExtends = new Vector3(size.x, 1, size.y) / 2f;
        }

        public void SetListener(Listener? listener)
        {
            this.listener = listener;
        }

        public void OnPhysicsCheckCollisionsPhase()
        {
            var hitsCount = Physics.OverlapBoxNonAlloc(
                center: placeTargetData.Position,
                halfExtents: halfExtends,
                results: colliders,
                orientation: Quaternion.identity,
                mask: targetLayerMask
            );

            playersInPlace.Clear();
            for(var i = 0; i < hitsCount; i++)
            {
                var playerController = colliders[i].GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playersInPlace.Add(playerController.Object.StateAuthority);
                }
            }

            var hasAllAlivePlayersInPlace = false;
            foreach (var playerRef in playersHolder.GetKeys())
            {
                var playerController = playersHolder.Get(playerRef);
                if (playerController.GetPlayerState() != PlayerState.Dead)
                {
                    if (!playersInPlace.Contains(playerRef))
                    {
                        UpdateTaskStatus(false);
                        return;
                    }

                    if (playersInPlace.Contains(playerRef))
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
            listener?.OnTaskCheckChangedEvent(isTargetChecked);
        }

        public interface Listener
        {
            public void OnTaskCheckChangedEvent(bool isChecked);
        }
    }
}