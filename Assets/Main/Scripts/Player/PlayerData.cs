using System;
using System.Linq;
using Fusion;
using Main.Scripts.Player.Experience;
using Main.Scripts.Skills;
using Newtonsoft.Json.Linq;

namespace Main.Scripts.Player
{
    public struct PlayerData : INetworkStruct
    {
        public int Level;
        public int Experience;
        public int MaxSkillPoints;
        public int AvailableSkillPoints;
        [Networked, Capacity(16)]
        public NetworkDictionary<SkillType, int> SkillLevels => default;

        public static PlayerData GetInitialPlayerData()
        {
            var playerData = new PlayerData();
            playerData.Level = 1;
            playerData.Experience = 0;
            playerData.MaxSkillPoints = ExperienceHelper.GetMaxSkillPointsByLevel(playerData.Level);
            playerData.AvailableSkillPoints = playerData.MaxSkillPoints;
            foreach (var skill in Enum.GetValues(typeof(SkillType)).Cast<SkillType>())
            {
                playerData.SkillLevels.Set(skill, 0);
            }

            return playerData;
        }

        public static PlayerData parseJSON(JObject jObject)
        {
            var playerData = new PlayerData();
            playerData.Level = jObject.Value<int>(LEVEL_KEY);
            playerData.Experience = jObject.Value<int>(EXPERIENCE_KEY);
            playerData.MaxSkillPoints = jObject.Value<int>(MAX_SKILL_POINTS_KEY);
            playerData.AvailableSkillPoints = jObject.Value<int>(AVAILABLE_SKILL_POINTS_KEY);
            var jSkillLevels = jObject.Value<JObject>(SKILL_LEVELS_KEY);
            foreach (var skillType in Enum.GetValues(typeof(SkillType)).Cast<SkillType>())
            {
                playerData.SkillLevels.Add(skillType, jSkillLevels.Value<int>(skillType.GetKey()));
            }

            return playerData;
        }

        public JObject toJSON()
        {
            var jObject = new JObject();
            jObject.Add(LEVEL_KEY, Level);
            jObject.Add(EXPERIENCE_KEY, Experience);
            jObject.Add(MAX_SKILL_POINTS_KEY, MaxSkillPoints);
            jObject.Add(AVAILABLE_SKILL_POINTS_KEY, AvailableSkillPoints);
            var jSkillLevels = new JObject();
            foreach (var skillType in Enum.GetValues(typeof(SkillType)).Cast<SkillType>())
            {
                jSkillLevels.Add(skillType.GetKey(), SkillLevels.Get(skillType));
            }

            jObject.Add(SKILL_LEVELS_KEY, jSkillLevels);
            return jObject;
        }

        private const string LEVEL_KEY = "level";
        private const string EXPERIENCE_KEY = "experience";
        private const string MAX_SKILL_POINTS_KEY = "max_skill_points";
        private const string AVAILABLE_SKILL_POINTS_KEY = "available_skill_points";
        private const string SKILL_LEVELS_KEY = "skill_levels";
    }
}