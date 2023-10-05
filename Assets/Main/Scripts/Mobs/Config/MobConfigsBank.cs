using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Mobs.Config
{
    public class MobConfigsBank : MonoBehaviour
    {
        [SerializeField]
        private List<MobConfig> mobConfigs = new();

        private Dictionary<int, MobConfig> mobConfigsMap = null!;
        private Dictionary<MobConfig, int> mobConfigsKeys = null!;

        private void Awake()
        {
            mobConfigsMap = new Dictionary<int, MobConfig>();
            mobConfigsKeys = new Dictionary<MobConfig, int>();

            for (var i = 0; i < mobConfigs.Count; i++)
            {
                var mobConfig = mobConfigs[i];

                mobConfigsMap.Add(i, mobConfig);
                mobConfigsKeys.Add(mobConfig, i);
            }
        }

        private void OnValidate()
        {
            mobConfigs.Clear();

            var mobConfigsObjects = Resources.LoadAll("Scriptable/Mobs", typeof(MobConfig));
            foreach (var mobConfig in mobConfigsObjects)
            {
                if (mobConfig is MobConfig config)
                {
                    mobConfigs.Add(config);
                }
            }
        }

        public MobConfig GetMobConfig(int key)
        {
            if (!mobConfigsMap.ContainsKey(key))
            {
                throw new ArgumentException($"MobConfig Key {key} is not registered in MobConfigsBank");
            }

            return mobConfigsMap[key];
        }

        public int GetMobConfigKey(MobConfig mobConfig)
        {
            if (!mobConfigsKeys.ContainsKey(mobConfig))
            {
                throw new ArgumentException(
                    $"{mobConfig.name}: MobConfig is not registered in MobConfigsBank. Check MobConfig file path.");
            }

            return mobConfigsKeys[mobConfig];
        }
    }
}