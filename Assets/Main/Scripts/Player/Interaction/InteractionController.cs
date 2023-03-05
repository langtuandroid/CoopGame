using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions.Interaction;
using UnityEngine;

namespace Main.Scripts.Player.Interaction
{
    public class InteractionController : NetworkBehaviour
    {
        [SerializeField]
        private float interactionRadius = 3f;
        [SerializeField]
        private LayerMask layerMask;

        private List<LagCompensatedHit> hits = new();
        private Interactable? lastInteractableObject;

        public override void FixedUpdateNetwork()
        {
            if (!HasInputAuthority || !Runner.IsLastTick) return;

            var allowedInteractableObject = GetClosestInteractable();

            lastInteractableObject?.SetInteractionInfoVisibility(Object.InputAuthority, false);
            lastInteractableObject = allowedInteractableObject;

            allowedInteractableObject?.SetInteractionInfoVisibility(Object.InputAuthority, true);
        }

        private Interactable? GetClosestInteractable()
        {
            Runner.LagCompensation.OverlapSphere(
                origin: transform.position,
                radius: interactionRadius,
                player: Object.InputAuthority,
                hits: hits,
                layerMask: layerMask
            );

            Interactable? closestInteractableObject = null;
            var minDistance = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit.Distance < minDistance && hit.GameObject.TryGetComponent<Interactable>(out var interactableObject))
                {
                    if (interactableObject.IsInteractionEnabled(Object.InputAuthority))
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
                allowedInteractableObject.Interact(Object.InputAuthority);
                return true;
            }

            return false;
        }
    }
}