using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Skills.Common.Component.Config;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component
{
    public class SkillConfigsBank : MonoBehaviour
    {
        public static SkillConfigsBank? Instance;

        [SerializeField]
        private List<SkillConfig> skillConfigs = new();

        private Dictionary<int, SkillConfig> skillsMap = default!;
        private Dictionary<string, int> skillsIds = default!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;

            skillsMap = new Dictionary<int, SkillConfig>(skillConfigs.Count);
            skillsIds = new Dictionary<string, int>(skillConfigs.Count);

            for (var i = 0; i < skillConfigs.Count; i++)
            {
                var skillConfig = skillConfigs[i];
                if (skillsIds.ContainsKey(skillConfig.name))
                {
                    throw new ArgumentException(
                        $"SkillConfig name {skillConfig.name} is using by more then one SkillConfigs");
                }

                skillsMap.Add(i, skillConfig);
                skillsIds.Add(skillConfig.name, i);
            }
        }

        private void OnValidate()
        {
            var ids = new HashSet<string>();
            skillConfigs.Clear();

            var skillConfigsObjects = Resources.LoadAll("Scriptable/Skills", typeof(SkillConfig));
            foreach (var skillConfig in skillConfigsObjects)
            {
                if (skillConfig is SkillConfig config)
                {
                    if (ids.Contains(config.name))
                    {
                        throw new ArgumentException(
                            $"{config.name}: SkillConfig name {config.name} is using in more then one SkillConfigs");
                    }

                    skillConfigs.Add(config);
                    ids.Add(config.name);
                }
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public SkillConfig GetSkillConfig(int id)
        {
            if (!skillsMap.ContainsKey(id))
            {
                throw new ArgumentException($"SkillConfig Id {id} is not registered in SkillConfigsBank");
            }

            return skillsMap[id];
        }

        public int GetSkillConfigId(SkillConfig skillConfig)
        {
            if (!skillsIds.ContainsKey(skillConfig.name))
            {
                throw new ArgumentException(
                    $"{skillConfig.name}: SkillConfig is not registered in SkillConfigsBank. Check SkillConfig file path.");
            }

            return skillsIds[skillConfig.name];
        }
    }
}