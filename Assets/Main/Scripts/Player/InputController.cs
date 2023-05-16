using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Main.Scripts.Enemies;
using Main.Scripts.Player.Interaction;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.UI.Windows;
using Main.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

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
        private EnemyController enemyPrefab = default!;
        [SerializeField]
        private GameObject mineGold = default!;
        
        private UIScreenManager? uiScreenManager;
        private EnemiesManager enemiesManager = default!;

        [Networked]
        private NetworkButtons ButtonsPrevious { get; set; }
        
        public bool fetchInput = true;

        private PlayerController playerController = default!;
        private InteractionController interactionController = default!;
        
        private NetworkInputData frameworkInput;
        private Vector2 moveDelta;
        private Vector2 aimDelta;
        private Vector2 mouseOnMapPosition;
        private bool primaryFire;
        private bool secondaryFire;
        private bool firstSkillPressed;
        private bool dashSkillPressed;
        private bool spawnEnemy;
        private bool spawnMine;
        private bool interact;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            interactionController = GetComponent<InteractionController>();
        }

        /// <summary>
        /// Hook up to the Fusion callbacks so we can handle the input polling
        /// </summary>
        public override void Spawned()
        {
            // Technically, it does not really matter which InputController fills the input structure, since the actual data will only be sent to the one that does have authority,
            // but in the name of clarity, let's make sure we give input control to the gameobject that also has Input authority.
            if (Object.HasInputAuthority)
            {
                Runner.AddCallbacks(this);
                uiScreenManager = UIScreenManager.Instance.ThrowWhenNull();
                uiScreenManager.OnCurrentScreenChangedEvent.AddListener(OnCurrentWindowChanged);
            }
            
            enemiesManager = EnemiesManager.Instance.ThrowWhenNull();

            Debug.Log("Spawned [" + this + "] IsClient=" + Runner.IsClient + " IsServer=" + Runner.IsServer +
                      " HasInputAuth=" + Object.HasInputAuthority + " HasStateAuth=" + Object.HasStateAuthority);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (HasInputAuthority)
            {
                uiScreenManager.ThrowWhenNull();
                uiScreenManager.OnCurrentScreenChangedEvent.RemoveListener(OnCurrentWindowChanged);
            }
        }

        private void OnCurrentWindowChanged(ScreenType screenType)
        {
            fetchInput = screenType == ScreenType.NONE;
        }

        /// <summary>
        /// Get Unity input and store them in a struct for Fusion
        /// </summary>
        /// <param name="runner">The current NetworkRunner</param>
        /// <param name="input">The target input handler that we'll pass our data to</param>
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (playerController != null && playerController.Object != null &&
                playerController.state == PlayerController.State.Active)
            {
                // Fill networked input struct with input data

                frameworkInput.aimDirection = aimDelta.normalized;
                frameworkInput.moveDirection = moveDelta.normalized;
                frameworkInput.mousePosition = mouseOnMapPosition;
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_FIRE_PRIMARY, primaryFire);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_FIRE_SECONDARY, secondaryFire);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_SPAWN_ENEMY, spawnEnemy);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_SPAWN_MINE, spawnMine);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_INTERACT, interact);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_CAST_FIRST_SKILL, firstSkillPressed);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_CAST_DASH_SKILL, dashSkillPressed);
            }

            // Hand over the data to Fusion
            input.Set(frameworkInput);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        private void Update()
        {
            if (!fetchInput)
            {
                primaryFire = false;
                secondaryFire = false;
                firstSkillPressed = false;
                dashSkillPressed = false;
                spawnEnemy = false;
                spawnMine = false;
                interact = false;
                moveDelta = Vector2.zero;
                aimDelta = Vector2.zero;

                return;
            }
            
            primaryFire = Input.GetMouseButton(0);

            secondaryFire = Input.GetMouseButton(1);

            firstSkillPressed = Input.GetKey(KeyCode.Alpha1);

            dashSkillPressed = Input.GetKey(KeyCode.LeftShift);

            spawnEnemy = Input.GetKey(KeyCode.T);

            spawnMine = Input.GetKey(KeyCode.Y);

            interact = Input.GetKey(KeyCode.F);

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
            mouseOnMapPosition = new Vector2(mouseCollisionPoint.x, mouseCollisionPoint.z);
        }

        /// <summary>
        /// FixedUpdateNetwork is the main Fusion simulation callback - this is where
        /// we modify network state.
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            if (GetInput<NetworkInputData>(out var input))
            {
                var pressedButtons = input.Buttons.GetPressed(ButtonsPrevious);
                var releasedButtons = input.Buttons.GetReleased(ButtonsPrevious);
                ButtonsPrevious = input.Buttons;

                playerController.ApplyMapTargetPosition(input.mousePosition);

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_FIRE_PRIMARY))
                {
                    playerController.OnPrimaryButtonClicked();
                }

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_CAST_FIRST_SKILL))
                {
                    playerController.ActivateSkill(ActiveSkillType.SecondarySkill);
                }

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_CAST_DASH_SKILL))
                {
                    playerController.ActivateSkill(ActiveSkillType.Dash);
                }

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_INTERACT))
                {
                    interactionController.TryInteract();
                }

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_SPAWN_ENEMY))
                {
                    enemiesManager.SpawnEnemy(transform.position);
                }

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_SPAWN_MINE))
                {
                    Runner.Spawn(mineGold, new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)));
                }

                playerController.SetDirections(input.moveDirection.normalized, input.aimDirection.normalized);
            }

            playerController.ApplyDirections();
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
        public const int BUTTON_FIRE_PRIMARY = 0;
        public const int BUTTON_FIRE_SECONDARY = 1;
        public const int BUTTON_SPAWN_ENEMY = 2;
        public const int BUTTON_SPAWN_MINE = 3;
        public const int BUTTON_INTERACT = 4;
        public const int BUTTON_CAST_FIRST_SKILL = 5;
        public const int BUTTON_CAST_DASH_SKILL = 6;

        public NetworkButtons Buttons;
        public Vector2 aimDirection;
        public Vector2 moveDirection;
        public Vector2 mousePosition;
    }
}