using Fusion;
using Main.Scripts.Core.Resources;
using Main.Scripts.Player.Experience;
using Newtonsoft.Json.Linq;
using Main.Scripts.Utils;

namespace Main.Scripts.Player.Data
{
    /**
     * Don't use as Networked field
     */
    public struct PlayerData : INetworkStruct
    {
        public uint Level;
        public uint Experience;
        public uint MaxSkillPoints;
        public uint UsedSkillPoints;
        public CustomizationData Customization;
        public ModifiersData Modifiers;

        public static PlayerData GetInitialPlayerData()
        {
            var playerData = new PlayerData();
            playerData.Level = 1;
            playerData.Experience = 0;
            playerData.MaxSkillPoints = ExperienceHelper.GetMaxSkillPointsByLevel(playerData.Level);
            playerData.UsedSkillPoints = 0;

            playerData.Customization = CustomizationData.GetDefault();
            playerData.Modifiers = ModifiersData.GetDefault();

            return playerData;
        }

        public static PlayerData ParseJSON(GlobalResources resources, JObject jObject)
        {
            var playerData = new PlayerData();
            playerData.Level = jObject.Value<uint>(KEY_LEVEL);
            playerData.Experience = jObject.Value<uint>(KEY_EXPERIENCE);
            playerData.MaxSkillPoints = jObject.Value<uint>(KEY_MAX_SKILL_POINTS);
            playerData.UsedSkillPoints = jObject.Value<uint>(KEY_USED_SKILL_POINTS);

            playerData.Customization =
                CustomizationData.ParseJSON(resources, jObject.Value<JObject>(KEY_CUSTOMIZATION).ThrowWhenNull());

            playerData.Modifiers = ModifiersData.ParseJSON(resources, jObject.Value<JArray>(KEY_MODIFIERS));

            return playerData;
        }

        public JObject ToJSON(GlobalResources resources)
        {
            var jObject = new JObject();
            jObject.Add(KEY_LEVEL, Level);
            jObject.Add(KEY_EXPERIENCE, Experience);
            jObject.Add(KEY_MAX_SKILL_POINTS, MaxSkillPoints);
            jObject.Add(KEY_USED_SKILL_POINTS, UsedSkillPoints);
            jObject.Add(KEY_CUSTOMIZATION, Customization.toJSON(resources));
            jObject.Add(KEY_MODIFIERS, Modifiers.toJSONArray(resources));
            return jObject;
        }

        public uint GetAvailableSkillPoints()
        {
            return MaxSkillPoints - UsedSkillPoints;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerData playerData && Equals(playerData);
        }

        public override int GetHashCode()
        {
            return Level.GetHashCode()
                   ^ Experience.GetHashCode()
                   ^ MaxSkillPoints.GetHashCode()
                   ^ UsedSkillPoints.GetHashCode();
        }

        public bool Equals(PlayerData other)
        {
            return Level.Equals(other.Level)
                   && Experience.Equals(other.Experience)
                   && MaxSkillPoints.Equals(other.MaxSkillPoints)
                   && UsedSkillPoints.Equals(other.UsedSkillPoints);
        }

        private const string KEY_LEVEL = "level";
        private const string KEY_EXPERIENCE = "experience";
        private const string KEY_MAX_SKILL_POINTS = "max_skill_points";
        private const string KEY_USED_SKILL_POINTS = "used_skill_points";
        private const string KEY_CUSTOMIZATION = "customization";
        private const string KEY_MODIFIERS = "modifiers";
    }
}