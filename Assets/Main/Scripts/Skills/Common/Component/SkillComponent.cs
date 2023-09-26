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
using Main.Scripts.Levels;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Data;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component.Config;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.ActionsPack;
using Main.Scripts.Skills.Common.Component.Config.Follow;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using Main.Scripts.Skills.Common.Component.Config.Trigger;
using Main.Scripts.Skills.Common.Component.Visual;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Pool;

namespace Main.Scripts.Skills.Common.Component
{
    public class SkillComponent : GameLoopListener
    {
        private SkillConfigsBank skillConfigsBank = null!;
        private ModifierIdsBank modifierIdsBank = null!;
        private SkillVisualManager skillVisualManager = null!;
        private SkillComponentsPoolHelper skillComponentsPoolHelper = null!;
        private GameLoopManager gameLoopManager = null!;
        
        private SkillConfig skillConfig = null!;
        private PlayerData? playerData;
        
        private NetworkObject objectContext = null!;
        private PlayerRef ownerId;
        private Vector3 initialMapPoint;
        private Vector3 dynamicMapPoint;
        private NetworkObject? selfUnit;
        private NetworkObject? selectedUnit;
        private int alliesLayerMask;
        private int opponentsLayerMask;

        private int startSkillTick;
        private TickTimer lifeTimer;
        private TickTimer triggerTimer;
        private Dictionary<SkillActionsPackData, int> activatedTriggersCountMap = new();
        private bool shouldStop;

        private List<GameObject> affectedTargets = new();

        private SkillFollowStrategyBase followStrategy = null!;
        private List<SkillActionsPackData> actionsPacksDataList = new();

        private HashSet<GameObject> findTargetHitObjectsSet = new();

        private List<SpawnSkillActionBase> spawnActions = new();
        private RaycastHit[] raycasts = new RaycastHit[100];
        private Collider[] colliders = new Collider[100];
        private Vector3 skillPositionOnCollisionTriggered;
        private bool isCollisionTriggered;
        private bool isClickTriggered;
        private int visualToken = -1;

        private Action<SkillComponent>? onSpawnNewSkillComponent;
        public event Action<SkillComponent>? OnReadyToRelease;
        
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

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

        public void Init(
            int skillConfigId,
            NetworkObject objectContext,
            Vector3 position,
            Quaternion rotation,
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
            this.objectContext = objectContext;
            Position = position;
            Rotation = rotation;
            this.ownerId = ownerId;
            this.initialMapPoint = initialMapPoint;
            this.dynamicMapPoint = dynamicMapPoint;
            this.selfUnit = selfUnit;
            this.selectedUnit = selectedUnit;
            this.alliesLayerMask = alliesLayerMask;
            this.opponentsLayerMask = opponentsLayerMask;
            this.onSpawnNewSkillComponent = onSpawnNewSkillComponent;

            var levelContext = LevelContext.Instance.ThrowWhenNull();
            skillComponentsPoolHelper = levelContext.SkillComponentsPoolHelper;
            gameLoopManager = levelContext.GameLoopManager;
            
            var resources = GlobalResources.Instance.ThrowWhenNull();
            skillConfigsBank = resources.SkillConfigsBank;
            modifierIdsBank = resources.ModifierIdsBank;
            skillVisualManager = levelContext.SkillVisualManager;
            
            playerData = PlayerDataManager.Instance.ThrowWhenNull().GetPlayerData(ownerId);
            skillConfig = skillConfigsBank.GetSkillConfig(skillConfigId);

            SkillFollowStrategyConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref playerData,
                skillConfig.FollowStrategy,
                out followStrategy
            );


            foreach (var skillActionsPack in skillConfig.ActionsPacks)
            {
                var resolvedDataOut = GenericPool<SkillActionsPackData>.Get();

                SkillActionsPackResolver.ResolveEnabledModifiers(
                    modifierIdsBank,
                    ref playerData,
                    skillActionsPack,
                    resolvedDataOut
                );
                actionsPacksDataList.Add(resolvedDataOut);

                activatedTriggersCountMap[resolvedDataOut] = 0;
            }
            
