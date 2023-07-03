using UnityEngine;

namespace Main.Scripts.Core.Architecture
{
    public interface ComponentsHolder
    {
        public T GetCachedComponent<T>() where T : Component;
    }
}