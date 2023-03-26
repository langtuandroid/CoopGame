using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Health;
using Main.Scripts.Effects;
using UnityEngine;

namespace Main.Scripts.Skills.ActiveSkills
{
    public class MeleeAttackActiveSkill : ActiveSkillBase
    {
        private const float ANGLE_STEP = 20f;
        private const float DISTANCE_STEP_MULTIPLIER = 3f;

        [SerializeField]
        private uint attackDamage = 30;
        [SerializeField]
        private EffectsCombination? effectsCombination;
        [SerializeField]
        private float stunDurationSec = 1f;
        [SerializeField]
        private Transform originTransform = default!;
        [SerializeField]
        private float attackDistance = 4f;
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
                OnSkillExecutedEvent.Invoke(this);
                return true;
            }

            return false;
        }

        public override bool IsOverrideMove()
        {
            return false;
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
                        origin: origin,
                        direction: raycastDirection,
                        length: attackDistance,
                        player: owner,
                        hits: hits,
                        layerMask: layerMask,
                        options: HitOptions.IgnoreInputAuthority & HitOptions.SubtickAccuracy
                    );
                    foreach (var hit in hits)
                    {
                        hitObjects.Add(hit.GameObject);
                    }
                }

                foreach (var hitObject in hitObjects)
                {
                    var takingDamageObject = hitObject.GetComponent<Damageable>();
                    takingDamageObject?.ApplyDamage(attackDamage);
                    var knockableObject = hitObject.GetComponent<ObjectWithGettingKnockBack>();
                    knockableObject?.ApplyKnockBack(directionForward);
                    var stunnableObject = hitObject.GetComponent<ObjectWithGettingStun>();
                    stunnableObject?.ApplyStun(stunDurationSec);
                    
                    if (effectsCombination != null)
                    {
                        hitObject.GetComponent<Affectable>()?.ApplyEffects(effectsCombination);
                    }
                }
            }
            
            if (attackTimer.Expired(Runner))
            {
                OnSkillFinishedEvent.Invoke(this);
                attackTimer = default;
            }
        }
    }
}