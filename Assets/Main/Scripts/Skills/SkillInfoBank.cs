using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Skills
{
    public class SkillInfoBank : MonoBehaviour
    {
        [SerializeField]
        private List<SkillInfo> skillInfos = null!;
        
        private Dictionary<int, SkillInfo> skillInfosMap = null!;
        private Dictionary<SkillInfo, int> skillInfosKeys = null!;
        
        private void Awake()
        {
            skillInfosMap = new Dictionary<int, SkillInfo>();
            skillInfosKeys = new Dictionary<SkillInfo, int>();

            for (var i = 0; i < skillInfos.Count; i++)
            {
                var skillInfo = skillInfos[i];

                skillInfosMap.Add(i, skillInfo);
                skillInfosKeys.Add(skillInfo, i);
            }
        }

        private void OnValidate()
        {
            skillInfos.Clear();

            var skillInfoConfigs = Resources.LoadAll("Scriptable/SkillInfo", typeof(SkillInfo));
            foreach (var skillInfo in skillInfoConfigs)
            {
                if (skillInfo is SkillInfo config)
                {
                    skillInfos.Add(config);
                }
            }
        }
        
        public IEnumerable<SkillInfo> GetSkillInfos()
        {
            return skillInfos;
        }

        public SkillInfo GetSkillInfo(int key)
        {
            if (!skillInfosMap.ContainsKey(key))
            {
                throw new ArgumentException($"MobConfig Key {key} is not registered in MobConfigsBank");
            }

            return skillInfosMap[key];
        }

        public int GetSkillInfoKey(SkillInfo skillInfo)
        {
            if (!skillInfosKeys.ContainsKey(skillInfo))
            {
                throw new ArgumentException(
                    $"{skillInfo.name}: MobConfig is not registered in MobConfigsBank. Check MobConfig file path.");
            }

            return skillInfosKeys[skillInfo];
        }
    }
}