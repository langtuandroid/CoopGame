using System;
using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config;
using Main.Scripts.Skills.Common.Component.Config.Action;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component
{
    public class SkillConfigsBank : MonoBehaviour
    {
        [SerializeField]
        private List<SkillConfig> skillConfigs = new();
        [SerializeField]
        private List<SpawnSkillVisualAction> skillVisualConfigs = new();

        private Dictionary<int, SkillConfig> skillsMap = default!;
        private Dictionary<string, int> skillsKeys = default!;

        private Dictionary<int, SpawnSkillVisualAction> skillVisualConfigsMap = default!;
        private Dictionary<string, int> skillVisualConfigsKeys = default!;

        private void Awake()
        {
            skillsMap = new Dictionary<int, SkillConfig>(skillConfigs.Count);
            skillsKeys = new Dictionary<string, int>(skillConfigs.Count);

            for (var i = 0; i < skillConfigs.Count; i++)
            {
                var skillConfig = skillConfigs[i];
                if (skillsKeys.ContainsKey(skillConfig.name))
                {
                    throw new ArgumentException(
                        $"SkillConfig name {skillConfig.name} is using by more then one SkillConfigs");
                }

                skillsMap.Add(i, skillConfig);
                skillsKeys.Add(skillConfig.name, i);
            }
            
            
            skillVisualConfigsMap = new Dictionary<int, SpawnSkillVisualAction>();
            skillVisualConfigsKeys = new Dictionary<string, int>();
            
            for (var i = 0; i < skillVisualConfigs.Count; i++)
            {
                var skillConfig = skillVisualConfigs[i];
                if (skillVisualConfigsKeys.ContainsKey(skillConfig.name))
                {
                    throw new ArgumentException(
                        $"SkillVisualConfig name {skillConfig.name} is using by more then one SkillVisualConfig");
                }

                skillVisualConfigsMap.Add(i, skillConfig);
                skillVisualConfigsKeys.Add(skillConfig.name, i);
            }
        }

        private void OnValidate()
        {
            var keys = new HashSet<string>();
            skillConfigs.Clear();

            var skillConfigsObjects = Resources.LoadAll("Scriptable/Skills", typeof(SkillConfig));
            foreach (var skillConfig in skillConfigsObjects)
            {
                if (skillConfig is SkillConfig config)
                {
                    if (keys.Contains(config.name))
                    {
                        throw new ArgumentException(
                            $"{config.name}: SkillConfig name {config.name} is using in more then one SkillConfigs");
                    }

                    skillConfigs.Add(config);
                    keys.Add(config.name);
                }
            }
            
            keys.Clear();
            skillVisualConfigs.Clear();

            var skillVisualConfigsObjects = Resources.LoadAll("Scriptable/Skills", typeof(SpawnSkillVisualAction));
            foreach (var skillVisualConfig in skillVisualConfigsObjects)
            {
                if (skillVisualConfig is SpawnSkillVisualAction config)
                {
                    if (keys.Contains(config.name))
                    {
                        throw new ArgumentException(
                            $"{config.name}: SpawnSkillVisualAction name {config.name} is using in more then one SpawnSkillVisualAction");
                    }

                    skillVisualConfigs.Add(config);
                    keys.Add(config.name);
                }
            }
        }

        public SkillConfig GetSkillConfig(int key)
        {
            if (!skillsMap.ContainsKey(key))
            {
                throw new ArgumentException($"SkillConfig Key {key} is not registered in SkillConfigsBank");
            }

            return skillsMap[key];
        }

        public int GetSkillConfigId(SkillConfig skillConfig)
        {
            if (!skillsKeys.ContainsKey(skillConfig.name))
            {
                throw new ArgumentException(
                    $"{skillConfig.name}: SkillConfig is not registered in SkillConfigsBank. Check SkillConfig file path.");
            }

            return skillsKeys[skillConfig.name];
        }

        public SpawnSkillVisualAction GetSkillVisualConfig(int key)
        {
            if (!skillVisualConfigsMap.ContainsKey(key))
            {
                throw new ArgumentException($"SpawnSkillVisualAction Key {key} is not registered in SkillConfigsBank");
            }

            return skillVisualConfigsMap[key];
        }

        public int GetSkillVisualConfigKey(SpawnSkillVisualAction skillVisualConfig)
        {
            if (!skillVisualConfigsKeys.ContainsKey(skillVisualConfig.name))
            {
                throw new ArgumentException(
                    $"{skillVisualConfig.name}: SpawnSkillVisualAction is not registered in SkillConfigsBank. Check SpawnSkillVisualAction file path.");
            }

            return skillVisualConfigsKeys[skillVisualConfig.name];
        }
    }
}