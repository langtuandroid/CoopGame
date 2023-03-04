using System;
using Main.Scripts.UI.Windows.LevelResults;
using Main.Scripts.UI.Windows.SkillTree;
using UnityEngine;

namespace Main.Scripts.UI.Windows
{
    public class UIScreensHolder : MonoBehaviour
    {
        [SerializeField]
        private SkillTreeWindow skillTreeScreen = default!;
        [SerializeField]
        private LevelResultsPresenter levelResultsPresenter = default!;

        public UIScreen? GetWindow(ScreenType screenType)
        {
            switch (screenType)
            {
                case ScreenType.SKILL_TREE:
                    return skillTreeScreen;
                case ScreenType.LEVEL_RESULTS:
                    return levelResultsPresenter;
                case ScreenType.MENU:
                case ScreenType.NONE:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(screenType), screenType, null);
            }
        }
    }
}