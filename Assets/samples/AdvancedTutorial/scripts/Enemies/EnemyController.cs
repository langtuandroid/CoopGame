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

			state.health = 100;

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
			if (state.Dead)
			{
				return;
			}

			if ((BoltNetwork.Frame % 5) == 0)
			{
				state.health = (byte) Mathf.Clamp(state.health + 1, 0, 100);
			}

			var targetPosition = transform.position;
			if (canMoveByController() && Player.allPlayers.Any())
			{
				targetPosition = Player.allPlayers.First().entity.gameObject.transform.position;
			}

			var movingDir = (targetPosition - transform.position).normalized;

			_motor.MoveTo(movingDir, movingDir);

			AnimateEnemy();
			state.weapon = weapon;

			if (fire)
			{
				FireWeapon();
			}
		}

		void AnimateEnemy()
		{
			state.MoveZ = 1;

			// JUMP
			state.IsGrounded = _motor.isGrounded;
		}

		void FireWeapon()
		{
			if (state.IsGrounded && activeWeapon.fireFrame + activeWeapon.refireRate <= BoltNetwork.ServerFrame)
			{
				activeWeapon.fireFrame = BoltNetwork.ServerFrame;

				state.Fire();

				// if we are the owner and the active weapon is a hitscan weapon, do logic
				if (entity.IsOwner)
				{
					activeWeapon.OnOwner(null, entity);
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

			var duration = 0.15f;
			var progress = 0f;
			var direction = (targetPosition - fromPosition).normalized;
			while (progress < duration)
			{
				progress += BoltNetwork.FrameDeltaTime;

				_motor.MoveTo(direction, direction, 5f);
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
