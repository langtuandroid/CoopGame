using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Enemies;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Player.Interaction;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.UI.Windows;
using Main.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Main.Scripts.Player.InputSystem
{
    /// <summary>
    /// Handle player input by responding to Fusion input polling, filling an input struct and then working with
    /// that input struct in the Fusion Simulation loop.
    /// </summary>
    public class InputController : GameLoopEntityNetworked, INetworkRunnerCallbacks, IAfterSpawned
    {
        [SerializeField]
        private LayerMask mouseRayMask;
        [SerializeField]
        private GameObject mineGold = default!;
        
        private UIScreenManager? uiScreenManager;
        private FindTargetManager? findTargetSystem;
        private EnemiesManager enemiesManager = default!;

        private NetworkButtons ButtonsPrevious;
        
        public bool fetchInput = true;

        private PlayerController? playerController;
        private InteractionController? interactionController;
        
        private NetworkInputData frameworkInput;
        private Vector2 moveDelta;
        private Vector2 aimDelta;
        private Vector2 mouseOnMapPosition;
        private NetworkObject? unitTarget;
        private bool primaryFire;
        private bool secondaryFire;
        private bool firstSkillPressed;
        private bool secondSkillPressed;
        private bool thirdSkillPressed;
        private bool dashSkillPressed;
        private bool spawnEnemy;
        private bool spawnMine;
        private bool interact;

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.PlayerInputPhase
        };

        /// <summary>
        /// Hook up to the Fusion callbacks so we can handle the input polling
        /// </summary>
        public override void Spawned()
        {
            base.Spawned();
            // Technically, it does not really matter which InputController fills the input structure, since the actual data will only be sent to the one that does have authority,
            // but in the name of clarity, let's make sure we give input control to the gameobject that also has Input authority.

            playerController = GetComponent<PlayerController>();
            interactionController = GetComponent<InteractionController>();
            interactionController.SetOwner(Object.InputAuthority);

            Debug.Log("Spawned [" + this + "] IsClient=" + Runner.IsClient + " IsServer=" + Runner.IsSharedModeMasterClient +
                      " HasInputAuth=" + Object.HasInputAuthority + " HasStateAuth=" + Object.HasStateAuthority);
        }

        public void AfterSpawned()
        {
            if (HasInputAuthority)
            {
                Runner.AddCallbacks(this);
                uiScreenManager = UIScreenManager.Instance.ThrowWhenNull();
                uiScreenManager.OnCurrentScreenChangedEvent.AddListener(OnCurrentWindowChanged);
                
                findTargetSystem = FindTargetManager.Instance.ThrowWhenNull();
            }

            enemiesManager = levelContext.EnemiesManager;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
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
                playerController.GetPlayerState() == PlayerState.Active)
            {
                // Fill networked input struct with input data

                frameworkInput.aimDirection = aimDelta.normalized;
                frameworkInput.moveDirection = moveDelta.normalized;
                frameworkInput.mousePosition = mouseOnMapPosition;
                frameworkInput.unitTargetId = unitTarget != null ? unitTarget.Id : new NetworkId();
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_FIRE_PRIMARY, primaryFire);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_FIRE_SECONDARY, secondaryFire);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_SPAWN_ENEMY, spawnEnemy);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_SPAWN_MINE, spawnMine);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_INTERACT, interact);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_CAST_FIRST_SKILL, firstSkillPressed);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_CAST_SECOND_SKILL, secondSkillPressed);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_CAST_THIRD_SKILL, thirdSkillPressed);
                frameworkInput.Buttons.Set(NetworkInputData.BUTTON_CAST_DASH_SKILL, dashSkillPressed);
            }

            // Hand over the data to Fusion
            input.Set(frameworkInput);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        private void Update()
        {
            if (playerController == null) return;
            
            if (!fetchInput)
            {
                primaryFire = false;
                secondaryFire = false;
                firstSkillPressed = false;
                secondSkillPressed = false;
                thirdSkillPressed = false;
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
            secondSkillPressed = Input.GetKey(KeyCode.Alpha2);
            thirdSkillPressed = Input.GetKey(KeyCode.Alpha3);
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

            var plane = new Plane(transform.up, transform.position);
            CursorUtils.GetCursorWorldPosition(Camera.main, plane, out var mapPoint);

            var aimDirection = mapPoint - playerController.transform.position;
            aimDelta = new Vector2(aimDirection.x, aimDirection.z);
            mouseOnMapPosition = new Vector2(mapPoint.x, mapPoint.z);

            unitTarget = findTargetSystem != null ? findTargetSystem.FocusedTarget : null;
            
            playerController.ApplyMapTargetPosition(mouseOnMapPosition);
            playerController.ApplyUnitTarget(unitTarget);
        }

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            if (playerController == null) return;
            
            if (GetInput<NetworkInputData>(out var input))
            {
                var pressedButtons = input.Buttons.GetPressed(ButtonsPrevious);
                var releasedButtons = input.Buttons.GetReleased(ButtonsPrevious);
                ButtonsPrevious = input.Buttons;

                playerController.ApplyMapTargetPosition(input.mousePosition);
                playerController.ApplyUnitTarget(input.unitTargetId);

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_FIRE_PRIMARY))
                {
                    playerController.OnPrimaryButtonClicked();
                }

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_FIRE_SECONDARY))
                {
                    playerController.OnCancelButtonClicked();
                }

                foreach (var skillButton in NetworkInputData.SKILL_BUTTONS)
                {
                    var skillType = GetSkillByButton(skillButton);
                    if (pressedButtons.IsSet(skillButton))
                    {
                        playerController.SkillBtnPressed(skillType);
                    }

                    if (releasedButtons.IsSet(skillButton))
                    {
                        playerController.SkillBtnReleased(skillType);
                    }

                    if (!pressedButtons.IsSet(skillButton) && input.Buttons.IsSet(skillButton))
                    {
                        playerController.SkillBtnHolding(skillType);
                    }
                }

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_INTERACT) && interactionController != null)
                {
                    interactionController.TryInteract();
                }

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_SPAWN_ENEMY))
                {
                    for (var i = 0; i < 1; i++)
                    {
                        enemiesManager.SpawnEnemy(transform.position);
                    }
                }

                if (pressedButtons.IsSet(NetworkInputData.BUTTON_SPAWN_MINE))
                {
                    Runner.Spawn(mineGold, new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)));
                }

                var moveDirectionNormalized = input.moveDirection.normalized;
                var aimDirectionNormalized = input.aimDirection.normalized;
                playerController.SetDirections(in moveDirectionNormalized, in aimDirectionNormalized);
            }
        }

        private ActiveSkillType GetSkillByButton(int skillButton)
        {
            return skillButton switch
            {
                NetworkInputData.BUTTON_FIRE_PRIMARY => ActiveSkillType.PRIMARY,
                NetworkInputData.BUTTON_FIRE_SECONDARY => ActiveSkillType.NONE,
                NetworkInputData.BUTTON_CAST_DASH_SKILL => ActiveSkillType.DASH,
                NetworkInputData.BUTTON_CAST_FIRST_SKILL => ActiveSkillType.FIRST_SKILL,
                NetworkInputData.BUTTON_CAST_SECOND_SKILL => ActiveSkillType.SECOND_SKILL,
                NetworkInputData.BUTTON_CAST_THIRD_SKILL => ActiveSkillType.THIRD_SKILL,
                _ => throw new ArgumentOutOfRangeException(nameof(skillButton), skillButton, null)
            };
        }
        
        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
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
        public const int BUTTON_CAST_DASH_SKILL = 5;
        public const int BUTTON_CAST_FIRST_SKILL = 6;
        public const int BUTTON_CAST_SECOND_SKILL = 7;
        public const int BUTTON_CAST_THIRD_SKILL = 8;

        public static readonly int[] SKILL_BUTTONS =
        {
            BUTTON_FIRE_PRIMARY,
            BUTTON_FIRE_SECONDARY,
            BUTTON_CAST_DASH_SKILL,
            BUTTON_CAST_FIRST_SKILL,
            BUTTON_CAST_SECOND_SKILL,
            BUTTON_CAST_THIRD_SKILL
        };

        public NetworkButtons Buttons;
        public Vector2 aimDirection;
        public Vector2 moveDirection;
        public Vector2 mousePosition;
        public NetworkId unitTargetId;
    }
}