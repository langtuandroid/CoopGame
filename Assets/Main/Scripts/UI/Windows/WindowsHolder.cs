using System;
using Main.Scripts.UI.Windows.SkillTree;
using UnityEngine;

namespace Main.Scripts.UI.Windows
{
    public class WindowsHolder : MonoBehaviour
    {
        [SerializeField]
        private SkillTreePresenter skillTreePresenter;

        public WindowObject GetWindow(WindowType windowType)
        {
            switch (windowType)
            {
                case WindowType.SKILL_TREE:
                    return skillTreePresenter;
                case WindowType.NONE:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(windowType), windowType, null);
            }
        }
    }
}