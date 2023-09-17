using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Tasks
{
    public class PlaceTargetTask : GameLoopEntityNetworked
    {
        [SerializeField]
        private PlayersHolder playersHolder = default!;
        [SerializeField]
        private LayerMask layerMask;

        [Networked, Capacity(4)]
        private NetworkDictionary<PlayerRef, bool> playersInPlace => default;
        [Networked]
        private bool isTargetChecked { get; set; }

        public UnityEvent<bool> OnTaskCheckChangedEvent = default!;

        private Collider[] colliders = new Collider[4];
        private Vector3 placeExtents;

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.PhysicsCheckCollisionsPhase
        };

        public bool IsTargetChecked => isTargetChecked;

        public override void Spawned()
        {
            base.Spawned();
            placeExtents = transform.localScale / 2;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            OnTaskCheckChangedEvent.RemoveAllListeners();
        }

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            if (!Runner.IsSharedModeMasterClient) return;
            
            var hitsCount = Physics.OverlapBoxNonAlloc(
                center: transform.position,
                halfExtents: placeExtents,
                results: colliders,
                orientation: transform.rotation,
                mask: layerMask
            );

            playersInPlace.Clear();
            for(var i = 0; i < hitsCount; i++)
            {
                var playerController = colliders[i].GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playersInPlace.Add(playerController.Object.StateAuthority, true);
                }
            }

            var hasAllAlivePlayersInPlace = false;
            foreach (var playerRef in playersHolder.GetKeys())
            {
                var playerController = playersHolder.Get(playerRef);
                if (playerController.GetPlayerState() != PlayerState.Dead)
                {
                    if (!playersInPlace.ContainsKey(playerRef))
                    {
                        UpdateTaskStatus(false);
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

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
        }

        private void UpdateTaskStatus(bool isCompleted)
        {
            if (isTargetChecked == isCompleted) return;
            
            isTargetChecked = isCompleted;
            OnTaskCheckChangedEvent.Invoke(isTargetChecked);
        }
    }
}