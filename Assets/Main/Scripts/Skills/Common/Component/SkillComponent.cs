using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Actions.Data;
using Main.Scripts.Actions.Health;
using Main.Scripts.Core.CustomPhysics;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
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
    public class SkillComponent : GameLoopEntity
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

        private List<GameObject> affectedTargets = new();

        private SkillFollowStrategyBase followStrategy = default!;
        private SkillActionTriggerBase actionTrigger = default!;
        private List<SkillFindTargetsStrategyBase> findTargetStrategiesList = new();
        private List<SkillActionBase> actionsList = new();

        private HashSet<GameObject> findTargetHitObjectsSet = new();

        private List<SpawnSkillActionBase> spawnActions = new();
        private RaycastHit[] raycasts = new RaycastHit[100];
        private Collider[] colliders = new Collider[100];
        private Vector3 skillPositionOnCollisionTriggered;
        private bool isCollisionTriggered;
        private bool shouldDespawn;

        private Action<SkillComponent>? onSpawnNewSkillComponent;

        private bool isFinished => destroyAfterFinishTimer.IsRunning;
        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.SkillSpawnPhase,
            GameLoopPhase.SkillUpdatePhase,
            GameLoopPhase.DespawnPhase,
            GameLoopPhase.PhysicsSkillMovementPhase,
            GameLoopPhase.PhysicsCheckCollisionsPhase,
            GameLoopPhase.PhysicsSkillLookPhase,
            GameLoopPhase.ObjectsSpawnPhase,
        };


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
            skillPositionOnCollisionTriggered = default;

            this.initialMapPoint = initialMapPoint;
            this.dynamicMapPoint = dynamicMapPoint;

            this.selfUnit = selfUnit;
            this.selectedUnit = selectedUnit;

            this.alliesLayerMask = alliesLayerMask;
            this.opponentsLayerMask = opponentsLayerMask;

            this.onSpawnNewSkillComponent = onSpawnNewSkillComponent;
            
            affectedTargets.Clear();
            findTargetHitObjectsSet.Clear();
        }

        public override void Spawned()
        {
            base.Spawned();
            spawnActions.Clear();
            shouldDespawn = false;
            isCollisionTriggered = false;
            
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

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.SkillSpawnPhase:
                    OnSpawnPhase();
                    break;
                case GameLoopPhase.SkillUpdatePhase:
                    OnSkillUpdatePhase();
                    break;
                case GameLoopPhase.DespawnPhase:
                    OnDespawnPhase();
                    break;
                case GameLoopPhase.PhysicsSkillMovementPhase:
                    OnPhysicsSkillMovementPhase();
                    break;
                case GameLoopPhase.PhysicsCheckCollisionsPhase:
                    OnPhysicsCheckCollisionsPhase();
                    break;
                case GameLoopPhase.PhysicsSkillLookPhase:
                    OnPhysicsSkillsLookPhase();
                    break;
                case GameLoopPhase.ObjectsSpawnPhase:
                    //todo спавнить обычные объекты, но учесть очередь деспавна
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
        }

        private void OnPhysicsSkillMovementPhase()
        {
            if (shouldStop || shouldDespawn) return;
            
            UpdatePosition();
        }

        private void OnSkillUpdatePhase()
        {
            if (!HasStateAuthority || shouldDespawn) return;

            if (isCollisionTriggered)
            {
                ExecuteActions();
                isCollisionTriggered = false;
            }
            
            if (isFinished)
            {
                if (destroyAfterFinishTimer.Expired(Runner))
                {
                    shouldDespawn = true;
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
                    //todo вынести в Render проверку стейта
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
                    shouldDespawn = true;
                }
                return;
            }

            if (triggerTimer.Expired(Runner))
            {
                triggerTimer = default;
                ExecuteActions();
            }

            CheckPeriodicTrigger();
        }

        private void OnPhysicsCheckCollisionsPhase()
        {
            if (shouldStop || shouldDespawn) return;
            
            CheckCollisionTrigger();
        }

        private void OnPhysicsSkillsLookPhase()
        {
            if (shouldStop || shouldDespawn) return;
            
            UpdateRotation();
        }

        private void OnDespawnPhase()
        {
            if (shouldDespawn && spawnActions.Count == 0)
            {
                Runner.Despawn(Object);
            }
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
                var moveDelta = deltaPosition.normalized * moveToTargetStrategy.Speed * PhysicsManager.DeltaTime;

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
                transform.position += direction * moveToDirectionStrategy.Speed * PhysicsManager.DeltaTime;
            }
        }

        private void UpdateRotation()
        {
            transform.rotation = Quaternion.LookRotation(GetDirectionByType(skillConfig.FollowDirectionType));
        }

        private IEnumerable<GameObject> FindActionTargets()
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
                            findTargetHitObjectsSet.Add(selectedUnit.gameObject);
                        }

                        break;

                    case SelfUnitSkillFindTargetsStrategy:
                        if (selfUnit != null)
                        {
                            findTargetHitObjectsSet.Add(selfUnit.gameObject);
                        }

                        break;
                }
            }

            if (skillConfig.IsAffectTargetsOnlyOneTime)
            {
                foreach (var affectedTarget in affectedTargets)
                {
                    findTargetHitObjectsSet.Remove(affectedTarget.gameObject);
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
                && TickHelper.CheckFrequency(Runner.Tick - startSkillTick, Runner.Simulation.Config.TickRate, periodicTrigger.Frequency))
            {
                ExecuteActions();
            }
        }

        private void CheckCollisionTrigger()
        {
            if (!HasStateAuthority) return;
            
            if (actionTrigger is CollisionSkillActionTrigger collisionTrigger)
            {
                var hitsCount = Physics.OverlapSphereNonAlloc(
                    position: transform.position,
                    radius: collisionTrigger.Radius,
                    results: colliders,
                    layerMask: GetLayerMaskByType(collisionTrigger.TargetType)
                );

                if (!isCollisionTriggered && (hitsCount >= 2 || (hitsCount > 0 && colliders[0].gameObject != gameObject)))
                {
                    skillPositionOnCollisionTriggered = transform.position;
                    isCollisionTriggered = true;
                }
            }
        }

        private void ExecuteActions()
        {
            //todo вынести в Render проверку стейта
            OnActionEvent.Invoke();

            activatedTriggersCount++;
            var actionTargets = FindActionTargets();
            foreach (var action in actionsList)
            {
                ApplyAction(action, actionTargets);
            }
        }

        private void ApplyAction(SkillActionBase action, IEnumerable<GameObject> actionTargets)
        {
            foreach (var actionTarget in actionTargets)
            {
                switch (action)
                {
                    case ApplyEffectsSkillAction applyEffectsAction
                        when actionTarget.TryGetInterface<Affectable>(out var affectable):

                        foreach (var effectsCombination in applyEffectsAction.EffectsCombinations)
                        {
                            affectable.AddEffects(effectsCombination);
                        }

                        break;
                    case DamageSkillAction damageAction
                        when actionTarget.TryGetInterface<Damageable>(out var damageable):
                        var damageActionData = new DamageActionData
                        {
                            damageOwner = selfUnit,
                            damageValue = damageAction.DamageValue
                        };
                        damageable.AddDamage(ref damageActionData);

                        break;
                    case ForceSkillAction forceAction
                        when actionTarget.TryGetInterface(out ObjectWithGettingKnockBack knockable):

                        var skillPosition =
                            isCollisionTriggered ? skillPositionOnCollisionTriggered : transform.position;
                        var direction = actionTarget.transform.position - skillPosition;
                        var knockBackActionData = new KnockBackActionData { direction = direction.normalized * forceAction.ForceValue };
                        knockable.AddKnockBack(ref knockBackActionData);

                        break;
                    case HealSkillAction healAction
                        when actionTarget.TryGetInterface<Healable>(out var healable):
                        var healActionData = new HealActionData
                        {
                            healOwner = selfUnit,
                            healValue = healAction.HealValue
                        };
                        healable.AddHeal(ref healActionData);

                        break;
                    case DashSkillAction dashAction
                        when actionTarget.TryGetInterface<Dashable>(out var dashable):

                        var directionByType = GetDirectionByType(dashAction.DirectionType);
                        var dashActionData = new DashActionData
                        {
                            direction = directionByType,
                            speed = dashAction.Speed,
                            durationSec = dashAction.DurationSec
                        };
                        dashable.AddDash(ref dashActionData);

                        break;
                    case StunSkillAction stunAction
                        when actionTarget.TryGetInterface(out ObjectWithGettingStun stunnable):
                        var stunActionData = new StunActionData
                        {
                            durationSec = stunAction.DurationSec
                        };
                        stunnable.AddStun(ref stunActionData);

                        break;
                }
            }

            if (action is SpawnSkillActionBase spawnAction)
            {
                spawnActions.Add(spawnAction);
            }

            if (action is StopSkillAction stopAction && activatedTriggersCount >= stopAction.LiveUntilTriggersCount)
            {
                shouldStop = true;
            }
        }

        private void OnSpawnPhase()
        {
            foreach (var spawnAction in spawnActions)
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
            
            spawnActions.Clear();
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
                        ? selfUnit.GetInterface<Movable>()?.GetMovingDirection() ?? Vector3.zero
                        : transform.forward;

                case SkillDirectionType.SelectedUnitLookDirection:
                    return selectedUnit != null ? selectedUnit.transform.forward : transform.forward;

                case SkillDirectionType.SelectedUnitMoveDirection:
                    return selectedUnit != null
                        ? selectedUnit.GetInterface<Movable>()?.GetMovingDirection() ?? Vector3.zero
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
            ISet<GameObject> targetsSet
        )
        {
            var origin = GetPointByType(strategyConfig.OriginPoint);

            var hitsCount = Physics.OverlapSphereNonAlloc(
                position: origin,
                radius: strategyConfig.Radius,
                results: colliders,
                layerMask: GetLayerMaskByType(strategyConfig.TargetType),
                queryTriggerInteraction: QueryTriggerInteraction.Ignore
            );

            for (var i = 0; i < hitsCount; i++)
            {
                var hit = colliders[i];
                if (hit.gameObject == gameObject) continue;

                targetsSet.Add(hit.gameObject);
            }
        }

        private void FindTargetsCircleSector(
            CircleSectorSkillFindTargetsStrategy strategyConfig,
            ISet<GameObject> targetsSet
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

                var hitsCount = Physics.RaycastNonAlloc(
                    origin: origin,
                    direction: raycastDirection,
                    results: raycasts,
                    maxDistance: strategyConfig.Radius,
                    layerMask: GetLayerMaskByType(strategyConfig.TargetType),
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore
                );

                for (var j = 0; j < hitsCount; j++)
                {
                    var hit = raycasts[j].collider;
                    if (hit.gameObject == gameObject) continue;

                    targetsSet.Add(hit.gameObject);
                }
            }
        }

        private void FindTargetsRectangle(
            RectangleSkillFindTargetsStrategy strategyConfig,
            ISet<GameObject> targetsSet
        )
        {
            var origin = GetPointByType(strategyConfig.OriginPoint);
            var direction = GetDirectionByType(strategyConfig.DirectionType);
            direction = Quaternion.AngleAxis(strategyConfig.DirectionAngleOffset, Vector3.up) * direction;
            
            var originForwardOffset = strategyConfig.OriginForwardOffset;

            var center = origin + direction * (originForwardOffset + strategyConfig.Length / 2f);

            var extents = new Vector3(strategyConfig.Width / 2f, 1f, strategyConfig.Length / 2f);

            var hitsCount = Physics.OverlapBoxNonAlloc(
                center: center,
                halfExtents: extents,
                results: colliders,
                orientation: Quaternion.LookRotation(direction),
                mask: GetLayerMaskByType(strategyConfig.TargetType),
                queryTriggerInteraction: QueryTriggerInteraction.Ignore
            );

            for (var i = 0; i < hitsCount; i++)
            {
                var hit = colliders[i];
                if (hit.gameObject == gameObject) continue;

                targetsSet.Add(hit.gameObject);
            }
        }
    }
}