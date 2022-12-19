using System;
using Fusion;
using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.GameResources
{
    public class GoldMineController : NetworkBehaviour,
        ObjectWithTakingDamage
    {
        [SerializeField]
        private GameObject goldBar;
        [SerializeField] 
        private int maxHealth = 100;
        
        [Networked]
        private int health { get; set; }
        [Networked]
        private float rotation { set; get; }
        [Networked]
        private ref NetworkRNG random => ref MakeRef<NetworkRNG>(new NetworkRNG(0)); //todo put random room seed
        
        public override void Spawned()
        {
            health = maxHealth;
            rotation = 0f;
        }

        public void ApplyDamage(int damage)
        {
            health -= damage;
            rotation += 30;
            transform.localRotation = Quaternion.Euler(0, -rotation, 0); //TODO change such an extraterrestrial and exquisite representation of damage receiving to something more vulgar and mundane
            
            if (health > maxHealth || health < 0)
            {
                health = 0;
            }
            
            if (health == 0 && HasStateAuthority)
            {
                var goldBars = random.RangeInclusive(3,6);
                Debug.Log($"{random.Peek}");
                for (var i = 0; i < goldBars; i++)
                {
                    Runner.Spawn(goldBar, transform.position + new Vector3(random.RangeInclusive(-5, 5), 0, random.RangeInclusive(-5, 5)));
                }
                Runner.Despawn(Object);
            }
        }
    }
}
