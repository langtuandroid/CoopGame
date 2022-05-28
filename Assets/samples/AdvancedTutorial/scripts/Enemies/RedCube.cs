using System.Linq;
using Photon.Bolt;
using UnityEngine;
using Player = Bolt.AdvancedTutorial.Player;

namespace Bolt.Samples.AdvancedTutorial.scripts.Enemies
{
    public class RedCube : EntityBehaviour<IRedCube>
    {
        public override void Attached()
        {
            state.SetTransforms(state.transform, transform);
        }

        public override void SimulateOwner()
        {
            if (Player.allPlayers.Any())
            {
                entity.transform.position += (Player.allPlayers.First().entity.gameObject.transform.position - entity.transform.position)
                    .normalized * 0.01f;
            }
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