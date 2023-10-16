using System;
using Fusion;
using Main.Scripts.UI.Windows.Customization;
using Main.Scripts.UI.Windows.DebugPanel;
using Main.Scripts.UI.Windows.HeroPicker;
using Main.Scripts.UI.Windows.LevelResults;
using Main.Scripts.UI.Windows.SkillTree;
using UnityEngine;

namespace Main.Scripts.UI.Windows
{
    public class UIScreensHolder : MonoBehaviour
    {
        public static UIScreensHolder? Instance { get; private set; }
        
        [SerializeField]
        private SkillTreeWindow skillTreeScreen = null!;
        [SerializeField]
        private LevelResultsPresenter levelResultsPresenter = null!;
        [SerializeField]
        private CustomizationWindow customizationWindow = null!;
        [SerializeField]
        private DebugWindow debugWindow = null!;
        [SerializeField]
        private HeroPickerWindow heroPickerWindow = null!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public UIScreen? GetWindow(ScreenType screenType)
        {
            switch (screenType)
            {
                case ScreenType.SKILL_TREE:
                    return skillTreeScreen;
                case ScreenType.LEVEL_RESULTS:
                    return levelResultsPresenter;
                case ScreenType.CUSTOMIZATION:
                    return customizationWindow;
                case ScreenType.DEBUG:
                    return debugWindow;
                case ScreenType.HERO_PICKER:
                    return heroPickerWindow;
                case ScreenType.MENU:
                case ScreenType.NONE:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(screenType), screenType, null);
            }
        }
    }
}