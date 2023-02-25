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
        private TextField UserIdTextInput = default!;
        private Button createServerButton = default!;
        private Button connectClientButton = default!;

        public EventCallback<ChangeEvent<string>> OnRoomNameChanged = default!;
        public EventCallback<ChangeEvent<string>> OnUserIdChanged = default!;
        public Action OnCreateServerClicked = default!;
        public Action OnConnectClientClicked = default!;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            roomNameTextInput = root.Q<TextField>("RoomNameTextInput");
            UserIdTextInput = root.Q<TextField>("UserIdTextInput");
            createServerButton = root.Q<Button>("CreateServerButton");
            connectClientButton = root.Q<Button>("ConnectClientButton");

            roomNameTextInput.RegisterValueChangedCallback(OnRoomNameChanged);
            UserIdTextInput.RegisterValueChangedCallback(OnUserIdChanged);
            createServerButton.clicked += () => { OnCreateServerClicked(); };
            connectClientButton.clicked += () => { OnConnectClientClicked(); };
        }

        public void Bind(string roomName, string playerName)
        {
            roomNameTextInput.value = roomName;
            UserIdTextInput.value = playerName;
        }
    }
}