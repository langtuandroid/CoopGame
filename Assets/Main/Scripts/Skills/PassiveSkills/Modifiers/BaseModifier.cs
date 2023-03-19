using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Modifiers
{
    public abstract class BaseModifier : ScriptableObject
    {
        [SerializeField]
        private float frequency = 1f;

        public float Frequency => frequency;
    }
}