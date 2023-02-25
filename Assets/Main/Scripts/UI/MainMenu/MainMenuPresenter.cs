using System;
using Main.Scripts.Room;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.MainMenu
{
    public class MainMenuPresenter : MonoBehaviour
    {
        [SerializeField]
        private ConnectionManager connectionManager;
        private MainMenuView view;

        private void Awake()
        {
            view = GetComponent<MainMenuView>();
            view.OnRoomNameChanged = OnRoomNameChanged;
            view.OnCreateServerClicked = OnCreateServerClicked;
            view.OnConnectClientClicked = OnConnectClientClicked;
        }

        private void Start()
        {
            view.Bind(connectionManager.RoomName);
        }

        private void OnRoomNameChanged(ChangeEvent<string> evt)
        {
            connectionManager.SetRoomName(evt.newValue);
        }

        private void OnCreateServerClicked()
        {
            connectionManager.CreateServer();
        }

        private void OnConnectClientClicked()
        {
            connectionManager.ConnectClient();
        }
    }
}