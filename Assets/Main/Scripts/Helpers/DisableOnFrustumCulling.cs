using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Helpers
{
    public class DisableOnFrustumCulling : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> disableTargets = default!;

        private void OnBecameVisible()
        {
            foreach (var target in disableTargets)
            {
                target.SetActive(true);
            }
        }

        private void OnBecameInvisible()
        {
            foreach (var target in disableTargets)
            {
                target.SetActive(false);
            }
        }
    }
}