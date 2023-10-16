using System;
using System.Collections.Generic;
using FSG.MeshAnimator.ShaderAnimated;
using Main.Scripts.Mobs.Config.Animations;
using Main.Scripts.Mobs.Config.Block;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
using UnityEngine;

namespace Main.Scripts.Mobs.Config
{
    [CreateAssetMenu(fileName = "MobConfig", menuName = "Mobs/MobConfig")]
    public class MobConfig : ScriptableObject
    {
        [SerializeField]
        private MobBlockConfigBase logicBlockConfig = null!;
        [SerializeField]
        [Min(0)]
        private int maxHealth = 100;
        [SerializeField]
        private float moveSpeed = 5;
        [SerializeField]
        private Mesh mobMesh = null!;
        [SerializeField]
        private MobAnimationsData animations;
        [SerializeField]
        public ActiveSkillsConfig activeSkillsConfig;
        [SerializeField]
        private PassiveSkillsConfig passiveSkillsConfig;
        [SerializeField]
        private LayerMask alliesLayerMask;
        [SerializeField]
        private LayerMask opponentsLayerMask;

        public MobBlockConfigBase LogicBlockConfig => logicBlockConfig;
        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public Mesh MobMesh => mobMesh;
        public ref MobAnimationsData Animations => ref animations;
        public ref ActiveSkillsConfig ActiveSkillsConfig => ref activeSkillsConfig;
        public ref PassiveSkillsConfig PassiveSkillsConfig => ref passiveSkillsConfig;
        public LayerMask AlliesLayerMask => alliesLayerMask;
        public LayerMask OpponentsLayerMask => opponentsLayerMask;

        public ShaderMeshAnimation[] AnimationsArray { get; private set; } = null!;
        public Dictionary<ShaderMeshAnimation, int> AnimationIndexMap { get; } = new();
        public Dictionary<ActiveSkillType, MobSkillAnimationData> ActiveSkillAnimationsMap { get; } =
            new();

        private void OnValidate()
        {
            PassiveSkillsManager.OnValidate(name, ref PassiveSkillsConfig);
            ActiveSkillsManager.OnValidate(ref ActiveSkillsConfig); //todo validate activate types
        }

        private void OnEnable()
        {
            var animationsList = new List<ShaderMeshAnimation>();

            if (animations.IdleAnimation != null)
            {
                var animation = animations.IdleAnimation;
                AnimationIndexMap[animation] = animationsList.Count;
                animationsList.Add(animation);
            }

            if (animations.RunAnimation != null)
            {
                var animation = animations.RunAnimation;
                AnimationIndexMap[animation] = animationsList.Count;
                animationsList.Add(animation);
            }

            foreach (var skillAnimationData in animations.SkillAnimationsData)
            {
                if (skillAnimationData.CastSkillAnimation != null)
                {
                    var animation = skillAnimationData.CastSkillAnimation;
                    AnimationIndexMap[animation] = animationsList.Count;
                    animationsList.Add(animation);
                }

                if (skillAnimationData.ExecutionSkillAnimation != null)
                {
                    var animation = skillAnimationData.ExecutionSkillAnimation;
                    AnimationIndexMap[animation] = animationsList.Count;
                    animationsList.Add(animation);
                }

                ActiveSkillAnimationsMap[skillAnimationData.SkillType] = skillAnimationData;
            }

            AnimationsArray = animationsList.ToArray();
        }
    }
}