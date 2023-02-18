using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.Weapon
{
    public class MeleeAttackActiveSkill : ActiveSkillBase
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

                var origin = originTransform.position;
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
    }
}