using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.MainMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuWindow : MonoBehaviour
    {
        private UIDocument doc = default!;
        private TextField roomNameTextInput = default!;
        private TextField UserIdTextInput = default!;
        private Button createServerButton = default!;
        private Button connectClientButton = default!;
        private Button profileSettingsButton = default!;

        public EventCallback<ChangeEvent<string>>? OnRoomNameChanged;
        public EventCallback<ChangeEvent<string>>? OnUserIdChanged;
        public Action? OnCreateServerClicked;
        public Action? OnConnectClientClicked;
        public Action? OnProfileSettingsClicked;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            roomNameTextInput = root.Q<TextField>("RoomNameTextInput");
            UserIdTextInput = root.Q<TextField>("UserIdTextInput");
            createServerButton = root.Q<Button>("CreateServerButton");
            connectClientButton = root.Q<Button>("ConnectClientButton");
            profileSettingsButton = root.Q<Button>("ProfileSettingsButton");

            roomNameTextInput.RegisterValueChangedCallback(changeEvent => { OnRoomNameChanged?.Invoke(changeEvent); });
            UserIdTextInput.RegisterValueChangedCallback(changeEvent => { OnUserIdChanged?.Invoke(changeEvent); });
            createServerButton.clicked += () => { OnCreateServerClicked?.Invoke(); };
            connectClientButton.clicked += () => { OnConnectClientClicked?.Invoke(); };
            profileSettingsButton.clicked += () => { OnProfileSettingsClicked?.Invoke(); };
        }

        public void SetVisibility(bool isVisible)
        {
            doc.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Bind(string roomName, string playerName)
        {
            roomNameTextInput.value = roomName;
            UserIdTextInput.value = playerName;
        }
    }
}