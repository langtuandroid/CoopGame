using System;
using System.Collections.Generic;
using Main.Scripts.Core.Resources;
using Newtonsoft.Json.Linq;

namespace Main.Scripts.Player.Data
{
    /**
     * Don't use as Networked field
     */
    public class UserData
    {
        private Dictionary<string, HeroData> heroDataMap = new();

        public static UserData GetInitialUserData(GlobalResources resources)
        {
            var userData = new UserData();
            var heroConfigs = resources.HeroConfigsBank.GetHeroConfigs();
            foreach (var heroConfig in heroConfigs)
            {
                userData.heroDataMap.Add(heroConfig.Id, HeroData.GetInitialHeroData());
            }

            return userData;
        }

        public static UserData ParseJSON(GlobalResources resources, JObject jObject)
        {
            var userData = new UserData();

            var heroConfigs = resources.HeroConfigsBank.GetHeroConfigs();
            foreach (var heroConfig in heroConfigs)
            {
                var jHeroData = jObject.Value<JObject>(heroConfig.Id);
                userData.heroDataMap.Add(
                    heroConfig.Id,
                    jHeroData != null ? HeroData.ParseJSON(resources, jHeroData) : HeroData.GetInitialHeroData()
                );
            }


            return userData;
        }

        public HeroData GetHeroData(string heroId)
        {
            return heroDataMap[heroId];
        }

        public void UpdateHeroData(string heroId, ref HeroData heroData)
        {
            if (!heroDataMap.ContainsKey(heroId))
            {
                throw new Exception($"HeroId {heroId} is not contains user data");
            }

            heroDataMap[heroId] = heroData;
        }

        public JObject ToJSON(GlobalResources resources)
        {
            var jObject = new JObject();

            foreach (var (heroId, heroData) in heroDataMap)
            {
                jObject.Add(heroId, heroData.ToJSON(resources));
            }

            return jObject;
        }
    }
}