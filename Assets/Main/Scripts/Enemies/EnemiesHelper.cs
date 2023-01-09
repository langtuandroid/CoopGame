using Main.Scripts.Player;
using UnityEngine;

namespace Main.Scripts.Enemies
{
    public class EnemiesHelper : MonoBehaviour
    {
        [SerializeField]
        private PlayersHolder playersHolder;
        
        public Vector3? findPlayerTarget(Vector3 fromPosition)
        {
            var target = (Vector3?) null;
            foreach (var playerEntry in playersHolder.players)
            {
                var playerPosition = playerEntry.Value.transform.position;
                if (target == null || Vector3.Distance(fromPosition, playerPosition) < Vector3.Distance(fromPosition, target.Value))
                {
                    target = playerPosition;
                }
            }

            return target;
        }
    }
}