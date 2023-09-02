using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

namespace Main.Scripts.Enemies
{
    public class NavigationManager : MonoBehaviour
    {
        [SerializeField]
        private int iterationsCount = 3;
        private Dictionary<uint, NavMeshPath> pathMap = new();
        private Dictionary<uint, TaskData> taskMap = new();
        private Queue<uint> taskQueue = new();
        private Stack<NavMeshPath> navMeshPool = new();
        
        private void Update()
        {
            Profiler.BeginSample("NavigationManager::Update");
            for (var i = 0; i < iterationsCount; i++)
            {
                Process();
            }
            Profiler.EndSample();
        }

        public void StartCalculatePath(ref NetworkId id, Vector3 fromPosition, Vector3 toPosition)
        {
            var taskData = new TaskData();

            taskData.fromPosition = fromPosition;
            taskData.toPosition = toPosition;

            var idRaw = id.Raw;

            if (!taskMap.ContainsKey(idRaw))
            {
                taskQueue.Enqueue(idRaw);
                taskData.navMeshPath = GetNavMeshPath();
                taskMap.Add(idRaw, taskData);
            }
            else
            {
                taskData.navMeshPath = taskMap[idRaw].navMeshPath;
                taskMap[idRaw] = taskData;
            }
        }

        public Vector3[] GetPathCorners(ref NetworkId id)
        {
            return pathMap.Remove(id.Raw, out var navMeshPath) ? navMeshPath.corners : Array.Empty<Vector3>();
        }

        public void StopCalculatePath(ref NetworkId id)
        {
            var idRaw = id.Raw;
            if (pathMap.Remove(idRaw, out var navMeshPath))
            {
                navMeshPool.Push(navMeshPath);
            }

            taskMap.Remove(idRaw);
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