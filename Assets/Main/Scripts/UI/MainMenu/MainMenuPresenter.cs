using Main.Scripts.Room;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.MainMenu
{
    [RequireComponent(typeof(MainMenuView))]
    public class MainMenuPresenter : MonoBehaviour
    {
        private ConnectionManager connectionManager = default!;
        private MainMenuView view = default!;

        private void Awake()
        {
            connectionManager = FindObjectOfType<ConnectionManager>();
            view = GetComponent<MainMenuView>();
            view.OnRoomNameChanged = OnRoomNameChanged;
            view.OnPlayerNameChanged = OnPlayerNameChanged;
            view.OnCreateServerClicked = OnCreateServerClicked;
            view.OnConnectClientClicked = OnConnectClientClicked;
        }

        private void Start()
        {
            view.Bind(connectionManager.RoomName, connectionManager.PlayerName);
        }

        private void OnRoomNameChanged(ChangeEvent<string> evt)
        {
            connectionManager.SetRoomName(evt.newValue);
        }

        private void OnPlayerNameChanged(ChangeEvent<string> evt)
        {
            connectionManager.SetPlayerName(evt.newValue);
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