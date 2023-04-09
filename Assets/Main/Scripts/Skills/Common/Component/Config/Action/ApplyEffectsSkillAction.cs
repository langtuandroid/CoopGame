using System.Collections.Generic;
using Main.Scripts.Effects;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "ApplyEffectsAction", menuName = "Skill/Action/ApplyEffects")]
    public class ApplyEffectsSkillAction : SkillActionBase
    {
        [SerializeField]
        private List<EffectsCombination> effectsCombinations = new();

        public List<EffectsCombination> EffectsCombinations => effectsCombinations;
    }
}