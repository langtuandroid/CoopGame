using System;
using System.Collections;
using System.Linq;
using Bolt.samples.AdvancedTutorial.scripts.Actions;
using Photon.Bolt;
using UnityEngine;
using Player = Bolt.AdvancedTutorial.Player;

namespace Bolt.Samples.AdvancedTutorial.scripts.Enemies
{
    public class RedCube : EntityBehaviour<IRedCube>,
        ObjectWithTakingDamage,
        ObjectWithGettingKnockBack,
        ObjectWithGettingStun
    {
        private Renderer renderer;
        private volatile bool isKnocking;
        private volatile bool isStunned;
        static readonly WaitForFixedUpdate WaitForFixed = new WaitForFixedUpdate();

        private void Awake()
        {
            renderer = GetComponent<Renderer>();
        }

        public override void Attached()
        {
            state.SetTransforms(state.transform, transform);
            state.health = 100;
        }

        public override void SimulateOwner()
        {
            if (canMoveByController() && Player.allPlayers.Any())
            {
                var playerTransform = Player.allPlayers.First().entity.gameObject.transform;
                entity.transform.position += (playerTransform.position - entity.transform.position)
                    .normalized * 0.04f;
                entity.transform.LookAt(playerTransform);
            }
        }

        private bool canMoveByController()
        {
            return !isKnocking && !isStunned;
        }

        private void Update()
        {
            var alpha = state.health * 0.01f;
            renderer.material.SetColor("_Color", new Color(1f,  alpha, alpha));
        }

        public void DealDamage(int damage)
        {
            state.health -= damage;

            if (state.health > 100 || state.health < 0)
            {
                state.health = 0;
            }

            if (state.health == 0)
            {
                Destroy(gameObject);
            }
        }

        public void ApplyKnockBack(Vector3 direction, float force)
        {
            var targetKnockBackPosition = entity.transform.position + direction.normalized * force;
            StartCoroutine(KnockBackCoroutine(entity.transform.position, targetKnockBackPosition));
        }

        private IEnumerator KnockBackCoroutine(Vector3 fromPosition, Vector3 targetPosition)
        {
            isKnocking = true;

            var knockingDuration = 0.15f;
            var knockingProgress = 0f;
            while (knockingProgress < knockingDuration)
            {
                knockingProgress += BoltNetwork.FrameDeltaTime;
                entity.transform.position = Vector3.Lerp(fromPosition, targetPosition,
                    knockingProgress / (float) knockingDuration);
                yield return WaitForFixed;
            }
            
            isKnocking = false;
        }

        public void ApplyStun(float durationSec)
        {
            StartCoroutine(StunCoroutine(durationSec));
        }

        private IEnumerator StunCoroutine(float durationSec)
        {
            isStunned = true;
            yield return new WaitForSeconds(durationSec);
            isStunned = false;
        }
    }
}