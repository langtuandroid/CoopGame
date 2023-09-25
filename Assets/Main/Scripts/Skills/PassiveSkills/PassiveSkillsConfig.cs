using System;
using System.Collections.Generic;
using Main.Scripts.Effects;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills
{
    [Serializable]
    public struct PassiveSkillsConfig
    {
        public LayerMask AlliesLayerMask;
        public LayerMask OpponentsLayerMask;
        public List<PassiveSkillControllerData> PassiveSkillControllersDataList;
        public List<EffectsCombination> InitialEffects;
    }
}