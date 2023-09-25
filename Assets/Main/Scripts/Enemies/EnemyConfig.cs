using System;
using Main.Scripts.Gui;
using Main.Scripts.Gui.HealthChangeDisplay;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;

namespace Main.Scripts.Enemies
{
    [Serializable]
    public struct EnemyConfig
    {
        public float DefaultMaxHealth;
        public float DefaultSpeed;
        public float AttackDistance; //todo replace to enemy logic config
        
        public HealthBar HealthBar;

        public ActiveSkillsConfig ActiveSkillsConfig;
        public PassiveSkillsConfig PassiveSkillsConfig;

        public bool ShowHealthChangeDisplay;
        public HealthChangeDisplayConfig HealthChangeDisplayConfig;

        public static EnemyConfig GetDefault()
        {
            var config = new EnemyConfig();
            config.DefaultMaxHealth = 100;
            config.DefaultSpeed = 10;
            config.AttackDistance = 3;
            config.ShowHealthChangeDisplay = false;
            config.HealthChangeDisplayConfig = HealthChangeDisplayConfig.GetDefault();
            return config;
        }
    }
}