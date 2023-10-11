using UnityEngine;

namespace Main.Scripts.Modifiers
{
    public abstract class ModifierBase : ScriptableObject
    {
        [SerializeField]
        [Min(1)]
        private int heatLevel;
        public int HeatLevel => heatLevel;
    }
}