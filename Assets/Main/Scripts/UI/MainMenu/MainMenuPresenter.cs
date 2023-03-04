using Fusion;
using Main.Scripts.Connection;
using Main.Scripts.Player.Data;
using Main.Scripts.Room.Transition;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.MainMenu
{
    [RequireComponent(typeof(MainMenuView))]
    public class MainMenuPresenter : MonoBehaviour
    {
        [SerializeField]
        private SessionManager sessionManagerPrefab = default!;
        private SessionManager? sessionManager;
        private LevelTransitionManager levelTransitionManager = default!;
        private MainMenuView view = default!;

        private string roomName = "Room";
        private string userId = "Player";

        private void Awake()
        {
            levelTransitionManager = FindObjectOfType<LevelTransitionManager>().ThrowWhenNull();
            view = GetComponent<MainMenuView>();
            view.OnRoomNameChanged = OnRoomNameChanged;
            view.OnUserIdChanged = OnUserIdChanged;
            view.OnCreateServerClicked = OnCreateServerClicked;
            view.OnConnectClientClicked = OnConnectClientClicked;
        }

        private void Start()
        {
            view.Bind(roomName, userId);
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
            OnEnterRoom(GameMode.Host);
        }

        private void OnConnectClientClicked()
        {
            OnEnterRoom(GameMode.Client);
        }

        private void OnEnterRoom(GameMode gameMode)
        {
            if (sessionManager == null)
            {
                sessionManager = Instantiate(sessionManagerPrefab);
            }

            sessionManager.LaunchSession(
                mode: gameMode,
                room: roomName,
                userId: new UserId(userId),
                sceneManager: levelTransitionManager
            );
        }
    }
}