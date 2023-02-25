using Main.Scripts.Player.Data;
using Main.Scripts.Room;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.MainMenu
{
    [RequireComponent(typeof(MainMenuView))]
    public class MainMenuPresenter : MonoBehaviour
    {
        private ConnectionManager connectionManager = default!;
        private MainMenuView view = default!;

        private string roomName = default!;
        private string userId = default!;

        private void Awake()
        {
            connectionManager = FindObjectOfType<ConnectionManager>().ThrowWhenNull();
            view = GetComponent<MainMenuView>();
            view.OnRoomNameChanged = OnRoomNameChanged;
            view.OnUserIdChanged = OnUserIdChanged;
            view.OnCreateServerClicked = OnCreateServerClicked;
            view.OnConnectClientClicked = OnConnectClientClicked;
        }

        private void Start()
        {
            view.Bind(connectionManager.RoomName, connectionManager.CurrentUserId.Id.Value);
        }

        private void OnRoomNameChanged(ChangeEvent<string> evt)
        {
            roomName = evt.newValue;
        }

        private void OnUserIdChanged(ChangeEvent<string> evt)
        {
            userId = evt.newValue;
        }

        private void OnCreateServerClicked()
        {
            connectionManager.CreateServer(roomName, new UserId(userId));
        }

        private void OnConnectClientClicked()
        {
            connectionManager.ConnectClient(roomName, new UserId(userId));
        }
    }
}