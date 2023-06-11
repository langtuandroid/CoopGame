using System;
using System.Collections.Generic;
using Main.Scripts.Customization.Configs;
using UnityEngine;

namespace Main.Scripts.Customization.Banks
{
    public abstract class CustomizationConfigsBankBase<T> : MonoBehaviour where T : CustomizationItemConfigBase
    {
        [SerializeField]
        private List<T> configsList = new();

        private Dictionary<int, T> configsMap = default!;
        private Dictionary<string, int> configsIdsMap = default!;

        private void Awake()
        {
            configsMap = new Dictionary<int, T>(configsList.Count);
            configsIdsMap = new Dictionary<string, int>(configsList.Count);

            for (var i = 0; i < configsList.Count; i++)
            {
                var customizationConfig = configsList[i];
                if (configsIdsMap.ContainsKey(customizationConfig.NameId))
                {
                    throw new ArgumentException(
                        $"{customizationConfig.name}: CustomizationConfig NameId {customizationConfig.NameId} is using by more then one CustomizationConfigs");
                }

                configsMap.Add(i, customizationConfig);
                configsIdsMap.Add(customizationConfig.NameId, i);
            }
        }

        protected abstract string GetResourcesPath();

        private void OnValidate()
        {
            var ids = new HashSet<string>();
            configsList.Clear();

            var configs = Resources.LoadAll(GetResourcesPath(), typeof(T));
            foreach (var config in configs)
            {
                if (config is T typedConfig)
                {
                    if (ids.Contains(typedConfig.NameId))
                    {
                        throw new ArgumentException(
                            $"{typedConfig.NameId}: CustomizationConfig NameId {typedConfig.NameId} is using in more then one CustomizationConfigs of the same type");
                    }

                    configsList.Add(typedConfig);
                    ids.Add(typedConfig.NameId);
                }
            }
        }

        public IEnumerable<T> GetConfigs()
        {
            return configsList;
        }

        public T GetCustomizationConfig(int id)
        {
            if (!configsMap.ContainsKey(id))
            {
                throw new ArgumentException(
                    $"CustomizationConfig Id {id} is not registered in CustomizationConfigsBank");
            }

            return configsMap[id];
        }

        public int GetCustomizationConfigId(string nameId)
        {
            if (!configsIdsMap.ContainsKey(nameId))
            {
                throw new ArgumentException(
                    $"{nameId}: CustomizationConfig is not registered in customizationConfigsBank. Check CustomizationConfig file path.");
            }

            return configsIdsMap[nameId];
        }
    }
}