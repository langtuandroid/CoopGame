using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Effects
{
    [CreateAssetMenu(fileName = "EffectsCombination", menuName = "Scriptable/Effects/EffectsCombination")]
    public class EffectsCombination : ScriptableObject
    {
        public string NameId => name;
        public List<EffectConfigBase> Effects = new();

        public void OnValidate()
        {
            foreach (var effect in Effects)
            {
                if (effect == null) throw new ArgumentNullException($"{name}: effect combination has null effect");
            }
        }
    }
}