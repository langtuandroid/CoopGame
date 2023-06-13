using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Health;
using Main.Scripts.Core.Resources;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component.Config;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.Follow;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using Main.Scripts.Skills.Common.Component.Config.Trigger;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Skills.Common.Component
{
    public class SkillComponent : NetworkBehaviour
    {
        private SkillConfigsBank skillConfigsBank = default!;
        private ModifierIdsBank modifierIdsBank = default!;

        private SkillConfig skillConfig = default!;
        private PlayerData? playerData;

        [Networked]
        private int skillConfigId { get; set; }
        [Networked]
        private PlayerRef ownerId { get; set; }

        [Networked]
        private int startSkillTick { get; set; }
        [Networked]
        private TickTimer lifeTimer { get; set; }
        [Networked]
        private TickTimer destroyAfterFinishTimer { get; set; }
        [Networked]
        private TickTimer triggerTimer { get; set; }
        [Networked]
        private int activatedTriggersCount { get; set; }
        [Networked]
        private bool shouldStop { get; set; }

        [Networked]
        private Vector3 initialMapPoint { get; set; }
        [Networked]
        private Vector3 dynamicMapPoint { get; set; }

        [Networked]
        private NetworkObject? selfUnit { get; set; }
        [Networked]
        private NetworkObject? selectedUnit { get; set; }

        [Networked]
        private int alliesLayerMask { get; set; }
        [Networked]
        private int opponentsLayerMask { get; set; }

        [Networked]
        [Capacity(20)]
        private NetworkLinkedList<NetworkObject> affectedTargets => default;

        private SkillFollowStrategyBase followStrategy = default!;
        private SkillActionTriggerBase actionTrigger = default!;
        private List<SkillFindTargetsStrategyBase> findTargetStrategiesList = new();
        private List<SkillActionBase> actionsList = new();

        private List<LagCompensatedHit> findTargetHitsList = new();
        private HashSet<NetworkObject> findTargetHitObjectsSet = new();

        private Action<SkillComponent>? onSpawnNewSkillComponent;

        private bool isFinished => destroyAfterFinishTimer.IsRunning;

        public UnityEvent OnFinishEvent = new();
        public UnityEvent OnActionEvent = new();

        public void Init(
            int skillConfigId,
            PlayerRef ownerId,
            Vector3 initialMapPoint,
            Vector3 dynamicMapPoint,
            NetworkObject? selfUnit,
            NetworkObject? selectedUnit,
            LayerMask alliesLayerMask,
            LayerMask opponentsLayerMask,
            Action<SkillComponent>? onSpawnNewSkillComponent
        )
        {
            this.skillConfigId = skillConfigId;
            this.ownerId = ownerId;
            lifeTimer = default;
            destroyAfterFinishTimer = default;
            triggerTimer = default;
            activatedTriggersCount = 0;
            shouldStop = false;
            startSkillTick = 0;

            this.initialMapPoint = initialMapPoint;
            this.dynamicMapPoint = dynamicMapPoint;

            this.selfUnit = selfUnit;
            this.selectedUnit = selectedUnit;

            this.alliesLayerMask = alliesLayerMask;
            this.opponentsLayerMask = opponentsLayerMask;

            this.onSpawnNewSkillComponent = onSpawnNewSkillComponent;
            
            affectedTargets.Clear();

            
            findTargetHitsList.Clear();
            findTargetHitObjectsSet.Clear();
        }

        public override void Spawned()
        {
            var resources = GlobalResources.Instance.ThrowWhenNull();
            skillConfigsBank = resources.SkillConfigsBank;
            modifierIdsBank = resources.ModifierIdsBank;
            
            playerData = PlayerDataManager.Instance.ThrowWhenNull().GetPlayerData(ownerId);
            skillConfig = skillConfigsBank.GetSkillConfig(skillConfigId);

            SkillFollowStrategyConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref playerData,
                skillConfig.FollowStrategy,
                out followStrategy
            );
            
            SkillActionTriggerConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref playerData,
                skillConfig.ActionTrigger,
                out actionTrigger
            );
            
            findTargetStrategiesList.Clear();
            SkillFindTargetsStrategiesConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref playerData,
                skillConfig.FindTargetsStrategies,
                findTargetStrategiesList
            );
            
            actionsList.Clear();
            SkillActionConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref playerData,
                skillConfig.Actions,
                actionsList
            );

        }

        public override void FixedUpdateNetwork()
        {
            UpdatePosition();
            UpdateRotation();
            
            if (isFinished)
            {
                if (destroyAfterFinishTimer.Expired(Runner))
                {
                    Runner.Despawn(Object);
                }
                return;
            }

            if (!lifeTimer.IsRunning)
            {
                startSkillTick = Runner.Tick;
                lifeTimer = TickTimer.CreateFromSeconds(Runner, skillConfig.DurationSec);

                if (actionTrigger is TimerSkillActionTrigger timerTrigger)
                {
                    triggerTimer = TickTimer.CreateFromSeconds(Runner, timerTrigger.DelaySec);
                }

                if (actionTrigger is StartSkillActionTrigger)
                {
                    ExecuteActions();
                }
            }

            if (lifeTimer.Expired(Runner) || shouldStop)
            {
                lifeTimer = default;
                var shouldDontDestroyIfNeed = !shouldStop || skillConfig.DontDestroyAfterStopAction;
                if (shouldDontDestroyIfNeed)
                {
                    OnFinishEvent.Invoke();
                }

                if (actionTrigger is FinishSkillActionTrigger)
                {
                    ExecuteActions();
                }

                if (shouldDontDestroyIfNeed && skillConfig.DontDestroyAfterFinishDurationSec > 0f)
                {
                    //откладываем дестрой после истечения жизни скилла, либо после экшена стоп при наличии соответствующего флага
                    destroyAfterFinishTimer =
                        TickTimer.CreateFromSeconds(Runner, skillConfig.DontDestroyAfterFinishDurationSec);
                }
                else
                {
                    Runner.Despawn(Object);
                }
                return;
            }

            if (triggerTimer.Expired(Runner))
            {
                triggerTimer = default;
                ExecuteActions();
            }

            CheckPeriodicTrigger();
            CheckCollisionTrigger();
        }

        public void UpdateMapPoint(Vector3 mapPoint)
        {
            if (isFinished) return;
            
            dynamicMapPoint = mapPoint;
        }

        public void OnClickTrigger()
        {
            if (isFinished) return;
            
            if (actionTrigger is ClickSkillActionTrigger)
            {
                ExecuteActions();
            }
        }

        public void TryInterrupt()
        {
            if (isFinished) return;
            
            //todo
        }

        public void OnLostControl()
        {
            onSpawnNewSkillComponent = null;
        }

        private void UpdatePosition()
        {
            if (followStrategy is AttachToTargetSkillFollowStrategy attachStrategy)
            {
                transform.position = GetPointByType(attachStrategy.AttachTo);
            }

            if (followStrategy is MoveToTargetSkillFollowStrategy moveToTargetStrategy)
            {
                var targetPoint = GetPointByType(moveToTargetStrategy.MoveTo);
                var deltaPosition = targetPoint - transform.position;
                var moveDelta = deltaPosition.normalized * moveToTargetStrategy.Speed * Runner.DeltaTime;

                if (deltaPosition.sqrMagnitude <= moveDelta.sqrMagnitude)
                {
                    transform.position = targetPoint;
                }
                else
                {
                    transform.position += moveDelta;
                }
            }

            if (followStrategy is MoveToDirectionSkillFollowStrategy moveToDirectionStrategy)
            {
                var direction = GetDirectionByType(moveToDirectionStrategy.MoveDirectionType);
                direction = Quaternion.AngleAxis(moveToDirectionStrategy.DirectionAngleOffset, Vector3.up) * direction;
                transform.position += direction * moveToDirectionStrategy.Speed * Runner.DeltaTime;
            }
        }

        private void UpdateRotation()
        {
            transform.rotation = Quaternion.LookRotation(GetDirectionByType(skillConfig.FollowDirectionType));
        }

        private IEnumerable<NetworkObject> FindActionTargets()
        {
            findTargetHitObjectsSet.Clear();
            foreach (var findTargetStrategy in findTargetStrategiesList)
            {
                switch (findTargetStrategy)
                {
                    case AroundPointSkillFindTargetsStrategy aroundPointStrategy:
                        FindTargetsAroundPoint(aroundPointStrategy, findTargetHitObjectsSet);
                        break;

                    case CircleSectorSkillFindTargetsStrategy circleSectorStrategy:
                        FindTargetsCircleSector(circleSectorStrategy, findTargetHitObjectsSet);
                        break;

                    case RectangleSkillFindTargetsStrategy rectangleStrategy:
                        FindTargetsRectangle(rectangleStrategy, findTargetHitObjectsSet);
                        break;

                    case SelectedUnitSkillFindTargetsStrategy:
                        if (selectedUnit != null)
                        {
                            findTargetHitObjectsSet.Add(selectedUnit);
                        }

                        break;

                    case SelfUnitSkillFindTargetsStrategy:
                        if (selfUnit != null)
                        {
                            findTargetHitObjectsSet.Add(selfUnit);
                        }

                        break;
                }
            }

            if (skillConfig.IsAffectTargetsOnlyOneTime)
            {
                foreach (var affectedTarget in affectedTargets)
                {
                    findTargetHitObjectsSet.Remove(affectedTarget);
                }

                foreach (var hitTarget in findTargetHitObjectsSet)
                {
                    affectedTargets.Add(hitTarget);
                }
            }

            return findTargetHitObjectsSet;
        }

        private void CheckPeriodicTrigger()
        {
            if (actionTrigger is PeriodicSkillActionTrigger periodicTrigger
                && TickHelper.CheckFrequency(startSkillTick + Runner.Tick, Runner.Simulation.Config.TickRate, periodicTrigger.Frequency))
            {
                ExecuteActions();
            }
        }

        private void CheckCollisionTrigger()
        {
            if (actionTrigger is CollisionSkillActionTrigger collisionTrigger)
            {
                Runner.LagCompensation.OverlapSphere(
                    origin: transform.position,
                    radius: collisionTrigger.Radius,
                    player: ownerId,
                    hits: findTargetHitsList,
                    layerMask: GetLayerMaskByType(collisionTrigger.TargetType),
                    options: HitOptions.IgnoreInputAuthority & HitOptions.SubtickAccuracy
                );

                if (findTargetHitsList.Count > 0)
                {
                    ExecuteActions();
                }
            }
        }

        private void ExecuteActions()
        {
            OnActionEvent.Invoke();

            activatedTriggersCount++;
            var actionTargets = FindActionTargets();
            foreach (var action in actionsList)
            {
                ApplyAction(action, actionTargets);
            }
        }

        private void ApplyAction(SkillActionBase action, IEnumerable<NetworkObject> actionTargets)
        {
            foreach (var actionTarget in actionTargets)
            {
                switch (action)
                {
                    case ApplyEffectsSkillAction applyEffectsAction
                        when actionTarget.TryGetComponent<Affectable>(out var affectable):

                        foreach (var effectsCombination in applyEffectsAction.EffectsCombinations)
                        {
                            affectable.ApplyEffects(effectsCombination);
                        }

                        break;
                    case DamageSkillAction damageAction
                        when actionTarget.TryGetComponent<Damageable>(out var damageable):

                        damageable.ApplyDamage(damageAction.DamageValue, selfUnit);

                        break;
                    case ForceSkillAction forceAction
                        when actionTarget.TryGetComponent(out ObjectWithGettingKnockBack knockable):

                        var direction = actionTarget.transform.position - transform.position;
                        knockable.ApplyKnockBack(direction.normalized * forceAction.ForceValue);

                        break;
                    case HealSkillAction healAction
                        when actionTarget.TryGetComponent<Healable>(out var healable):

                        healable.ApplyHeal(healAction.HealValue, selfUnit);

                        break;
                    case DashSkillAction dashAction
                        when actionTarget.TryGetComponent<Dashable>(out var dashable):

                        dashable.Dash(
                            direction: GetDirectionByType(dashAction.DirectionType),
                            speed: dashAction.Speed,
                            durationSec: dashAction.DurationSec
                        );

                        break;
                    case StunSkillAction stunAction
                        when actionTarget.TryGetComponent(out ObjectWithGettingStun stunnable):

                        stunnable.ApplyStun(stunAction.DurationSec);

                        break;
                }
            }

            if (action is SpawnSkillActionBase spawnAction)
            {
                var position = GetPointByType(spawnAction.SpawnPointType);
                var rotation = Quaternion.LookRotation(GetDirectionByType(spawnAction.SpawnDirectionType));

                switch (spawnAction)
                {
                    case SpawnPrefabSkillAction spawnPrefabSkillAction:
                        Runner.Spawn(
                            prefab: spawnPrefabSkillAction.PrefabToSpawn,
                            position: position,
                            rotation: rotation
                        );
                        break;
                    case SpawnConfigSkillAction spawnPrefabSkillAction:
                        Runner.Spawn(
                            prefab: spawnPrefabSkillAction.SkillConfig.Prefab,
                            position: position,
                            rotation: rotation,
                            onBeforeSpawned: (runner, spawnedObject) =>
                            {
                                if (spawnedObject.TryGetComponent<SkillComponent>(out var skillComponent))
                                {
                                    skillComponent.Init(
                                        skillConfigId: skillConfigsBank.GetSkillConfigId(spawnPrefabSkillAction.SkillConfig),
                                        ownerId: ownerId,
                                        initialMapPoint: initialMapPoint,
                                        dynamicMapPoint: dynamicMapPoint,
                                        selfUnit: Object,
                                        selectedUnit: selectedUnit,
                                        alliesLayerMask: alliesLayerMask,
                                        opponentsLayerMask: opponentsLayerMask,
                                        onSpawnNewSkillComponent: onSpawnNewSkillComponent
                                    );
                                    onSpawnNewSkillComponent?.Invoke(skillComponent);
                                }
                            }
                        );
                        break;
                }

            }

            if (action is StopSkillAction stopAction && activatedTriggersCount >= stopAction.LiveUntilTriggersCount)
            {
                shouldStop = true;
            }
        }

        private Vector3 GetPointByType(SkillPointType pointType)
        {
            switch (pointType)
            {
                case SkillPointType.SkillPosition:
                    return transform.position;

                case SkillPointType.SelfUnitTarget:
                    return selfUnit != null ? selfUnit.transform.position : transform.position;

                case SkillPointType.SelectedUnitTarget:
                    return selectedUnit != null ? selectedUnit.transform.position : transform.position;

                case SkillPointType.InitialMapPointTarget:
                    return initialMapPoint;

                case SkillPointType.DynamicMapPointTarget:
                    return dynamicMapPoint;

                default:
                    throw new ArgumentOutOfRangeException(nameof(pointType), pointType, null);
            }
        }

        private Vector3 GetDirectionByType(SkillDirectionType directionType)
        {
            switch (directionType)
            {
                case SkillDirectionType.SkillDirection:
                    return transform.forward;

                case SkillDirectionType.ToSelfUnit:
                    return selfUnit != null
                        ? (selfUnit.transform.position - transform.position).normalized
                        : transform.forward;

                case SkillDirectionType.ToSelectedUnit:
                    return selectedUnit != null
                        ? (selectedUnit.transform.position - transform.position).normalized
                        : transform.forward;

                case SkillDirectionType.ToInitialMapPoint:
                    return (initialMapPoint - transform.position).normalized;

                case SkillDirectionType.ToDynamicMapPoint:
                    return (dynamicMapPoint - transform.position).normalized;

                case SkillDirectionType.SelfUnitLookDirection:
                    return selfUnit != null ? selfUnit.transform.forward : transform.forward;

                case SkillDirectionType.SelfUnitMoveDirection:
                    return selfUnit != null
                        ? selfUnit.GetComponent<Movable>()?.GetMovingDirection() ?? Vector3.zero
                        : transform.forward;

                case SkillDirectionType.SelectedUnitLookDirection:
                    return selectedUnit != null ? selectedUnit.transform.forward : transform.forward;

                case SkillDirectionType.SelectedUnitMoveDirection:
                    return selectedUnit != null
                        ? selectedUnit.GetComponent<Movable>()?.GetMovingDirection() ?? Vector3.zero
                        : transform.forward;

                default:
                    throw new ArgumentOutOfRangeException(nameof(directionType), directionType, null);
            }
        }

        private LayerMask GetLayerMaskByType(UnitTargetType targetType)
        {
            return targetType switch
            {
                UnitTargetType.Allies => alliesLayerMask,
                UnitTargetType.Opponents => opponentsLayerMask,
                UnitTargetType.All => alliesLayerMask | opponentsLayerMask,
                _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
            };
        }

        private void FindTargetsAroundPoint(
            AroundPointSkillFindTargetsStrategy strategyConfig,
            ISet<NetworkObject> targetsSet
        )
        {
            var origin = GetPointByType(strategyConfig.OriginPoint);

            Runner.LagCompensation.OverlapSphere(
                origin: origin,
                radius: strategyConfig.Radius,
                player: ownerId,
                hits: findTargetHitsList,
                layerMask: GetLayerMaskByType(strategyConfig.TargetType),
                options: HitOptions.IgnoreInputAuthority & HitOptions.SubtickAccuracy
            );

            foreach (var hit in findTargetHitsList)
            {
                targetsSet.Add(hit.Hitbox.Root.Object);
            }
        }

        private void FindTargetsCircleSector(
            CircleSectorSkillFindTargetsStrategy strategyConfig,
            ISet<NetworkObject> targetsSet
        )
        {
            var ANGLE_STEP = 20f;
            var DISTANCE_STEP_MULTIPLIER = 3f;

            var origin = GetPointByType(strategyConfig.OriginPoint);

            var direction = GetDirectionByType(strategyConfig.DirectionType);
            direction = Quaternion.AngleAxis(strategyConfig.DirectionAngleOffset, Vector3.up) * direction;

            var raycastsCount = Mathf.CeilToInt(strategyConfig.Angle / ANGLE_STEP) *
                                Mathf.CeilToInt(strategyConfig.Radius / DISTANCE_STEP_MULTIPLIER);

            for (var i = 0; i <= raycastsCount; i++)
            {
                var rotateAngle = -strategyConfig.Angle / 2f + i * strategyConfig.Angle / raycastsCount;
                var raycastDirection = Quaternion.AngleAxis(rotateAngle, Vector3.up) * direction;

                Runner.LagCompensation.RaycastAll(
                    origin: origin,
                    direction: raycastDirection,
                    length: strategyConfig.Radius,
                    player: ownerId,
                    hits: findTargetHitsList,
                    layerMask: GetLayerMaskByType(strategyConfig.TargetType),
                    options: HitOptions.IgnoreInputAuthority & HitOptions.SubtickAccuracy
                );

                foreach (var hit in findTargetHitsList)
                {
                    targetsSet.Add(hit.Hitbox.Root.Object);
                }
            }
        }

        private void FindTargetsRectangle(
            RectangleSkillFindTargetsStrategy strategyConfig,
            ISet<NetworkObject> targetsSet
        )
        {
            var origin = GetPointByType(strategyConfig.OriginPoint);
            var direction = GetDirectionByType(strategyConfig.DirectionType);
            direction = Quaternion.AngleAxis(strategyConfig.DirectionAngleOffset, Vector3.up) * direction;
            
            var originForwardOffset = strategyConfig.OriginForwardOffset;

            var center = origin + direction * (originForwardOffset + strategyConfig.Length / 2f);

            var extents = new Vector3(strategyConfig.Width / 2f, 1f, strategyConfig.Length / 2f);

            Runner.LagCompensation.OverlapBox(
                center: center,
                extents: extents,
                orientation: Quaternion.LookRotation(direction),
                player: ownerId,
                hits: findTargetHitsList,
                layerMask: GetLayerMaskByType(strategyConfig.TargetType),
                options: HitOptions.IgnoreInputAuthority & HitOptions.SubtickAccuracy
            );

            foreach (var hit in findTargetHitsList)
            {
                targetsSet.Add(hit.Hitbox.Root.Object);
            }
        }
    }
}