using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.MainMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuView : MonoBehaviour
    {
        private UIDocument doc = default!;
        private TextField roomNameTextInput = default!;
        private TextField playerNameTextInput = default!;
        private Button createServerButton = default!;
        private Button connectClientButton = default!;

        public EventCallback<ChangeEvent<string>> OnRoomNameChanged = default!;
        public EventCallback<ChangeEvent<string>> OnPlayerNameChanged = default!;
        public Action OnCreateServerClicked = default!;
        public Action OnConnectClientClicked = default!;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            roomNameTextInput = root.Q<TextField>("RoomNameTextInput");
            playerNameTextInput = root.Q<TextField>("PlayerNameTextInput");
            createServerButton = root.Q<Button>("CreateServerButton");
            connectClientButton = root.Q<Button>("ConnectClientButton");

            roomNameTextInput.RegisterValueChangedCallback(OnRoomNameChanged);
            playerNameTextInput.RegisterValueChangedCallback(OnPlayerNameChanged);
            createServerButton.clicked += () => { OnCreateServerClicked(); };
            connectClientButton.clicked += () => { OnConnectClientClicked(); };
        }

        public void Bind(string roomName, string playerName)
        {
            roomNameTextInput.value = roomName;
            playerNameTextInput.value = playerName;
        }
    }
}