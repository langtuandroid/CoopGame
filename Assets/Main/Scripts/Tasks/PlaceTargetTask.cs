using Fusion;
using Main.Scripts.Player;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Tasks
{
    public class PlaceTargetTask : NetworkBehaviour
    {
        [SerializeField]
        private PlayersHolder playersHolder;

        public UnityEvent OnTaskCompleted;

        [Networked, Capacity(16)]
        private NetworkDictionary<PlayerRef, bool> playersInPlace => default;

        [Networked]
        private bool isCompleted { get; set; }

        public void OnTriggerEnter(Collider other)
        {
            if (isCompleted)
            {
                return;
            }

            if (other.gameObject.TryGetComponent<PlayerController>(out var enteredPlayer))
            {
                playersInPlace.Add(enteredPlayer.Object.InputAuthority, true);
                var hasAnyAlivePlayerInPlace = false;
                foreach (var (playerRef, playerController) in playersHolder.Players)
                {
                    if (playerController.state != PlayerController.State.Dead)
                    {
                        if (!playersInPlace.ContainsKey(playerRef))
                        {
                            return;
                        }

                        if (playersInPlace.ContainsKey(playerRef))
                        {
                            hasAnyAlivePlayerInPlace = true;
                        }
                    }
                }

                if (hasAnyAlivePlayerInPlace)
                {
                    CompleteTask();
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent<PlayerController>(out var enteredPlayer))
            {
                playersInPlace.Remove(enteredPlayer.Object.InputAuthority);
            }
        }

        private void CompleteTask()
        {
            isCompleted = true;
            OnTaskCompleted.Invoke();
        }
    }
}