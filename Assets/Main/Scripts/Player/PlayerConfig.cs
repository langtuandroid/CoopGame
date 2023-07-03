using System;
using Main.Scripts.Gui;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
using UnityEngine.UIElements;

namespace Main.Scripts.Player
{
    [Serializable]
    public struct PlayerConfig
    {
        public uint DefaultMaxHealth;
        public float DefaultSpeed;
        
        public UIDocument InteractionInfoDoc;
        public HealthBar HealthBar;

        public ActiveSkillsConfig ActiveSkillsConfig;
        public PassiveSkillsConfig PassiveSkillsConfig;

        public bool ShowHealthChangeDisplay;
        public HealthChangeDisplayConfig HealthChangeDisplayConfig;

        public static PlayerConfig GetDefault()
        {
            var config = new PlayerConfig();
            config.DefaultMaxHealth = 100;
            config.DefaultSpeed = 6f;
            config.ShowHealthChangeDisplay = false;
            config.HealthChangeDisplayConfig = HealthChangeDisplayConfig.GetDefault();
            return config;
        }
    }
}