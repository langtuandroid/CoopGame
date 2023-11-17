using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Player.Config
{
    public class HeroConfigsBank : MonoBehaviour
    {
        [SerializeField]
        private List<HeroConfig> heroConfigs = new();

        private Dictionary<int, HeroConfig> heroConfigsMap = null!;
        private Dictionary<string, int> heroConfigsKeys = null!;

        private void Awake()
        {
            heroConfigsMap = new Dictionary<int, HeroConfig>();
            heroConfigsKeys = new Dictionary<string, int>();

            for (var i = 0; i < heroConfigs.Count; i++)
            {
                var heroConfig = heroConfigs[i];

                heroConfigsMap.Add(i, heroConfig);
                heroConfigsKeys.Add(heroConfig.Id, i);
            }
        }

        private void OnValidate()
        {
            foreach (var heroConfig in heroConfigs)
            {
                if (heroConfig == null)
                {
                    throw new ArgumentException($"heroConfigs has null value in HeroConfigsBank");
                }
            }
        }

        public IEnumerable<HeroConfig> GetHeroConfigs()
        {
            return heroConfigs;
        }

        public HeroConfig GetHeroConfig(int key)
        {
            if (!heroConfigsMap.ContainsKey(key))
            {
                throw new ArgumentException($"HeroConfig Key {key} is not registered in HeroConfigsBank");
            }

            return heroConfigsMap[key];
        }

        public int GetHeroConfigKey(string heroId)
        {
            if (!heroConfigsKeys.ContainsKey(heroId))
            {
                throw new ArgumentException(
                    $"{heroId}: HeroConfig is not registered in HeroConfigsBank. Check HeroConfig file path.");
            }

            return heroConfigsKeys[heroId];
        }
    }
}