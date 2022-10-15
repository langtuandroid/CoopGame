using System;
using UnityEngine;
using UnityEngine.AI;

namespace Main.Scripts.Component
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AvoidNavMeshAgent : MonoBehaviour
    {
        private NavMeshAgent navMeshAgent;
        [SerializeField]
        private int minPriority = 50;
        [SerializeField]
        private int maxPriority = 50;
        [SerializeField]
        private float priorityStepDistance = 1;

        public void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void SetDestination(Vector3? destination)
        {
            if (destination != null)
            {
                var distanceToTarget = Mathf.Max(Vector3.Distance(transform.position, destination.Value) - navMeshAgent.stoppingDistance, 0);
                navMeshAgent.SetDestination(destination.Value);
                navMeshAgent.avoidancePriority = Math.Min(maxPriority, minPriority + 1 + (int) (distanceToTarget / priorityStepDistance));
            }
            else
            {
                navMeshAgent.ResetPath();
                navMeshAgent.avoidancePriority = minPriority;
            }
        }

        public void Move(Vector3 direction)
        {
            navMeshAgent.Move(direction);
        }

        public void OnDisable()
        {
            navMeshAgent.enabled = false;
        }

        public void OnEnable()
        {
            navMeshAgent.enabled = true;
        }
    }
}