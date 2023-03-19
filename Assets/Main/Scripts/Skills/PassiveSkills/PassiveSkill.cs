using System.Collections.Generic;
using Main.Scripts.Skills.PassiveSkills.Effects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Main.Scripts.Skills.PassiveSkills
{
    [CreateAssetMenu(fileName = "PassiveSkill", menuName = "Scriptable/PassiveSkill")]
    public class PassiveSkill : ScriptableObject
    {
        [FormerlySerializedAs("passiveSkillModifiers")]
        public List<BaseEffect> passiveSkillEffects = new();
    }
}