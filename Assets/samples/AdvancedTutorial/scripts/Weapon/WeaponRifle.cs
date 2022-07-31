using Bolt.samples.AdvancedTutorial.scripts.Actions;
using UnityEngine;
using Bolt.Samples.AdvancedTutorial.scripts.Enemies;
using Photon.Bolt;

namespace Bolt.AdvancedTutorial
{
	public class WeaponRifle : WeaponBase
	{
		private float attackSphereRadius = 2f;

		public override void OnOwner (Command cmd, BoltEntity entity)
		{
			if (entity.IsOwner) {
				IPlayerState state = entity.GetState<IPlayerState> ();
				PlayerController controller = entity.GetComponent<PlayerController> ();

				var spherePosition = entity.transform.position + entity.transform.forward * attackSphereRadius + entity.transform.up * attackSphereRadius;
				
				using (var hits = BoltNetwork.OverlapSphereAll(spherePosition, attackSphereRadius, cmd.ServerFrame)) {
					for (int i = 0; i < hits.count; ++i) {
						var hit = hits.GetHit (i);
						
						// var serializer = hit.body.GetComponent<PlayerController>();

						// if ((serializer != null) && (serializer.state.team != state.team)) {
						// 	serializer.ApplyDamage (controller.activeWeapon.damagePerBullet);
						// 	break;
						// }

						var takingDamageObject = hit.body.GetComponent<ObjectWithTakingDamage>();
						takingDamageObject?.ApplyDamage(controller.activeWeapon.damagePerBullet);
					}
				}
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
