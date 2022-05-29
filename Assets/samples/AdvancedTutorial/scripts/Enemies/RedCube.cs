using System;
using System.Linq;
using Photon.Bolt;
using UnityEngine;
using Player = Bolt.AdvancedTutorial.Player;

namespace Bolt.Samples.AdvancedTutorial.scripts.Enemies
{
    public class RedCube : EntityBehaviour<IRedCube>
    {
        private Renderer renderer;

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
            if (Player.allPlayers.Any())
            {
                var playerTransform = Player.allPlayers.First().entity.gameObject.transform;
                entity.transform.position += (playerTransform.position - entity.transform.position)
                    .normalized * 0.04f;
                entity.transform.LookAt(playerTransform);
            }
        }

        private void Update()
        {
            var alpha = state.health * 0.01f;
            renderer.material.SetColor("_Color", new Color(1f,  alpha, alpha));
        }

        public void ApplyDamage(byte damage)
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
    }
}