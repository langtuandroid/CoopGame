using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.Weapon
{
    public class MeleeAttackSkill : SkillBase
    {
        private const float ANGLE_STEP = 20f;
        private const float DISTANCE_STEP_MULTIPLIER = 3f;

        [SerializeField]
        private int attackDamage = 30;
        [SerializeField]
        private float stunDurationSec = 1f;
        [SerializeField]
        private Transform originTransform;
        [SerializeField]
        private float attackDistance = 3f;
        [SerializeField]
        private float attackRadiusOffset = 1f;
        [SerializeField]
        private float attackAngle = 90f;
        [SerializeField]
        private LayerMask layerMask = -1;
        [SerializeField]
        private float animationDurationSec = 1f;
        [SerializeField]
        private float attackDelaySec = 0.5f;


        [Networked]
        private TickTimer attackTimer { get; set; }
        [Networked]
        private bool isAttacked { get; set; }
        [Networked]
        private PlayerRef owner { get; set; }


        public override bool Activate(PlayerRef owner)
        {
            if (attackTimer.ExpiredOrNotRunning(Runner))
            {
                attackTimer = TickTimer.CreateFromSeconds(Runner, animationDurationSec);
                isAttacked = false;
                this.owner = owner;
                return true;
            }

            return false;
        }

        public override bool IsRunning()
        {
            return !attackTimer.ExpiredOrNotRunning(Runner);
        }

        public override void FixedUpdateNetwork()
        {
            if (!isAttacked && attackDelaySec < animationDurationSec - attackTimer.RemainingTime(Runner))
            {
                isAttacked = true;
                var hitObjects = new HashSet<GameObject>();
                var hits = new List<LagCompensatedHit>();

                var origin = originTransform.position + Vector3.up;
                var directionForward = originTransform.forward;

                var raycastsCount = Mathf.CeilToInt(attackAngle / ANGLE_STEP) *
                                    Mathf.Ceil(attackDistance / DISTANCE_STEP_MULTIPLIER);

                for (var i = 0; i < raycastsCount; i++)
                {
                    var rotateAngle = -attackAngle / 2f + i * attackAngle / raycastsCount;
                    var raycastDirection = Quaternion.AngleAxis(rotateAngle, Vector3.up) * directionForward;
                    Runner.LagCompensation.RaycastAll(
                        origin: origin + raycastDirection * attackRadiusOffset,
                        direction: raycastDirection,
                        length: attackDistance,
                        player: owner,
                        hits: hits,
                        layerMask: layerMask
                    );
                    foreach (var hit in hits)
                    {
                        hitObjects.Add(hit.GameObject);
                    }
                }

                foreach (var hitObject in hitObjects)
                {
                    var takingDamageObject = hitObject.GetComponent<ObjectWithTakingDamage>();
                    takingDamageObject?.ApplyDamage(attackDamage);
                    var knockableObject = hitObject.GetComponent<ObjectWithGettingKnockBack>();
                    knockableObject?.ApplyKnockBack(directionForward);
                    var stunnableObject = hitObject.GetComponent<ObjectWithGettingStun>();
                    stunnableObject?.ApplyStun(stunDurationSec);
                }
            }
        }

        // private IEnumerator<WaitForEndOfFrame> AttackDelayed(Command cmd, BoltEntity entity)
        // {
        // 	while (fireTime + takeDamageDelayFrames > BoltNetwork.ServerFrame)
        // 	{
        // 		yield return new WaitForEndOfFrame();
        // 	}
        // 	
        // 	IBobState state = entity.GetState<IBobState> ();
        // 	PlayerController controller = entity.GetComponent<PlayerController> ();
        //
        // 	var spherePosition = entity.transform.position + entity.transform.forward * attackSphereRadius;
        // 	var hitsUnder = BoltNetwork.OverlapSphereAll(spherePosition, attackSphereRadius, cmd.ServerFrame);
        // 	var hitsForward = BoltNetwork.OverlapSphereAll(spherePosition + entity.transform.up, attackSphereRadius, cmd.ServerFrame);
        // 	var hitsUpper = BoltNetwork.OverlapSphereAll(spherePosition + entity.transform.up * 2, attackSphereRadius, cmd.ServerFrame);
        //
        // 	var color = new Color(Random.value, Random.value, Random.value, 0.1f);
        // 	// DebugHelper.DrawSphere(spherePosition, attackSphereRadius, color);
        // 	// DebugHelper.DrawSphere(spherePosition + entity.transform.up, attackSphereRadius, color);
        // 	// DebugHelper.DrawSphere(spherePosition + entity.transform.up * 2, attackSphereRadius, color);
        //
        // 	var hitsMap = new Dictionary <BoltHitboxBody, BoltPhysicsHit>();
        // 	for (var i = 0; i < hitsUnder.count; ++i)
        // 	{
        // 		var hit = hitsUnder.GetHit(i);
        // 		hitsMap[hit.body] = hit;
        // 	}
        // 	for (var i = 0; i < hitsForward.count; ++i)
        // 	{
        // 		var hit = hitsForward.GetHit(i);
        // 		hitsMap[hit.body] = hit;
        // 	}
        // 	for (var i = 0; i < hitsUpper.count; ++i)
        // 	{
        // 		var hit = hitsUpper.GetHit(i);
        // 		hitsMap[hit.body] = hit;
        // 	}
        //
        // 	foreach (var hit in hitsMap.Values)
        // 	{
        // 		// var serializer = hit.body.GetComponent<PlayerController>();
        //
        // 		// if ((serializer != null) && (serializer.state.team != state.team)) {
        // 		// 	serializer.ApplyDamage (controller.activeWeapon.damagePerBullet);
        // 		// 	break;
        // 		// }
        // 		
        // 		// if (hit.body.GetComponent<BobController>() == null) {
        // 		// 	break;
        // 		// }
        //
        // 		var takingDamageObject = hit.body.GetComponent<ObjectWithTakingDamage>();
        // 		takingDamageObject?.ApplyDamage(damagePerBullet);
        // 		var knockableObject = hit.body.GetComponent<ObjectWithGettingKnockBack>();
        // 		knockableObject?.ApplyKnockBack(entity.transform.forward);
        // 		var stunnableObject = hit.body.GetComponent<ObjectWithGettingStun>();
        // 		stunnableObject?.ApplyStun(0.5f);
        // 	}
        // }
    }
}