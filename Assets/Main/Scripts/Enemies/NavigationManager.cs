using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace Main.Scripts.Enemies
{
    public class NavigationManager : MonoBehaviour
    {
        private Dictionary<NetworkId, NavMeshPath> pathMap = new();
        private Dictionary<NetworkId, TaskData> taskMap = new();
        private Queue<NetworkId> taskQueue = new();
        private Stack<NavMeshPath> navMeshPool = new();
        
        private void Update()
        {
            for (var i = 0; i < 10; i++)
            {
                Process();
            }
        }

        public void StartCalculatePath(NetworkId id, Vector3 fromPosition, Vector3 toPosition)
        {
            var taskData = new TaskData();

            taskData.fromPosition = fromPosition;
            taskData.toPosition = toPosition;

            if (!taskMap.ContainsKey(id))
            {
                taskQueue.Enqueue(id);
                taskData.navMeshPath = GetNavMeshPath();
                taskMap.Add(id, taskData);
            }
            else
            {
                taskData.navMeshPath = taskMap[id].navMeshPath;
                taskMap[id] = taskData;
            }
        }

        public Vector3[]? GetPathCorners(NetworkId id)
        {
            Vector3[]? navMeshPathCorners = null;

            if (pathMap.Remove(id, out var navMeshPath))
            {
                navMeshPathCorners = navMeshPath.corners;
            }

            return navMeshPathCorners;
        }

        public void StopCalculatePath(NetworkId id)
        {
            if (pathMap.Remove(id, out var navMeshPath))
            {
                navMeshPool.Push(navMeshPath);
            }

            taskMap.Remove(id);
        }

        private void Process()
        {
            if (taskQueue.Count == 0)
            {
                return;
            }

            var id = taskQueue.Dequeue();
            if (!taskMap.Remove(id, out var taskData))
            {
                return;
            }

            var navMeshPath = taskData.navMeshPath;

            NavMesh.CalculatePath(taskData.fromPosition, taskData.toPosition, NavMesh.AllAreas, navMeshPath);


            if (pathMap.ContainsKey(id))
            {
                navMeshPool.Push(pathMap[id]);
            }
            pathMap[id] = navMeshPath;
        }

        private NavMeshPath GetNavMeshPath()
        {
            return navMeshPool.Count > 0 ? navMeshPool.Pop() : new NavMeshPath();
        }
    }

    internal struct TaskData
    {
        public Vector3 fromPosition;
        public Vector3 toPosition;
        public NavMeshPath navMeshPath;
    }
}