using Fusion;
using Main.Scripts.FusionHelpers;
using Main.Scripts.Gui;
using Main.Scripts.Player;
using UnityEngine;

namespace Main.Scripts
{
    /// <summary>
    /// App entry point and main UI flow management.
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        [SerializeField]
        private GameManager _gameManagerPrefab;

        [SerializeField]
        private PlayerController _playerPrefab;

        private FusionLauncher.ConnectionStatus _status = FusionLauncher.ConnectionStatus.Disconnected;
        private GameMode _gameMode;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            OnConnectionStatusUpdate(null, FusionLauncher.ConnectionStatus.Disconnected, "");
        }

        private void Update()
        {
            // if (_uiProgress.isShowing)
            // {
            //     if (Input.GetKeyUp(KeyCode.Escape))
            //     {
            //         NetworkRunner runner = FindObjectOfType<NetworkRunner>();
            //         if (runner != null && !runner.IsShutdown)
            //         {
            //             // Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
            //             runner.Shutdown(false);
            //         }
            //     }
            //
            //     UpdateUI();
            // }
        }

        public void OnGUI()
        {
            switch (_status)
            {
                case FusionLauncher.ConnectionStatus.Connected:
                case FusionLauncher.ConnectionStatus.Connecting:
                case FusionLauncher.ConnectionStatus.Loaded:
                case FusionLauncher.ConnectionStatus.Loading:
                    return;
                case FusionLauncher.ConnectionStatus.Disconnected:
                case FusionLauncher.ConnectionStatus.Failed:
                    break;
            }

            if (GUI.Button(new Rect(10, 10, Screen.width - 20, Screen.height / 2f - 20), "Server"))
            {
                OnHostOptions();
                OnEnterRoom();
            }

            if (GUI.Button(new Rect(10, Screen.height / 2f + 10, Screen.width - 20, Screen.height / 2 - 20), "Client"))
            {
                OnJoinOptions();
                OnEnterRoom();
            }
        }

        // What mode to play - Called from the start menu
        public void OnHostOptions()
        {
            SetGameMode(GameMode.Host);
        }

        public void OnJoinOptions()
        {
            SetGameMode(GameMode.Client);
        }

        private void SetGameMode(GameMode gamemode)
        {
            _gameMode = gamemode;
        }

        public void OnEnterRoom()
        {
            FusionLauncher launcher = FindObjectOfType<FusionLauncher>();
            if (launcher == null)
                launcher = new GameObject("Launcher").AddComponent<FusionLauncher>();

            LevelManager lm = FindObjectOfType<LevelManager>();
            lm.launcher = launcher;

            launcher.Launch(_gameMode, "roomName", lm, OnConnectionStatusUpdate, OnSpawnWorld, OnSpawnPlayer,
                OnDespawnPlayer);
        }

        /// <summary>
        /// Call this method from button events to close the current UI panel and check the return value to decide
        /// if it's ok to proceed with handling the button events. Prevents double-actions and makes sure UI panels are closed. 
        /// </summary>
        /// <param name="ui">Currently visible UI that should be closed</param>
        /// <returns>True if UI is in fact visible and action should proceed</returns>
        private bool GateUI(Panel ui)
        {
            if (!ui.isShowing) return false;

            ui.SetVisible(false);
            return true;
        }

        private void OnConnectionStatusUpdate(NetworkRunner runner, FusionLauncher.ConnectionStatus status,
            string reason)
        {
            if (!this)
                return;

            Debug.Log(status);

            if (status != _status)
            {
                switch (status)
                {
                    case FusionLauncher.ConnectionStatus.Disconnected:
                        // ErrorBox.Show("Disconnected!", reason, () => { });
                        break;
                    case FusionLauncher.ConnectionStatus.Failed:
                        // ErrorBox.Show("Error!", reason, () => { });
                        break;
                }
            }

            _status = status;
            UpdateUI();
        }

        private void OnSpawnWorld(NetworkRunner runner)
        {
            Debug.Log("Spawning GameManager");
            runner.Spawn(_gameManagerPrefab, Vector3.zero, Quaternion.identity, null, InitNetworkState);

            void InitNetworkState(NetworkRunner runner, NetworkObject world)
            {
                world.transform.parent = transform;
            }
        }

        private void OnSpawnPlayer(NetworkRunner runner, PlayerRef playerref)
        {
            if (GameManager.playState != GameManager.PlayState.LOBBY)
            {
                Debug.Log("Not Spawning Player - game has already started");
                return;
            }

            Debug.Log($"Spawning tank for player {playerref}");
            runner.Spawn(_playerPrefab, Vector3.up, Quaternion.identity, playerref, InitNetworkState);

            void InitNetworkState(NetworkRunner runner, NetworkObject networkObject)
            {
                PlayerController player = networkObject.gameObject.GetComponent<PlayerController>();
                Debug.Log($"Initializing player {player.playerID}");
                player.InitNetworkState();
            }
        }

        private void OnDespawnPlayer(NetworkRunner runner, PlayerRef playerref)
        {
            Debug.Log($"Despawning Player {playerref}");
            PlayerController player = PlayerManager.Get(playerref);
            player.TriggerDespawn();
        }

        private void UpdateUI()
        {
            bool intro = false;
            bool progress = false;
            bool running = false;

            // switch (_status)
            // {
            //     case FusionLauncher.ConnectionStatus.Disconnected:
            //         _progress.text = "Disconnected!";
            //         intro = true;
            //         break;
            //     case FusionLauncher.ConnectionStatus.Failed:
            //         _progress.text = "Failed!";
            //         intro = true;
            //         break;
            //     case FusionLauncher.ConnectionStatus.Connecting:
            //         _progress.text = "Connecting";
            //         progress = true;
            //         break;
            //     case FusionLauncher.ConnectionStatus.Connected:
            //         _progress.text = "Connected";
            //         progress = true;
            //         break;
            //     case FusionLauncher.ConnectionStatus.Loading:
            //         _progress.text = "Loading";
            //         progress = true;
            //         break;
            //     case FusionLauncher.ConnectionStatus.Loaded:
            //         running = true;
            //         break;
            // }

            // _uiCurtain.SetVisible(!running);
            // _uiStart.SetVisible(intro);
            // _uiProgress.SetVisible(progress);
            // _uiGame.SetActive(running);
        }
    }
}