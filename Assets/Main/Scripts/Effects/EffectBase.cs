using UnityEngine;

namespace Main.Scripts.Effects
{
    public abstract class EffectBase : ScriptableObject
    {
        [SerializeField]
        private string id = "";
        [SerializeField]
        private bool isStackable;
        [SerializeField]
        private float durationSec;

        public string Id => id;
        public bool IsStackable => isStackable;
        public float DurationSec => durationSec;
    }
}