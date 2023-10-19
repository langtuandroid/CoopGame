using System;
using System.Collections.Generic;
using Main.Scripts.Customization.Configs;
using UnityEngine;

namespace Main.Scripts.Customization.Banks
{
    public abstract class CustomizationConfigsBankBase<T> : MonoBehaviour where T : CustomizationItemConfigBase
    {
        private List<T> configsList = null!;

        private Dictionary<int, T> configsMap = default!;
        private Dictionary<string, int> configsIdsMap = default!;

        public void Init(List<T> configs)
        {
            configsList = configs;
            
            configsMap = new Dictionary<int, T>(configsList.Count);
            configsIdsMap = new Dictionary<string, int>(configsList.Count);

            for (var i = 0; i < configsList.Count; i++)
            {
                var customization = configsList[i];
                var customizationNameId = customization.NameId;
                if (configsIdsMap.ContainsKey(customizationNameId))
                {
                    throw new ArgumentException(
                        $"{customizationNameId}: CustomizationConfig NameId {customizationNameId} is using by more then one CustomizationConfigs");
                }

                configsMap.Add(i, customization);
                configsIdsMap.Add(customizationNameId, i);
            }
        }

        protected abstract string GetResourcesPath();

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