using System;
using System.Threading.Tasks;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Component;
using Main.Scripts.Weapon;
using UnityEngine;

namespace Main.Scripts.Player
{
    public class PlayerController : NetworkBehaviour,
        ObjectWithTakingDamage,
        OnAttackCallback
    {
        private static readonly int MOVE_X_ANIM = Animator.StringToHash("MoveX");
        private static readonly int MOVE_Z_ANIM = Animator.StringToHash("MoveZ");
        private static readonly int ATTACK_ANIM = Animator.StringToHash("Attack");

        private NetworkNavMeshAgent navMeshAgent;
        private LevelManager levelManager;
        private Animator animator;


        [SerializeField]
        private SkillManager skillManager;
        [SerializeField]
        private int maxHealth = 100;

        [SerializeField]
        private float speed = 6f;

        [Networked(OnChanged = nameof(OnStateChanged))]
        public State state { get; set; }
        [Networked]
        private int health { get; set; }
        [Networked]
        private Vector2 moveDirection { get; set; }
        [Networked]
        private Vector2 aimDirection { get; set; }
        [Networked]
        private TickTimer respawnTimer { get; set; }

        public bool isActivated => (gameObject.activeInHierarchy && (state == State.Active || state == State.Spawning));
        public bool isRespawningDone => state == State.Spawning && respawnTimer.Expired(Runner);
        public int playerID { get; private set; }
        public SkillManager SkillManager => skillManager;

        private float respawnInSeconds = -1;

        void Awake()
        {
            navMeshAgent = GetComponent<NetworkNavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        public override void Spawned()
        {
            navMeshAgent.NavMeshAgentComponent.updateRotation = false;
            playerID = Object.InputAuthority;

            PlayerManager.AddPlayer(this);
        }

        public void InitNetworkState()
        {
            state = State.New;
            health = maxHealth;
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
            {
                if (respawnInSeconds >= 0)
                {
                    CheckRespawn();
                }

                if (isRespawningDone)
                {
                    ResetPlayer();
                }
            }

            // CheckForPowerupPickup(); //todo pickup items

            AnimatePlayer();
        }

        public void SetDirections(Vector2 moveDirection, Vector2 aimDirection)
        {
            this.moveDirection = moveDirection;
            this.aimDirection = aimDirection;
        }

        public void Move()
        {
            if (!isActivated)
                return;

            transform.LookAt(transform.position + new Vector3(aimDirection.x, 0, aimDirection.y));
            navMeshAgent.Move(speed * new Vector3(moveDirection.x, 0, moveDirection.y));
        }

        public void Respawn(float inSeconds)
        {
            respawnInSeconds = inSeconds;
        }

        private void ResetPlayer()
        {
            state = State.Active;
        }

        private void CheckRespawn()
        {
            if (respawnInSeconds > 0)
            {
                respawnInSeconds -= Runner.DeltaTime;
            }

            var spawnPoint = GetLevelManager().GetPlayerSpawnPoint(playerID);
            if (respawnInSeconds <= 0)
            {
                respawnInSeconds = -1;

                health = maxHealth;

                respawnTimer = TickTimer.CreateFromSeconds(Runner, 1);

                transform.position = spawnPoint;

                if (state != State.Active)
                {
                    state = State.Spawning;
                }
            }
        }

        public void Despawn()
        {
            if (state == State.Dead) return;

            state = State.Despawned;
        }

        public async void TriggerDespawn()
        {
            Despawn();
            PlayerManager.RemovePlayer(this);

            await Task.Delay(300); // wait for effects

            if (Object == null)
            {
                return;
            }

            if (Object.HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
        }

        private LevelManager GetLevelManager()
        {
            if (levelManager == null)
            {
                levelManager = FindObjectOfType<LevelManager>();
            }

            return levelManager;
        }

        public static void OnStateChanged(Changed<PlayerController> changed)
        {
            if (changed.Behaviour)
                changed.Behaviour.OnStateChanged();
        }

        public void OnStateChanged()
        {
            switch (state)
            {
                //todo 
                // case State.Spawning:
                // 	_teleportIn.StartTeleport();
                // 	break;
                // case State.Active:
                // 	_damageVisuals.CleanUpDebris();
                // 	_teleportIn.EndTeleport();
                // 	break;
                // case State.Dead:
                // 	_deathExplosionInstance.transform.position = transform.position;
                // 	_deathExplosionInstance.SetActive(false); // dirty fix to reactivate the death explosion if the particlesystem is still active
                // 	_deathExplosionInstance.SetActive(true);
                //
                // 	_visualParent.gameObject.SetActive(false);
                // 	_damageVisuals.OnDeath();
                // 	break;
                // case State.Despawned:
                // 	_teleportOut.StartTeleport();
                // 	break;
            }
        }

        //todo pickup items
        // public void Pickup(PowerupSpawner powerupSpawner)
        // {
        //     if (!powerupSpawner)
        //         return;
        //
        //     PowerupElement powerup = powerupSpawner.Pickup();
        //
        //     if (powerup == null)
        //         return;
        //
        //     if (powerup.powerupType == PowerupType.HEALTH)
        //         life = MAX_HEALTH;
        //     else
        //         shooter.InstallWeapon(powerup);
        // }
        //
        // private void CheckForPowerupPickup()
        // {
        //     // If we run into a powerup, pick it up
        //     if (isActivated && Runner.GetPhysicsScene().OverlapSphere(transform.position, _pickupRadius, _overlaps, _pickupMask, QueryTriggerInteraction.Collide) > 0)
        //     {
        //         Pickup(_overlaps[0].GetComponent<PowerupSpawner>());
        //     }
        // }

        void AnimatePlayer()
        {
            var moveX = 0f;
            var moveZ = 0f;
            if (moveDirection.sqrMagnitude > 0)
            {
                var moveAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(moveDirection.x, 0, moveDirection.y),
                    Vector3.up);
                var lookAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(aimDirection.x, 0, aimDirection.y),
                    Vector3.up);
                var animationAngle = Mathf.Deg2Rad * (moveAngle - lookAngle);

                moveZ = (float) Math.Cos(animationAngle);
                moveX = (float) Math.Sin(animationAngle);
            }

            Debug.Log($"x={moveX} y={moveZ}");

            animator.SetFloat(MOVE_X_ANIM, moveX);
            animator.SetFloat(MOVE_Z_ANIM, moveZ);
        }

        public void ApplyDamage(int damage)
        {
            if (!isActivated) return;

            if (damage >= health)
            {
                health = 0;
                state = State.Dead;

                GameManager.instance.OnPlayerDeath();
            }
            else
            {
                health -= damage;
                Debug.Log($"Player {playerID} took {damage} damage, health = {health}");
            }
        }

        public void OnAttack()
        {
            animator.SetTrigger(ATTACK_ANIM);
        }

        public enum State
        {
            New,
            Despawned,
            Spawning,
            Active,
            Dead
        }
    }
}