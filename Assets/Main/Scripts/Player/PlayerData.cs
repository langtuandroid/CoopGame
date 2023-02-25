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
        public uint Level;
        public uint Experience;
        public uint MaxSkillPoints;
        public uint UsedSkillPoints;
        [Networked, Capacity(16)]
        public NetworkDictionary<SkillType, uint> SkillLevels => default;

        public static PlayerData GetInitialPlayerData()
        {
            var playerData = new PlayerData();
            playerData.Level = 1;
            playerData.Experience = 0;
            playerData.MaxSkillPoints = ExperienceHelper.GetMaxSkillPointsByLevel(playerData.Level);
            playerData.UsedSkillPoints = 0;
            foreach (var skill in Enum.GetValues(typeof(SkillType)).Cast<SkillType>())
            {
                playerData.SkillLevels.Set(skill, 0);
            }

            return playerData;
        }

        public static PlayerData parseJSON(JObject jObject)
        {
            var playerData = new PlayerData();
            playerData.Level = jObject.Value<uint>(LEVEL_KEY);
            playerData.Experience = jObject.Value<uint>(EXPERIENCE_KEY);
            playerData.MaxSkillPoints = jObject.Value<uint>(MAX_SKILL_POINTS_KEY);
            playerData.UsedSkillPoints = jObject.Value<uint>(USED_SKILL_POINTS_KEY);
            var jSkillLevels = jObject.Value<JObject>(SKILL_LEVELS_KEY);
            foreach (var skillType in Enum.GetValues(typeof(SkillType)).Cast<SkillType>())
            {
                playerData.SkillLevels.Add(skillType, jSkillLevels.Value<uint>(skillType.GetKey()));
            }

            return playerData;
        }

        public JObject toJSON()
        {
            var jObject = new JObject();
            jObject.Add(LEVEL_KEY, Level);
            jObject.Add(EXPERIENCE_KEY, Experience);
            jObject.Add(MAX_SKILL_POINTS_KEY, MaxSkillPoints);
            jObject.Add(USED_SKILL_POINTS_KEY, UsedSkillPoints);
            var jSkillLevels = new JObject();
            foreach (var skillType in Enum.GetValues(typeof(SkillType)).Cast<SkillType>())
            {
                jSkillLevels.Add(skillType.GetKey(), SkillLevels.Get(skillType));
            }

            jObject.Add(SKILL_LEVELS_KEY, jSkillLevels);
            return jObject;
        }

        public uint GetAvailableSkillPoints()
        {
            return MaxSkillPoints - UsedSkillPoints;
        }

        private const string LEVEL_KEY = "level";
        private const string EXPERIENCE_KEY = "experience";
        private const string MAX_SKILL_POINTS_KEY = "max_skill_points";
        private const string USED_SKILL_POINTS_KEY = "used_skill_points";
        private const string SKILL_LEVELS_KEY = "skill_levels";
    }
}