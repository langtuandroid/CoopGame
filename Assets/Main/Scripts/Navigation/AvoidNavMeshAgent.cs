using System;
using UnityEngine;
using UnityEngine.AI;

namespace Main.Scripts.Navigation
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AvoidNavMeshAgent : MonoBehaviour
    {
        private NavMeshAgent navMeshAgent;
        private int priority;
        [SerializeField]
        private int priorityDelta = 3;
        [SerializeField]
        private float priorityStepDistance = 1;
        
        public void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            priority = navMeshAgent.avoidancePriority;
        }

        public void SetDestination(Vector3? destination)
        {
            if (destination != null)
            {
                var distanceToTarget = Mathf.Max(Vector3.Distance(transform.position, destination.Value) - navMeshAgent.stoppingDistance, 0);
                navMeshAgent.SetDestination(destination.Value);
                navMeshAgent.avoidancePriority = Math.Min(priority, priority - (priorityDelta - (int) (distanceToTarget / priorityStepDistance)));
            }
            else
            {
                navMeshAgent.SetDestination(transform.position);
                navMeshAgent.avoidancePriority = priority - priorityDelta - 1;
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