            levelContext.GameLoopManager.AddListener(this);
        }

        public void Release()
        {
            if (gameLoopManager != null)
            {
                gameLoopManager.RemoveListener(this);
            }

            foreach (var actionsPackData in actionsPacksDataList)
            {
                GenericPool<SkillActionsPackData>.Release(actionsPackData);
            }
            
            Reset();
        }

        private void Reset()
        {
            skillConfigsBank = null!;
            modifierIdsBank = null!;
            skillVisualManager = null!;
            skillComponentsPoolHelper = null!;
            gameLoopManager = null!;
            
            skillConfig = null!;
            playerData = null;
        
            objectContext = null!;
            selfUnit = null!;
            selectedUnit = null!;
            
            
            lifeTimer = default;
            triggerTimer = default;
            activatedTriggersCountMap.Clear();
            shouldStop = false;
            startSkillTick = 0;
            skillPositionOnCollisionTriggered = default;
            visualToken = -1;
            
            affectedTargets.Clear();
            findTargetHitObjectsSet.Clear();
            actionsPacksDataList.Clear();

            spawnActions.Clear();
            isCollisionTriggered = false;
            isClickTriggered = false;

            followStrategy = null!;

            onSpawnNewSkillComponent = null;
        }

        public void OnGameLoopPhase(GameLoopPhase phase)
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

