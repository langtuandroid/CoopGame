using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Main.Scripts.Connection
{
	public class FusionObjectPoolRoot : MonoBehaviour, INetworkObjectPool
	{
		private Dictionary<object, FusionObjectPool> _poolsByPrefab = new Dictionary<object, FusionObjectPool>();
		private Dictionary<NetworkObject, FusionObjectPool> _poolsByInstance = new Dictionary<NetworkObject, FusionObjectPool>();

		private FusionObjectPool GetPool<T>(T prefab) where T : NetworkObject
		{
			FusionObjectPool pool;
			if (!_poolsByPrefab.TryGetValue(prefab, out pool))
			{
				pool = new FusionObjectPool();
				_poolsByPrefab[prefab] = pool;
			}

			return pool;
		}

		public NetworkObject? AcquireInstance(NetworkRunner runner, NetworkPrefabInfo info)
		{
			NetworkObject prefab;
			if (NetworkProjectConfig.Global.PrefabTable.TryGetPrefab(info.Prefab, out prefab))
			{
				var pool = GetPool(prefab);
				var newt = pool.GetFromPool(Vector3.zero, Quaternion.identity);

				if (newt == null)
				{
					newt = Instantiate(prefab, Vector3.zero, Quaternion.identity);
					_poolsByInstance[newt] = pool;
				}

				SceneManager.MoveGameObjectToScene(newt.gameObject, SceneManager.GetActiveScene());
				newt.gameObject.SetActive(true);
				return newt;
			}

			Debug.LogError("No prefab for " + info.Prefab);
			return null;
		}

		public void ReleaseInstance(NetworkRunner runner, NetworkObject no, bool isSceneObject)
		{
			if (no != null)
			{
				FusionObjectPool pool;
				if (_poolsByInstance.TryGetValue(no, out pool))
				{
					pool.ReturnToPool(no);
					no.gameObject.SetActive(false); // Should always disable before re-parenting, or we will dirty it twice
					no.transform.SetParent(transform, false);
				}
				else
				{
					no.gameObject.SetActive(false); // Should always disable before re-parenting, or we will dirty it twice
					no.transform.SetParent(null, false);
					Destroy(no.gameObject);
				}
			}
		}

		public void ClearPools()
		{
			foreach (FusionObjectPool pool in _poolsByPrefab.Values)
			{
				pool.Clear();
			}

			foreach (FusionObjectPool pool in _poolsByInstance.Values)
			{
				pool.Clear();
			}

			_poolsByPrefab = new Dictionary<object, FusionObjectPool>();
		}
	}
}