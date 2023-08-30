using Fusion;
using Main.Scripts.Connection;
using Main.Scripts.Player.Data;
using Main.Scripts.Room.Transition;
using Main.Scripts.UI.MainMenu.ProfileSettings;
using Main.Scripts.UI.MainMenu.Status;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private SessionManager sessionManagerPrefab = default!;
        [SerializeField]
        private MainMenuWindow mainMenuWindow = default!;
        [SerializeField]
        private ProfileSettingsWindow profileSettingsWindow = default!;
        [SerializeField]
        private ConnectionStatusWindow connectionStatusWindow = default!;
        [SerializeField]
        private LevelTransitionManager levelTransitionManagerPrefab = default!;

        private SessionManager? sessionManager;

        private string roomName = "Room";
        private string userId = "Player";

        private void Start()
        {
            sessionManager = SessionManager.Instance;

            mainMenuWindow.OnRoomNameChanged = OnRoomNameChanged;
            mainMenuWindow.OnUserIdChanged = OnUserIdChanged;
            mainMenuWindow.OnCreateServerClicked = OnCreateServerClicked;
            mainMenuWindow.OnConnectClientClicked = OnConnectClientClicked;
            mainMenuWindow.OnProfileSettingsClicked = OnProfileSettingsClicked;

            profileSettingsWindow.OnClosedEvent = OnProfileSettingsClosed;

            connectionStatusWindow.SetVisibility(false);
            connectionStatusWindow.OnReturnButtonClicked = OnReturnButtonClicked;

            mainMenuWindow.Bind(roomName, userId);

            if (sessionManager != null)
            {
                OnConnectionStatusChanged(sessionManager.CurrentConnectionStatus);
                sessionManager.Release();
                sessionManager = null;
            }
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
            OnEnterRoom(GameMode.Shared);
        }

        private void OnConnectClientClicked()
        {
            OnEnterRoom(GameMode.Shared);
        }

        private void OnEnterRoom(GameMode gameMode)
        {
            if (sessionManager == null)
            {
                sessionManager = Instantiate(sessionManagerPrefab);
                sessionManager.OnConnectionStatusChangedEvent.AddListener(OnConnectionStatusChanged);
            }

            sessionManager.LaunchSession(
                mode: gameMode,
                room: roomName,
                userId: new UserId(userId),
                sceneManager: Instantiate(levelTransitionManagerPrefab)
            );
        }

        private void OnProfileSettingsClicked()
        {
            mainMenuWindow.SetVisibility(false);
            profileSettingsWindow.Show();
        }

        private void OnProfileSettingsClosed()
        {
            mainMenuWindow.SetVisibility(true);
        }

        private void OnConnectionStatusChanged(ConnectionStatus status)
        {
            mainMenuWindow.SetVisibility(false);
            connectionStatusWindow.SetVisibility(true);
            connectionStatusWindow.SetStatus(status);
        }

        private void OnReturnButtonClicked()
        {
            connectionStatusWindow.SetVisibility(false);
            mainMenuWindow.SetVisibility(true);
        }
    }
}