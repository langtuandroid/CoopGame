using System.Collections.Generic;
using Bolt.samples.AdvancedTutorial.scripts.Actions;
using UnityEngine;
using Bolt.Samples.AdvancedTutorial.scripts.Enemies;
using Photon.Bolt;
using Photon.Bolt.LagCompensation;

namespace Bolt.AdvancedTutorial
{
	public class WeaponMelee : WeaponBase
	{
		private float attackSphereRadius = 1.5f;
		[SerializeField]
		private int takeDamageDelayFrames = 28;

		public override void OnOwner (PlayerCommand cmd, BoltEntity entity)
		{
			if (entity.IsOwner)
			{
				StartCoroutine(AttackDelayed(cmd, entity));
			}
		}

		private IEnumerator<WaitForEndOfFrame> AttackDelayed(PlayerCommand cmd, BoltEntity entity)
		{
			while (fireFrame + takeDamageDelayFrames > BoltNetwork.ServerFrame)
			{
				yield return new WaitForEndOfFrame();
			}
			
			IBobState state = entity.GetState<IBobState> ();
			BobController controller = entity.GetComponent<BobController> ();

			var spherePosition = entity.transform.position + entity.transform.forward * attackSphereRadius; 
			var hitsUnder = BoltNetwork.OverlapSphereAll(spherePosition, attackSphereRadius, cmd.ServerFrame);
			var hitsForward = BoltNetwork.OverlapSphereAll(spherePosition + entity.transform.up, attackSphereRadius, cmd.ServerFrame);
			var hitsUpper = BoltNetwork.OverlapSphereAll(spherePosition + entity.transform.up * 2, attackSphereRadius, cmd.ServerFrame);

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
				// var serializer = hit.body.GetComponent<PlayerController>();

				// if ((serializer != null) && (serializer.state.team != state.team)) {
				// 	serializer.ApplyDamage (controller.activeWeapon.damagePerBullet);
				// 	break;
				// }

				var takingDamageObject = hit.body.GetComponent<ObjectWithTakingDamage>();
				takingDamageObject?.DealDamage(controller.activeWeapon.damagePerBullet);
				var knockableObject = hit.body.GetComponent<ObjectWithGettingKnockBack>();
				knockableObject?.ApplyKnockBack(entity.transform.forward, 1.5f);
				var stunnableObject = hit.body.GetComponent<ObjectWithGettingStun>();
				stunnableObject?.ApplyStun(0.5f);
			}
		}

		public override void Fx (BoltEntity entity)
		{
			Vector3 pos;
			Quaternion rot;
			PlayerCamera.instance.CalculateCameraAimTransform (entity.transform, entity.GetState<IPlayerState> ().pitch, out pos, out rot);

			Ray r = new Ray (pos, rot * Vector3.forward);
			RaycastHit rh;

			if (Physics.Raycast (r, out rh) && impactPrefab) {
				var en = rh.transform.GetComponent<BoltEntity> ();
				var hit = GameObject.Instantiate (impactPrefab, rh.point, Quaternion.LookRotation (rh.normal)) as GameObject;

				if (en) {
					hit.GetComponent<RandomSound> ().enabled = false;
				}

				if (trailPrefab) {
					var trailGo = GameObject.Instantiate (trailPrefab, muzzleFlash.position, Quaternion.identity) as GameObject;
					var trail = trailGo.GetComponent<LineRenderer> ();

					trail.SetPosition (0, muzzleFlash.position);
					trail.SetPosition (1, rh.point);
				}
			}

			GameObject go = (GameObject)GameObject.Instantiate (shellPrefab, shellEjector.position, shellEjector.rotation);
			go.GetComponent<Rigidbody> ().AddRelativeForce (0, 0, 2, ForceMode.VelocityChange);
			go.GetComponent<Rigidbody> ().AddTorque (new Vector3 (Random.Range (-32f, +32f), Random.Range (-32f, +32f), Random.Range (-32f, +32f)), ForceMode.VelocityChange);

			// show flash
			muzzleFlash.gameObject.SetActive (true);
		}
	}
}
