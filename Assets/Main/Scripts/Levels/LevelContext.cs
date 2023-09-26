using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Enemies;
using Main.Scripts.Player;
using Main.Scripts.Skills.Common;
using Main.Scripts.Skills.Common.Component;
using Main.Scripts.UI.Windows.HUD;
using UnityEngine;

namespace Main.Scripts.Levels
{
    public class LevelContext : MonoBehaviour
    {
        public static LevelContext? Instance { get; private set; }

        [SerializeField]
        private PlayersHolder playersHolder = default!;
        [SerializeField]
        private EnemiesManager enemiesManager = default!;
        [SerializeField]
        private GameLoopManager gameLoopManager = default!;
        [SerializeField]
        private NavigationManager navigationManager = default!;
        [SerializeField]
        private SkillVisualManager skillVisualManager = default!;
        [SerializeField]
        private HUDScreen hudScreen = default!;

        public PlayersHolder PlayersHolder => playersHolder;
        public EnemiesManager EnemiesManager => enemiesManager;
        public GameLoopManager GameLoopManager => gameLoopManager;
        public NavigationManager NavigationManager => navigationManager;
        public SkillVisualManager SkillVisualManager => skillVisualManager;
        public HUDScreen HudScreen => hudScreen;

        public SkillComponentsPoolHelper SkillComponentsPoolHelper { get; } = new();

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDisable()
        {
            SkillComponentsPoolHelper.Clear();
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}