        public IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
        }

        private void OnPhysicsSkillMovementPhase()
        {
            if (shouldStop) return;
            
            UpdatePosition();
        }

        private void OnSkillUpdatePhase()
        {
            if (shouldStop) return;

            if (isCollisionTriggered)
            {
                foreach (var actionsPack in actionsPacksDataList)
                {
                    if (actionsPack.ActionTrigger is CollisionSkillActionTrigger)
                    {
                        ExecuteActions(actionsPack);
                    }
                }

                isCollisionTriggered = false;
            }

            if (isClickTriggered)
            {
                foreach (var actionsPack in actionsPacksDataList)
                {
                    if (actionsPack.ActionTrigger is ClickSkillActionTrigger)
                    {
                        ExecuteActions(actionsPack);
                    }
                }

                isClickTriggered = false;
            }

            if (!lifeTimer.IsRunning)
            {
                startSkillTick = objectContext.Runner.Tick;
                lifeTimer = TickTimer.CreateFromSeconds(objectContext.Runner, skillConfig.DurationSec);

                foreach (var actionsPack in actionsPacksDataList)
                {
                    if (actionsPack.ActionTrigger is TimerSkillActionTrigger timerTrigger)
                    {
                        triggerTimer = TickTimer.CreateFromSeconds(objectContext.Runner, timerTrigger.DelaySec);
                    }

                    if (actionsPack.ActionTrigger is StartSkillActionTrigger)
                    {
                        ExecuteActions(actionsPack);
                    }
                }
            }

            if (!lifeTimer.Expired(objectContext.Runner))
            {
                if (triggerTimer.Expired(objectContext.Runner))
                {
                    triggerTimer = default;
                    foreach (var actionsPack in actionsPacksDataList)
                    {
                        if (actionsPack.ActionTrigger is TimerSkillActionTrigger)
                        {
                            ExecuteActions(actionsPack);
                        }
                    }
                }

                CheckPeriodicTrigger();
            }

            if (lifeTimer.Expired(objectContext.Runner) || shouldStop)
            {
                lifeTimer = default;

                foreach (var actionsPack in actionsPacksDataList)
                {
                    if (actionsPack.ActionTrigger is FinishSkillActionTrigger)
                    {
                        ExecuteActions(actionsPack);
                    }
                }

                shouldStop = true;

                RefreshVisual(-1);
            }
        }

        private void OnPhysicsCheckCollisionsPhase()
        {
            if (shouldStop) return;
            
            CheckCollisionTrigger();
        }

        private void OnPhysicsSkillsLookPhase()
        {
            if (shouldStop) return;
            
            UpdateRotation();
        }

        private void OnDespawnPhase()
        {
            if (shouldStop && spawnActions.Count == 0)
            {
                OnReadyToRelease?.Invoke(this);
            }
        }

        public void UpdateMapPoint(Vector3 mapPoint)
        {
            if (shouldStop) return;
            
            dynamicMapPoint = mapPoint;
        }

        public void OnClickTrigger()
        {
            if (shouldStop) return;

            isClickTriggered = true;
        }

        public void TryInterrupt()
        {
            if (shouldStop) return;
            
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
                Position = GetPointByType(attachStrategy.AttachTo);
            }

            if (followStrategy is MoveToTargetSkillFollowStrategy moveToTargetStrategy)
            {
                var targetPoint = GetPointByType(moveToTargetStrategy.MoveTo);
                var deltaPosition = targetPoint - Position;
                var moveDelta = deltaPosition.normalized * moveToTargetStrategy.Speed * PhysicsManager.DeltaTime;

                if (deltaPosition.sqrMagnitude <= moveDelta.sqrMagnitude)
                {
                    Position = targetPoint;
                }
                else
                {
                    Position += moveDelta;
                }
            }

            if (followStrategy is MoveToDirectionSkillFollowStrategy moveToDirectionStrategy)
            {
                var direction = GetDirectionByType(moveToDirectionStrategy.MoveDirectionType);
                direction = Quaternion.AngleAxis(moveToDirectionStrategy.DirectionAngleOffset, Vector3.up) * direction;
                Position += direction * moveToDirectionStrategy.Speed * PhysicsManager.DeltaTime;
            }
        }

        private void UpdateRotation()
        {
            Rotation = Quaternion.LookRotation(GetDirectionByType(skillConfig.FollowDirectionType));
        }

        private IEnumerable<GameObject> FindActionTargets(List<SkillFindTargetsStrategyBase> findTargetStrategiesList)
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
            foreach (var actionsPack in actionsPacksDataList)
            {
                if (actionsPack.ActionTrigger is PeriodicSkillActionTrigger periodicTrigger
                    && TickHelper.CheckFrequency(objectContext.Runner.Tick - startSkillTick, objectContext.Runner.Simulation.Config.TickRate,
                        periodicTrigger.Frequency))
                {
                    ExecuteActions(actionsPack);
                }
            }
        }

        private void CheckCollisionTrigger()
        {
            foreach (var actionsPack in actionsPacksDataList)
            {
                if (actionsPack.ActionTrigger is CollisionSkillActionTrigger collisionTrigger)
                {
                    var hitsCount = Physics.OverlapSphereNonAlloc(
                        position: Position,
                        radius: collisionTrigger.Radius,
                        results: colliders,
                        layerMask: GetLayerMaskByType(collisionTrigger.TargetType) |
                                   collisionTrigger.TriggerByDecorationsLayer
                    );

                    if (!isCollisionTriggered &&
                        (hitsCount >= 2 || (hitsCount > 0 && colliders[0].gameObject != objectContext.gameObject)))
                    {
                        skillPositionOnCollisionTriggered = Position;
                        isCollisionTriggered = true;
                    }

                    //todo переделать структуру триггеров и стратегий поиска (с
                    break;
                }
            }
        }

        private void ExecuteActions(SkillActionsPackData actionsPackData)
        {
            var activatedTriggersCount = activatedTriggersCountMap[actionsPackData]++;

            var actionTargets = FindActionTargets(actionsPackData.FindTargetsStrategies);
            foreach (var action in actionsPackData.Actions)
            {
                ApplyAction(action, actionTargets, activatedTriggersCount);
            }
        }

        private void ApplyAction(SkillActionBase action, IEnumerable<GameObject> actionTargets, int activatedTriggersCount)
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
                            isCollisionTriggered ? skillPositionOnCollisionTriggered : Position;
                        var direction = actionTarget.transform.position - skillPosition;
                        var knockBackActionData = new KnockBackActionData { force = direction.normalized * forceAction.ForceValue };
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
                var spawnPosition = GetPointByType(spawnAction.SpawnPointType);
                var spawnRotation = Quaternion.LookRotation(GetDirectionByType(spawnAction.SpawnDirectionType));

                switch (spawnAction)
                {
                    case SpawnPrefabSkillAction spawnPrefabSkillAction:
                        var spawnedObject = objectContext.Runner.Spawn(
                            prefab: spawnPrefabSkillAction.PrefabToSpawn,
                            position: spawnPosition,
                            rotation: spawnRotation
                        );
                        if (spawnedObject.TryGetComponent<SkillNetworkTransform>(out var networkSkillTransform))
                        {
                            networkSkillTransform.FollowSkillComponent(this);
                        }
                        break;
                    case SpawnConfigSkillAction spawnConfigSkillAction:
                        var skillComponent = skillComponentsPoolHelper.Get();
                        skillComponent.Init(
                            skillConfigId: skillConfigsBank.GetSkillConfigId(spawnConfigSkillAction.SkillConfig),
                            objectContext: objectContext,
                            position: spawnPosition,
                            rotation: spawnRotation,
                            ownerId: ownerId,
                            initialMapPoint: initialMapPoint,
                            dynamicMapPoint: dynamicMapPoint,
                            selfUnit: objectContext,
                            selectedUnit: selectedUnit,
                            alliesLayerMask: alliesLayerMask,
                            opponentsLayerMask: opponentsLayerMask,
                            onSpawnNewSkillComponent: onSpawnNewSkillComponent
                        );
                        onSpawnNewSkillComponent?.Invoke(skillComponent);
                        break;
                    case SpawnSkillVisualAction spawnVisualForSkillAction:
                        var token = skillVisualManager.StartVisual(
                            spawnSkillConfig: spawnVisualForSkillAction,
                            spawnPosition: spawnPosition,
                            spawnDirection: GetForward()
                        );
                        RefreshVisual(token);
                        break;
                }
            }
            
            spawnActions.Clear();
        }

        private void RefreshVisual(int token)
        {
            if (visualToken >= 0)
            {
                skillVisualManager.FinishVisual(visualToken);
            }

            visualToken = token;
        }

        private Vector3 GetPointByType(SkillPointType pointType)
        {
            switch (pointType)
            {
                case SkillPointType.SkillPosition:
                    return Position;

                case SkillPointType.SelfUnitTarget:
                    return selfUnit != null ? selfUnit.transform.position : Position;

                case SkillPointType.SelectedUnitTarget:
                    return selectedUnit != null ? selectedUnit.transform.position : Position;

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
                    return GetForward();

                case SkillDirectionType.ToSelfUnit:
                    return selfUnit != null
                        ? (selfUnit.transform.position - Position).normalized
                        : GetForward();

                case SkillDirectionType.ToSelectedUnit:
                    return selectedUnit != null
                        ? (selectedUnit.transform.position - Position).normalized
                        : GetForward();

                case SkillDirectionType.ToInitialMapPoint:
                    return (initialMapPoint - Position).normalized;

                case SkillDirectionType.ToDynamicMapPoint:
                    return (dynamicMapPoint - Position).normalized;

                case SkillDirectionType.SelfUnitLookDirection:
                    return selfUnit != null ? selfUnit.transform.forward : GetForward();

                case SkillDirectionType.SelfUnitMoveDirection:
                    return selfUnit != null
                        ? selfUnit.GetInterface<Movable>()?.GetMovingDirection() ?? Vector3.zero
                        : GetForward();

                case SkillDirectionType.SelectedUnitLookDirection:
                    return selectedUnit != null ? selectedUnit.transform.forward : GetForward();

                case SkillDirectionType.SelectedUnitMoveDirection:
                    return selectedUnit != null
                        ? selectedUnit.GetInterface<Movable>()?.GetMovingDirection() ?? Vector3.zero
                        : GetForward();

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
                if (hit.gameObject == objectContext.gameObject) continue;

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
                    if (hit.gameObject == objectContext.gameObject) continue;

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
                if (hit.gameObject == objectContext.gameObject) continue;

                targetsSet.Add(hit.gameObject);
            }
        }

        private Vector3 GetForward()
        {
            return Rotation * Vector3.forward;
        }
    }
}