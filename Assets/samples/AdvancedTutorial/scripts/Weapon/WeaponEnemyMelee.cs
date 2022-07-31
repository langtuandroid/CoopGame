using System.Collections.Generic;
using Bolt.samples.AdvancedTutorial.scripts.Actions;
using UnityEngine;
using Photon.Bolt;
using Photon.Bolt.LagCompensation;
using Random = UnityEngine.Random;

namespace Bolt.AdvancedTutorial
{
	public class WeaponEnemyMelee : WeaponBase
	{
		private float attackSphereRadius = 1.3f;
		[SerializeField]
		private int takeDamageDelayFrames = 28;

		public override void OnOwner (Command cmd, BoltEntity entity)
		{
			if (entity.IsOwner)
			{
				StartCoroutine(AttackDelayed(cmd, entity));
			}
		}

		private IEnumerator<WaitForEndOfFrame> AttackDelayed(Command cmd, BoltEntity entity)
		{
			while (fireFrame + takeDamageDelayFrames > BoltNetwork.ServerFrame)
			{
				yield return new WaitForEndOfFrame();
			}

			var spherePosition = entity.transform.position + entity.transform.forward * attackSphereRadius;
			var hitsUnder = BoltNetwork.OverlapSphereAll(spherePosition, attackSphereRadius, cmd.ServerFrame);
			var hitsForward = BoltNetwork.OverlapSphereAll(spherePosition + entity.transform.up, attackSphereRadius, cmd.ServerFrame);
			var hitsUpper = BoltNetwork.OverlapSphereAll(spherePosition + entity.transform.up * 2, attackSphereRadius, cmd.ServerFrame);

			var color = new Color(Random.value, Random.value, Random.value, 0.1f);
			// DebugHelper.DrawSphere(spherePosition, attackSphereRadius, color);
			// DebugHelper.DrawSphere(spherePosition + entity.transform.up, attackSphereRadius, color);
			// DebugHelper.DrawSphere(spherePosition + entity.transform.up * 2, attackSphereRadius, color);

			var hitsMap = new Dictionary <BoltHitboxBody, BoltPhysicsHit>();
			for (var i = 0; i < hitsUnder.count; ++i)
			{
				var hit = hitsUnder.GetHit(i);
				hitsMap[hit.body] = hit;
			}
			for (var i = 0; i < hitsForward.count; ++i)
			{
				var hit = hitsForward.GetHit(i);
				hitsMap[hit.body] = hit;
			}
			for (var i = 0; i < hitsUpper.count; ++i)
			{
				var hit = hitsUpper.GetHit(i);
				hitsMap[hit.body] = hit;
			}

			foreach (var hit in hitsMap.Values)
			{
				if (hit.body.GetComponent<BobController>() == null) {
					break;
				}

				var takingDamageObject = hit.body.GetComponent<ObjectWithTakingDamage>();
				takingDamageObject?.ApplyDamage(damagePerBullet);
				var knockableObject = hit.body.GetComponent<ObjectWithGettingKnockBack>();
				knockableObject?.ApplyKnockBack(entity.transform.forward, 1.5f);
				var stunnableObject = hit.body.GetComponent<ObjectWithGettingStun>();
				stunnableObject?.ApplyStun(0.5f);
			}
		}

		public override void Fx (BoltEntity entity)
		{
		}
	}
}
