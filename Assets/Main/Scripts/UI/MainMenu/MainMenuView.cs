using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        private UIDocument doc;
        private TextField roomNameTextInput;
        private Button createServerButton;
        private Button connectClientButton;

        public EventCallback<ChangeEvent<string>> OnRoomNameChanged;
        public Action OnCreateServerClicked;
        public Action OnConnectClientClicked;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            roomNameTextInput = root.Q<TextField>("RoomNameTextInput");
            createServerButton = root.Q<Button>("CreateServerButton");
            connectClientButton = root.Q<Button>("ConnectClientButton");

            roomNameTextInput.RegisterValueChangedCallback(OnRoomNameChanged);
            createServerButton.clicked += () => { OnCreateServerClicked(); };
            connectClientButton.clicked += () => { OnConnectClientClicked(); };
        }

        public void Bind(string roomName)
        {
            roomNameTextInput.value = roomName;
        }
    }
}