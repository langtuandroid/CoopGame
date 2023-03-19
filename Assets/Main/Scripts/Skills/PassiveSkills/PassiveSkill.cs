using System.Collections.Generic;
using Main.Scripts.Skills.PassiveSkills.Modifiers;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills
{
    [CreateAssetMenu(fileName = "PassiveSkill", menuName = "Scriptable/PassiveSkill")]
    public class PassiveSkill : ScriptableObject
    {
        public List<BaseModifier> passiveSkillModifiers = new();
    }
}