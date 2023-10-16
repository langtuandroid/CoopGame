using System;
using Main.Scripts.Gui;
using Main.Scripts.Gui.HealthChangeDisplay;
using UnityEngine.UIElements;

namespace Main.Scripts.Player
{
    [Serializable]
    public struct PlayerPrefabData
    {
        public UIDocument InteractionInfoDoc;
        public HealthBar HealthBar;
        public HealthChangeDisplayConfig HealthChangeDisplayConfig;

        public static PlayerPrefabData GetDefault()
        {
            var config = new PlayerPrefabData();
            config.HealthChangeDisplayConfig = HealthChangeDisplayConfig.GetDefault();
            return config;
        }
    }
}