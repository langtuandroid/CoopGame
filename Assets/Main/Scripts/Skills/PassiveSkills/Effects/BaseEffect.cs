using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Effects
{
    public abstract class BaseEffect : ScriptableObject, Effect
    {
        [SerializeField]
        private float frequency = 1f;

        public float GetFrequency()
        {
            return frequency;
        }
    }
}