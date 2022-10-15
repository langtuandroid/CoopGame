using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Main.Scripts.Weapon;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Main.Scripts.Player
{
    /// <summary>
    /// Handle player input by responding to Fusion input polling, filling an input struct and then working with
    /// that input struct in the Fusion Simulation loop.
    /// </summary>
    public class InputController : NetworkBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField]
        private LayerMask mouseRayMask;
        [SerializeField]
        private GameObject enemyPrefab;

        public static bool fetchInput = true; //todo false on switch scene
        public bool ToggleReady { get; set; }

        private PlayerController playerController;
        private NetworkInputData frameworkInput = new NetworkInputData();
        private Vector2 moveDelta;
        private Vector2 aimDelta;
        private bool primaryFire;
        private bool secondaryFire;
        private bool spawnEnemy;

        /// <summary>
        /// Hook up to the Fusion callbacks so we can handle the input polling
        /// </summary>
        public override void Spawned()
        {
            playerController = GetComponent<PlayerController>();
            // Technically, it does not really matter which InputController fills the input structure, since the actual data will only be sent to the one that does have authority,
            // but in the name of clarity, let's make sure we give input control to the gameobject that also has Input authority.
            if (Object.HasInputAuthority)
            {
                Runner.AddCallbacks(this);
            }

            Debug.Log("Spawned [" + this + "] IsClient=" + Runner.IsClient + " IsServer=" + Runner.IsServer +
                      " HasInputAuth=" + Object.HasInputAuthority + " HasStateAuth=" + Object.HasStateAuthority);
        }

        /// <summary>
        /// Get Unity input and store them in a struct for Fusion
        /// </summary>
        /// <param name="runner">The current NetworkRunner</param>
        /// <param name="input">The target input handler that we'll pass our data to</param>
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (playerController != null && playerController.Object != null &&
                playerController.state == PlayerController.State.Active && fetchInput)
            {
                // Fill networked input struct with input data

                frameworkInput.aimDirection = aimDelta.normalized;

                frameworkInput.moveDirection = moveDelta.normalized;

                if (primaryFire)
                {
                    primaryFire = false;
                    frameworkInput.Buttons |= NetworkInputData.BUTTON_FIRE_PRIMARY;
                }

                if (secondaryFire)
                {
                    secondaryFire = false;
                    frameworkInput.Buttons |= NetworkInputData.BUTTON_FIRE_SECONDARY;
                }

                if (ToggleReady)
                {
                    ToggleReady = false;
                    frameworkInput.Buttons |= NetworkInputData.READY;
                }

                if (spawnEnemy)
                {
                    spawnEnemy = false;
                    frameworkInput.Buttons |= NetworkInputData.BUTTON_SPAWN_ENEMY;
                }
            }

            // Hand over the data to Fusion
            input.Set(frameworkInput);
            frameworkInput.Buttons = 0;
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        private void Update()
        {
            ToggleReady = ToggleReady || Input.GetKeyDown(KeyCode.R);

            if (Input.GetMouseButton(0))
            {
                primaryFire = true;
            }

            if (Input.GetMouseButton(1))
            {
                secondaryFire = true;
            }

            if (Input.GetKey(KeyCode.T))
            {
                spawnEnemy = true;
            }

            moveDelta = Vector2.zero;

            if (Input.GetKey(KeyCode.W))
            {
                moveDelta += Vector2.up;
            }

            if (Input.GetKey(KeyCode.S))
            {
                moveDelta += Vector2.down;
            }

            if (Input.GetKey(KeyCode.A))
            {
                moveDelta += Vector2.left;
            }

            if (Input.GetKey(KeyCode.D))
            {
                moveDelta += Vector2.right;
            }

            Vector3 mousePos = Input.mousePosition;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            Vector3 mouseCollisionPoint = Vector3.zero;
            // Raycast towards the mouse collider box in the world
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mouseRayMask))
            {
                if (hit.collider != null)
                {
                    mouseCollisionPoint = hit.point;
                }
            }

            Vector3 aimDirection = mouseCollisionPoint - playerController.transform.position;
            aimDelta = new Vector2(aimDirection.x, aimDirection.z);
        }

        /// <summary>
        /// FixedUpdateNetwork is the main Fusion simulation callback - this is where
        /// we modify network state.
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            if (GameManager.playState == GameManager.PlayState.TRANSITION)
                return;

            if (GetInput(out NetworkInputData input))
            {
                if (input.IsDown(NetworkInputData.BUTTON_FIRE_PRIMARY))
                {
                    playerController.ActivateSkill(SkillType.PRIMARY);
                }

                if (input.IsDown(NetworkInputData.BUTTON_SPAWN_ENEMY))
                {
                    Runner.Spawn(
                        enemyPrefab,
                        playerController.transform.position + new Vector3(Random.Range(-5, 5) * 5, 0, Random.Range(-5, 5) * 5));
                }

                playerController.SetDirections(input.moveDirection.normalized, input.aimDirection.normalized);
            }

            playerController.Move();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }

    /// <summary>
    /// Our custom definition of an INetworkStruct. Keep in mind that
    /// * bool does not work (C# does not define a consistent size on different platforms)
    /// * Must be a top-level struct (cannot be a nested class)
    /// * Stick to primitive types and structs
    /// * Size is not an issue since only modified data is serialized, but things that change often should be compact (e.g. button states)
    /// </summary>
    public struct NetworkInputData : INetworkInput
    {
        public const uint BUTTON_FIRE_PRIMARY = 1 << 0;
        public const uint BUTTON_FIRE_SECONDARY = 1 << 1;
        public const uint BUTTON_SPAWN_ENEMY = 1 << 2;
        public const uint READY = 1 << 6;

        public uint Buttons;
        public Vector2 aimDirection;
        public Vector2 moveDirection;

        public bool IsUp(uint button)
        {
            return IsDown(button) == false;
        }

        public bool IsDown(uint button)
        {
            return (Buttons & button) == button;
        }
    }
}