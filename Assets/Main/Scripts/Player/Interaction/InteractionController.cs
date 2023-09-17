using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions.Interaction;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Player.Interaction
{
    public class InteractionController : GameLoopEntityNetworked
    {
        [SerializeField]
        private float interactionRadius = 3f;
        [SerializeField]
        private LayerMask layerMask;

        private Collider[] colliders = new Collider[10];
        private Interactable? lastInteractableObject;

        private PlayerRef owner;

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.VisualStateUpdatePhase
        };

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            if ((Runner.LocalPlayer != owner) || !Runner.IsLastTick) return;

            var allowedInteractableObject = GetClosestInteractable();

            lastInteractableObject?.SetInteractionInfoVisibility(owner, false);
            lastInteractableObject = allowedInteractableObject;

            allowedInteractableObject?.SetInteractionInfoVisibility(owner, true);
        }

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
        }

        public void SetOwner(PlayerRef owner)
        {
            this.owner = owner;
        }

        private Interactable? GetClosestInteractable()
        {
            var hitsCount = Physics.OverlapSphereNonAlloc( 
                position: transform.position,
                radius: interactionRadius,
                results: colliders,
                layerMask: layerMask
            );

            Interactable? closestInteractableObject = null;
            var minDistance = float.MaxValue;
            for (var i = 0; i < hitsCount; i++)
            {
                var hit = colliders[i];
                var distance = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
                if (distance < minDistance && hit.gameObject.TryGetInterface<Interactable>(out var interactableObject))
                {
                    if (interactableObject.IsInteractionEnabled(owner))
                    {
                        minDistance = distance;
                        closestInteractableObject = interactableObject;
                    }
                }
            }


            return closestInteractableObject;
        }

        public bool TryInteract()
        {
            var allowedInteractableObject = GetClosestInteractable();
            if (allowedInteractableObject != null)
            {
                allowedInteractableObject.AddInteract(owner);
                return true;
            }

            return false;
        }
    }
}