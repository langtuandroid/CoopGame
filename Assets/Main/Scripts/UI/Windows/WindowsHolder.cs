using System;
using Main.Scripts.UI.Windows.LevelResults;
using Main.Scripts.UI.Windows.SkillTree;
using UnityEngine;
using UnityEngine.Serialization;

namespace Main.Scripts.UI.Windows
{
    public class WindowsHolder : MonoBehaviour
    {
        [FormerlySerializedAs("skillTreePresenter")]
        [SerializeField]
        private SkillTreeWindow skillTreeWindow = default!;
        [SerializeField]
        private LevelResultsPresenter levelResultsPresenter = default!;

        public UIScreen? GetWindow(WindowType windowType)
        {
            switch (windowType)
            {
                case WindowType.SKILL_TREE:
                    return skillTreeWindow;
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