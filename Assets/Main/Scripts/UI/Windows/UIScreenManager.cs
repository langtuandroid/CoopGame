using System;
using System.Linq;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.UI.Windows
{
    public class UIScreenManager : MonoBehaviour
    {
        private UIScreensHolder uiScreensHolder = default!;

        public ScreenType CurrentScreenType { get; private set; }
        public UnityEvent<ScreenType> OnCurrentScreenChangedEvent = default!;

        private void Awake()
        {
            uiScreensHolder = FindObjectOfType<UIScreensHolder>().ThrowWhenNull();
            CurrentScreenType = ScreenType.NONE;
        }

        private void OnDestroy()
        {
            OnCurrentScreenChangedEvent.RemoveAllListeners();
        }

        private void Update()
        {
            if (CurrentScreenType != ScreenType.NONE)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SetScreenType(ScreenType.NONE);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                SetScreenType(ScreenType.SKILL_TREE);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetScreenType(ScreenType.MENU);
                return;
            }
        }

        public void SetScreenType(ScreenType newScreenType)
        {
            Debug.Log($"Set {newScreenType} screen");
            CurrentScreenType = newScreenType;

            foreach (var screenType in Enum.GetValues(typeof(ScreenType)).Cast<ScreenType>())
            {
                uiScreensHolder.GetWindow(screenType)?.Hide();
            }

            if (CurrentScreenType != ScreenType.NONE)
            {
                uiScreensHolder.GetWindow(CurrentScreenType)?.Show();
            }

            OnCurrentScreenChangedEvent.Invoke(CurrentScreenType);
        }
    }
}