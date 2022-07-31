using System.Collections;
using System.Linq;
using Bolt.samples.AdvancedTutorial.scripts.Actions;
using UnityEngine;
using Photon.Bolt;
using Random = UnityEngine.Random;

namespace Bolt.AdvancedTutorial
{
	public class EnemyController : EntityEventListener<IEnemyState>,
		ObjectWithTakingDamage,
		ObjectWithGettingKnockBack,
		ObjectWithGettingStun
	{
		bool fire;

		int weapon;

		private volatile bool isKnocking;
		private volatile bool isStunned;
		static readonly WaitForFixedUpdate WaitForFixed = new WaitForFixedUpdate();

		EnemyMotor _motor;

		[SerializeField]
		WeaponBase[] _weapons;

		public WeaponBase activeWeapon
		{
			get { return _weapons[state.weapon]; }
		}

		void Awake()
		{
			_motor = GetComponent<EnemyMotor>();
		}

		void Update()
		{
			
			if (entity.IsOwner && entity.HasControl && Input.GetKey(KeyCode.L))
			{
				for (int i = 0; i < 100; ++i)
				{
					BoltNetwork.Instantiate(BoltPrefabs.SceneCube, new Vector3(Random.value * 512, Random.value * 512, Random.value * 512), Quaternion.identity);
				}
			}
		}

		public override void Attached()
		{
			state.SetTransforms(state.transform, transform);
			state.SetAnimator(GetComponentInChildren<Animator>());

			state.OnFire += OnFire;
			state.AddCallback("weapon", WeaponChanged);

			// setup weapon
			WeaponChanged();
		}

		void WeaponChanged()
		{
			// setup weapon
			for (int i = 0; i < _weapons.Length; ++i)
			{
				_weapons[i].gameObject.SetActive(false);
			}

			_weapons[state.weapon].gameObject.SetActive(true);
		}

		void OnFire()
		{
			// play sfx
			// _weaponSfxSource.PlayOneShot(activeWeapon.fireSound);

			GameUI.instance.crosshair.Spread += 0.1f;

			// 
			activeWeapon.Fx(entity);
		}

		public override void SimulateOwner()
		{
			if ((BoltNetwork.Frame % 5) == 0 && (state.Dead == false))
			{
				state.health = (byte)Mathf.Clamp(state.health + 1, 0, 100);
			}
		}

		public override void SimulateController()
		{
			var targetPosition = transform.position;
			if (canMoveByController() && Player.allPlayers.Any())
			{
				targetPosition = Player.allPlayers.First().entity.gameObject.transform.position;
			}

			var movingDir = (targetPosition - transform.position).normalized;
			
			IEnemyCommandInput input = EnemyCommand.Create();
			
			input.fire = fire;

			input.movingDir = movingDir;

			input.weapon = weapon;

			entity.QueueInput(input);
		}

		public override void ExecuteCommand(Command c, bool resetState)
		{
			if (state.Dead)
			{
				return;
			}

			EnemyCommand cmd = (EnemyCommand)c;

			if (resetState)
			{
				_motor.SetState(cmd.Result.position, cmd.Result.velocity, cmd.Result.isGrounded);
			}
			else
			{
				// move and save the resulting state
				var result = _motor.MoveTo(cmd.Input.movingDir);

				cmd.Result.position = result.position;
				cmd.Result.velocity = result.velocity;
				cmd.Result.isGrounded = result.isGrounded;

				if (cmd.IsFirstExecution)
				{
					// animation
					AnimateEnemy(cmd);
					state.weapon = cmd.Input.weapon;

					// deal with weapons
					if (cmd.Input.fire)
					{
						FireWeapon(cmd);
					}
				}
			}
		}

		void AnimateEnemy(Command cmd)
		{
			state.MoveZ = 1;

			// JUMP
			state.IsGrounded = _motor.isGrounded;
		}

		void FireWeapon(Command cmd)
		{
			if (state.IsGrounded && activeWeapon.fireFrame + activeWeapon.refireRate <= BoltNetwork.ServerFrame)
			{
				activeWeapon.fireFrame = BoltNetwork.ServerFrame;

				state.Fire();

				// if we are the owner and the active weapon is a hitscan weapon, do logic
				if (entity.IsOwner)
				{
					activeWeapon.OnOwner(cmd, entity);
				}
			}
		}

		private bool isAttacking()
		{
			return activeWeapon.fireFrame + activeWeapon.refireRate > BoltNetwork.ServerFrame;
		}
		
		public void ApplyDamage(int damage)
		{
			state.health -= damage;

			if (state.health > 100 || state.health < 0)
			{
				state.health = 0;
			}

			if (state.health == 0)
			{
				Destroy(gameObject);
			}
		}
		
		public void ApplyKnockBack(Vector3 direction, float force)
		{
			var targetKnockBackPosition = entity.transform.position + direction.normalized * force;
			StartCoroutine(KnockBackCoroutine(entity.transform.position, targetKnockBackPosition));
		}

		private IEnumerator KnockBackCoroutine(Vector3 fromPosition, Vector3 targetPosition)
		{
			isKnocking = true;

			var knockingDuration = 0.15f;
			var knockingProgress = 0f;
			while (knockingProgress < knockingDuration)
			{
				knockingProgress += BoltNetwork.FrameDeltaTime;
				entity.transform.position = Vector3.Lerp(fromPosition, targetPosition,
					knockingProgress / (float) knockingDuration);
				yield return WaitForFixed;
			}
            
			isKnocking = false;
		}

		public void ApplyStun(float durationSec)
		{
			StartCoroutine(StunCoroutine(durationSec));
		}

		private IEnumerator StunCoroutine(float durationSec)
		{
			isStunned = true;
			yield return new WaitForSeconds(durationSec);
			isStunned = false;
		}
		
		private bool canMoveByController()
		{
			return !isKnocking && !isStunned;
		}
	}
}
