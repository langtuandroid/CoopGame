using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Main.Scripts.Connection
{
    /// <summary>
    /// Pool of all free instances of a single type of NetworkObject's
    /// </summary>
    public class FusionObjectPool
    {
        private Stack<NetworkObject> _free = new();

        public NetworkObject? GetFromPool(Vector3 p, Quaternion q, Transform? parent = null)
        {
            NetworkObject? newt = null;

            while (newt == null && _free.TryPop(out var t))
            {
                if (t) // In case a recycled object was destroyed
                {
                    Transform xform = t.transform;
                    xform.SetParent(parent, false);
                    xform.position = p;
                    xform.rotation = q;
                    newt = t;
                }
                else
                {
                    Debug.LogWarning("Recycled object was destroyed - not re-using!");
                }
            }

            return newt;
        }

        public void Clear()
        {
            foreach (var pooled in _free)
            {
                if (pooled)
                {
                    Debug.Log($"Destroying pooled object: {pooled.gameObject.name}");
                    Object.Destroy(pooled.gameObject);
                }
            }

            _free = new Stack<NetworkObject>();
        }

        public void ReturnToPool(NetworkObject no)
        {
            _free.Push(no);
        }
    }
}