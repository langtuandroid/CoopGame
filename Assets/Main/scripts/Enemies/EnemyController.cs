using System.Collections;
using System.Linq;
using Bolt.AdvancedTutorial;
using Bolt.samples.AdvancedTutorial.scripts.Actions;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.AI;

namespace samples.AdvancedTutorial.scripts.Enemies
{
	public class EnemyController : EntityEventListener<IEnemyState>,
		ObjectWithTakingDamage,
		ObjectWithGettingKnockBack,
		ObjectWithGettingStun
	{
		private bool fire;
		private bool isKnocking;
		private bool isStunned;
		private readonly WaitForFixedUpdate WaitForFixed = new WaitForFixedUpdate();

		private NavMeshAgent navMeshAgent;

		[SerializeField]
		WeaponBase[] _weapons;

		private WeaponBase activeWeapon => _weapons[state.weapon];
		private bool isDead => !enabled || state.Dead;

		void Awake()
		{
			navMeshAgent = GetComponent<NavMeshAgent>();
		}

		public override void Attached()
		{
			state.SetTransforms(state.transform, transform);
			state.SetAnimator(GetComponentInChildren<Animator>());

			state.health = 100;

			state.OnFire += OnFire;
		}

		void OnFire()
		{
			// play sfx
			// _weaponSfxSource.PlayOneShot(activeWeapon.fireSound);

			// 
			activeWeapon.Fx(entity);
		}

		public override void SimulateOwner()
		{
			if (isDead)
			{
				return;
			}

			if ((BoltNetwork.Frame % 5) == 0)
			{
				state.health = (byte) Mathf.Clamp(state.health + 1, 0, 100);
			}

			if (canMoveByController() && Player.allPlayers.Any())
			{
				navMeshAgent.isStopped = false;
				navMeshAgent.destination = Player.allPlayers.First().entity.gameObject.transform.position;
			}
			else
			{
				navMeshAgent.isStopped = true;
				navMeshAgent.ResetPath();
			}

			AnimateEnemy();
			state.weapon = 0;

			if (fire)
			{
				FireWeapon();
			}
		}

		private void AnimateEnemy()
		{
			state.MoveZ = 1;
		}

		private void FireWeapon()
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
				state.Dead = true;
				StopAllCoroutines();
				Destroy(gameObject);
			}
		}

		public void ApplyKnockBack(Vector3 direction)
		{
			if (isDead) return;
			StartCoroutine(KnockBackCoroutine(direction));
		}

		private IEnumerator KnockBackCoroutine(Vector3 direction) 
		{
			isKnocking = true;

			var duration = 0.1f;
			var progress = 0f;
			while (progress < duration)
			{
				var deltaTime = BoltNetwork.FrameDeltaTime;
				progress += deltaTime;

				navMeshAgent.Move((deltaTime / duration) * direction);
				yield return WaitForFixed;
			}
            
			isKnocking = false;
		}

		public void ApplyStun(float durationSec)
		{
			if (isDead) return;
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
