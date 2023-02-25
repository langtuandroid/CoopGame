using System;
using JetBrains.Annotations;
using Main.Scripts.UI.Windows.LevelResults;
using Main.Scripts.UI.Windows.SkillTree;
using UnityEngine;

namespace Main.Scripts.UI.Windows
{
    public class WindowsHolder : MonoBehaviour
    {
        [SerializeField]
        private SkillTreePresenter skillTreePresenter;
        [SerializeField]
        private LevelResultsPresenter levelResultsPresenter;

        [CanBeNull]
        public WindowObject GetWindow(WindowType windowType)
        {
            switch (windowType)
            {
                case WindowType.SKILL_TREE:
                    return skillTreePresenter;
                case WindowType.LEVEL_RESULTS:
                    return levelResultsPresenter;
                case WindowType.MENU:
                case WindowType.NONE:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(windowType), windowType, null);
            }
        }
    }
}