using System;
using System.Collections;
using System.Linq;
using Main.Scripts.Actions;
using Main.Scripts.Navigation;
using Main.Scripts.Player;
using Main.Scripts.Weapon;
using Photon.Bolt;
using UnityEngine;

namespace Main.Scripts.Enemies
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

		private AvoidNavMeshAgent avoidNavMeshAgent;

		[SerializeField]
		private WeaponBase[] _weapons;

		[SerializeField]
		private float attackDistance = 3; //todo replace to activeWeapon parameter
		[SerializeField]
		private float knockBackForce = 1.3f;
		[SerializeField]
		private float knockBackDuration = 0.1f;

		private WeaponBase activeWeapon => _weapons[state.weapon];
		private bool isDead => !enabled || state.Dead;

		void Awake()
		{
			avoidNavMeshAgent = GetComponent<AvoidNavMeshAgent>();
		}

		public override void Attached()
		{
			avoidNavMeshAgent.enabled = entity.IsOwner;

			state.SetTransforms(state.transform, transform);
			state.SetAnimator(GetComponentInChildren<Animator>());

			state.health = 100;
			state.weapon = 0;

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

			if (canMoveByController() && PlayerInfo.allPlayers.Any())
			{
				var targetPosition = PlayerInfo.allPlayers.First().entity.gameObject.transform.position;
				if (Vector3.Distance(transform.position, targetPosition) > attackDistance)
				{
					updateDestination(targetPosition);
				}
				else
				{
					updateDestination(null);
					transform.LookAt(targetPosition);
					FireWeapon();
				}
			}
			else
			{
				updateDestination(null);
			}
		}

		private void updateDestination(Vector3? destination)
		{
			avoidNavMeshAgent.SetDestination(destination);
			state.isMoving = destination != null;
		}

		private void FireWeapon()
		{
			if (activeWeapon.fireFrame + activeWeapon.refireRate <= BoltNetwork.ServerFrame)
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

			var progress = 0f;
			while (progress < knockBackDuration)
			{
				var deltaTime = BoltNetwork.FrameDeltaTime;
				progress += deltaTime;

				avoidNavMeshAgent.Move(knockBackForce * (deltaTime / knockBackDuration) * direction);
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
			return !isKnocking && !isStunned && !isAttacking();
		}
	}
}
