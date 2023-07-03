using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions.Interaction;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Player.Interaction
{
    public class InteractionController : GameLoopEntity
    {
        [SerializeField]
        private float interactionRadius = 3f;
        [SerializeField]
        private LayerMask layerMask;

        private List<LagCompensatedHit> hits = new();
        private Interactable? lastInteractableObject;

        private PlayerRef owner;

        public override void OnAfterPhysicsSteps()
        {
            if ((Runner.LocalPlayer != owner) || !Runner.IsLastTick) return;

            var allowedInteractableObject = GetClosestInteractable();

            lastInteractableObject?.SetInteractionInfoVisibility(owner, false);
            lastInteractableObject = allowedInteractableObject;

            allowedInteractableObject?.SetInteractionInfoVisibility(owner, true);
        }
        
        public void SetOwner(PlayerRef owner)
        {
            this.owner = owner;
        }

        private Interactable? GetClosestInteractable()
        {
            Runner.LagCompensation.OverlapSphere(
                origin: transform.position,
                radius: interactionRadius,
                player: owner,
                hits: hits,
                layerMask: layerMask
            );

            Interactable? closestInteractableObject = null;
            var minDistance = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit.Distance < minDistance && hit.GameObject.TryGetInterface<Interactable>(out var interactableObject))
                {
                    if (interactableObject.IsInteractionEnabled(owner))
                    {
                        minDistance = hit.Distance;
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
                allowedInteractableObject.Interact(owner);
                return true;
            }

            return false;
        }
    }
}