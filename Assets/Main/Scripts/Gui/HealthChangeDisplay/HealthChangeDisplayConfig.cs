using System;
using UnityEngine;

namespace Main.Scripts.Gui.HealthChangeDisplay
{
    [Serializable]
    public struct HealthChangeDisplayConfig
    {
        public bool ShowHealthChangeDisplay;
        public Transform interpolationTransform;
        public int tickBufferStep;
        public float textLifeTimer;
        public float textSpeed;
        public HealthChangeDisplay healthChangeDisplay;

        public static HealthChangeDisplayConfig GetDefault()
        {
            var config = new HealthChangeDisplayConfig();
            config.tickBufferStep = 5;
            config.textLifeTimer = 0.5f;
            config.textSpeed = 5f;
            return config;
        }
    }
}