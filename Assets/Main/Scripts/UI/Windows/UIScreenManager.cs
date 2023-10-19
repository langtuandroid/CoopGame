using System;
using System.Linq;
using Fusion;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.UI.Windows
{
    public class UIScreenManager : MonoBehaviour
    {
        public static UIScreenManager? Instance { get; private set; }
        
        private UIScreensHolder uiScreensHolder = null!;

        public ScreenType CurrentScreenType { get; private set; }
        public UnityEvent<ScreenType> OnCurrentScreenChangedEvent = default!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
            
            CurrentScreenType = ScreenType.NONE;
        }

        private void Start()
        {
            uiScreensHolder = UIScreensHolder.Instance.ThrowWhenNull();
        }

        private void OnDestroy()
        {
            OnCurrentScreenChangedEvent.RemoveAllListeners();
            Instance = null;
            uiScreensHolder = null!;
        }

        private void Update()
        {
            var playerDataManager = PlayerDataManager.Instance;
            var hasHeroData = playerDataManager != null && playerDataManager.HasHeroData(playerDataManager.Runner.LocalPlayer);


            if (CurrentScreenType != ScreenType.NONE)
            {
                if (Input.GetKeyDown(KeyCode.Escape) && hasHeroData)
                {
                    SetScreenType(ScreenType.NONE);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.K) && hasHeroData)
            {
                SetScreenType(ScreenType.SKILL_TREE);
                return;
            }

            if (Input.GetKeyDown(KeyCode.H) && hasHeroData)
            {
                SetScreenType(ScreenType.CUSTOMIZATION);
                return;
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                SetScreenType(ScreenType.HERO_PICKER);
                return;
            }

            if (Input.GetKeyDown(KeyCode.P) && hasHeroData)
            {
                SetScreenType(ScreenType.DEBUG);
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
                uiScreensHolder.GetWindow(screenType)?.Close();
            }

            if (CurrentScreenType != ScreenType.NONE)
            {
                uiScreensHolder.GetWindow(CurrentScreenType)?.Open();
            }

            OnCurrentScreenChangedEvent.Invoke(CurrentScreenType);
        }
    }
}