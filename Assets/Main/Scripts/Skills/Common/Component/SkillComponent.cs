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
using Main.Scripts.Skills.Charge;
using Main.Scripts.Skills.Common.Component.Config;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.ActionsPack;
using Main.Scripts.Skills.Common.Component.Config.Follow;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using Main.Scripts.Skills.Common.Component.Config.Trigger;
using Main.Scripts.Skills.Common.Component.Config.Value;
using Main.Scripts.Skills.Common.Component.Config.Value.Resolver;
using Main.Scripts.Skills.Common.Component.Visual;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Pool;

namespace Main.Scripts.Skills.Common.Component
{
    public class SkillComponent : GameLoopListener, SkillVariableProvider
    {
        private static readonly Type START_TRIGGER_TYPE = typeof(StartSkillActionTrigger);
        private static readonly Type FINISH_TRIGGER_TYPE = typeof(FinishSkillActionTrigger);
        private static readonly Type TIMER_TRIGGER_TYPE = typeof(TimerSkillActionTrigger);
        private static readonly Type PERIODIC_TRIGGER_TYPE = typeof(PeriodicSkillActionTrigger);
        private static readonly Type COLLISION_TRIGGER_TYPE = typeof(CollisionSkillActionTrigger);
        private static readonly Type CLICK_TRIGGER_TYPE = typeof(ClickSkillActionTrigger);

        private ModifierIdsBank modifierIdsBank = null!;
        private SkillHeatLevelManager skillHeatLevelManager = null!;
        private SkillVisualManager skillVisualManager = null!;
        private SkillComponentsPoolHelper skillComponentsPoolHelper = null!;
        private GameLoopManager gameLoopManager = null!;
        
        private SkillConfig skillConfig = null!;
        
        private NetworkRunner runner = null!;
        private PlayerRef ownerId;
        private int heatLevel;
        private int stackCount;
        private Vector3 initialMapPoint;
        private Vector3 dynamicMapPoint;
        private int powerChargeLevel;
        private int executionChargeLevel;
        private int executionChargeProgress;
        private NetworkId selfUnitId;
        private NetworkId selectedUnitId;
        private int selectedTargetEffectValue;
        private List<NetworkId> targetUnitIdsList = new();
        private Dictionary<NetworkId, int> targetEffectValues = new();
        private int alliesLayerMask;
        private int opponentsLayerMask;

        private int startSkillTick;
        private TickTimer lifeTimer;
        private int durationTicks;
        private bool continueRunningWhileHolding;
        private TickTimer triggerTimer;
        private int clicksCount;
        private Dictionary<Type, int> activatedTriggersCountMap = new();
        private bool shouldStop;

        private HashSet<CollisionTargetData> collisionDetectedTargets = new();
        private HashSet<CollisionTargetData> affectedCollisionTargets = new();

        private SkillFollowStrategyBase followStrategy = null!;
        private Dictionary<Type, SkillTriggerPackData> triggerPacksDataMap = new();

        private HashSet<GameObject> findTargetHitObjectsSet = new();
        private HashSet<List<NetworkId>> targetUnitIdsListsToRelease = new();

        private NetworkObject? selfUnit;
        private NetworkObject? selectedUnit;
        private List<SpawnSkillActionData> spawnActions = new();
        private RaycastHit[] raycasts = new RaycastHit[100];
        private Collider[] colliders = new Collider[100];
        private Vector3 skillPositionOnCollisionTriggered;
        private bool isCollisionTriggered;
        private bool isClickTriggered;
        private int visualToken = -1;

        private Listener? listener;
        public event Action<SkillComponent>? OnPrepareToRelease;
        public event Action<SkillComponent>? OnReadyToRelease;
        
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public bool IsDisableMove => skillConfig.DisableMoveWhileRunning; 

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
            SkillConfig skillConfig,
            NetworkRunner runner,
            Vector3 position,
            Quaternion rotation,
            PlayerRef ownerId,
            int heatLevel,
            int stackCount,
            Vector3 initialMapPoint,
            Vector3 dynamicMapPoint,
            int powerChargeLevel,
            int executionChargeLevel,
            int executionChargeProgress,
            int clicksCount,
            NetworkId selfUnitId,
            NetworkId selectedUnitId,
            int selectedTargetEffectValue,
            IEnumerable<NetworkId> targetUnitIdsList,
            Dictionary<NetworkId, int>? targetEffectValues,
            LayerMask alliesLayerMask,
            LayerMask opponentsLayerMask,
            Listener? listener
        )
        {
            this.runner = runner;
            Position = position;
            Rotation = rotation;
            this.ownerId = ownerId;
            this.heatLevel = heatLevel;
            this.stackCount = stackCount;
            this.initialMapPoint = initialMapPoint;
            this.dynamicMapPoint = dynamicMapPoint;
            this.powerChargeLevel = powerChargeLevel;
            this.executionChargeLevel = executionChargeLevel;
            this.executionChargeProgress = executionChargeProgress;
            this.clicksCount = clicksCount;
            this.selfUnitId = selfUnitId;
            this.selectedUnitId = selectedUnitId;
            this.selectedTargetEffectValue = selectedTargetEffectValue;
            this.alliesLayerMask = alliesLayerMask;
            this.targetUnitIdsList.AddRange(targetUnitIdsList);
            if (targetEffectValues != null)
            {
                foreach (var (id, value) in targetEffectValues)
                {
                    this.targetEffectValues[id] = value;
                }
            }
            this.opponentsLayerMask = opponentsLayerMask;
            this.listener = listener;

            var levelContext = LevelContext.Instance.ThrowWhenNull();
            skillComponentsPoolHelper = levelContext.SkillComponentsPoolHelper;
            gameLoopManager = levelContext.GameLoopManager;
            
            var resources = GlobalResources.Instance.ThrowWhenNull();
            modifierIdsBank = resources.ModifierIdsBank;
            skillHeatLevelManager = levelContext.SkillHeatLevelManager;
            skillVisualManager = levelContext.SkillVisualManager;

            this.skillConfig = skillConfig;

            var playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
            if (playerDataManager.HasUser(ownerId))
            {
                //StateAuthority is always local player
                var heroData = playerDataManager.GetLocalHeroData();
                ResolveConfigWithHeroData(ref heroData);
            }
            else
            {
                ResolveConfigWithoutHeroData();
            }

            if (skillConfig.ResetClicksCount)
            {
                this.clicksCount = 0;
            }

            levelContext.GameLoopManager.AddListener(this);
            
            listener?.OnSpawnNewSkillComponent(this);
        }

        private void ResolveConfigWithHeroData(ref HeroData heroData)
        {
            SkillFollowStrategyConfigsResolver.ResolveEnabledModifiers(
                modifierIdsBank,
                ref heroData,
                heatLevel,
                stackCount,
                powerChargeLevel,
                executionChargeLevel,
                clicksCount,
                skillConfig.FollowStrategy,
                out followStrategy
            );
            
            foreach (var skillTriggerPack in skillConfig.TriggerPacks)
            {
                SkillActionTriggerConfigsResolver.ResolveEnabledModifiers(
                    modifierIdsBank,
                    ref heroData,
                    heatLevel,
                    stackCount,
                    powerChargeLevel,
                    executionChargeLevel,
                    clicksCount,
                    skillTriggerPack.ActionTrigger,
                    out var resolvedActionTrigger
                );
                var triggerType = resolvedActionTrigger.GetType();

                var actionsPackList = ListPool<SkillActionsPackData>.Get();

                foreach (var skillActionsPack in skillTriggerPack.ActionsPackList)
                {
                    var resolvedDataOut = GenericPool<SkillActionsPackData>.Get();
                    resolvedDataOut.FindTargetsStrategies = ListPool<SkillFindTargetsStrategyBase>.Get();
                    resolvedDataOut.Actions = ListPool<SkillActionBase>.Get();

                    SkillActionsPackResolver.ResolveEnabledModifiers(
                        modifierIdsBank,
                        ref heroData,
                        heatLevel,
                        stackCount,
                        powerChargeLevel,
                        executionChargeLevel,
                        clicksCount,
                        skillActionsPack,
                        resolvedDataOut
                    );

                    actionsPackList.Add(resolvedDataOut);
                }

                activatedTriggersCountMap[triggerType] = 0;
                triggerPacksDataMap[triggerType] = new SkillTriggerPackData
                {
                    ActionTrigger = resolvedActionTrigger,
                    ActionsPackList = actionsPackList
                };
            }
        }

        private void ResolveConfigWithoutHeroData()
        {
            SkillFollowStrategyConfigsResolver.ResolveEnabledModifiers(
                stackCount,
                powerChargeLevel,
                executionChargeLevel,
                skillConfig.FollowStrategy,
                out followStrategy
            );
            
            foreach (var skillTriggerPack in skillConfig.TriggerPacks)
            {
                SkillActionTriggerConfigsResolver.ResolveEnabledModifiers(
                    stackCount,
                    powerChargeLevel,
                    executionChargeLevel,
                    skillTriggerPack.ActionTrigger,
                    out var resolvedActionTrigger
                );
                var triggerType = resolvedActionTrigger.GetType();

                var actionsPackList = ListPool<SkillActionsPackData>.Get();

                foreach (var skillActionsPack in skillTriggerPack.ActionsPackList)
                {
                    var resolvedDataOut = GenericPool<SkillActionsPackData>.Get();
                    resolvedDataOut.FindTargetsStrategies = ListPool<SkillFindTargetsStrategyBase>.Get();
                    resolvedDataOut.Actions = ListPool<SkillActionBase>.Get();

                    SkillActionsPackResolver.ResolveEnabledModifiers(
                        stackCount,
                        powerChargeLevel,
                        executionChargeLevel,
                        skillActionsPack,
                        resolvedDataOut
                    );

                    actionsPackList.Add(resolvedDataOut);
                }

                activatedTriggersCountMap[triggerType] = 0;
                triggerPacksDataMap[triggerType] = new SkillTriggerPackData
                {
                    ActionTrigger = resolvedActionTrigger,
                    ActionsPackList = actionsPackList
                };
            }
        }

        public void Release()
        {
            if (gameLoopManager != null)
            {
                gameLoopManager.RemoveListener(this);
            }

            foreach (var (_, triggerPackData) in triggerPacksDataMap)
            {
                foreach (var actionsPackData in triggerPackData.ActionsPackList)
                {
                    if (ownerId != default)
                    {
                        actionsPackData.Actions.Clear();
                        ListPool<SkillActionBase>.Release(actionsPackData.Actions);
                        actionsPackData.FindTargetsStrategies.Clear();
                        ListPool<SkillFindTargetsStrategyBase>.Release(actionsPackData.FindTargetsStrategies);
                    }
                    actionsPackData.Actions = null!;
                    actionsPackData.FindTargetsStrategies = null!;
                    
                    GenericPool<SkillActionsPackData>.Release(actionsPackData);
                }

                triggerPackData.ActionsPackList.Clear();
                ListPool<SkillActionsPackData>.Release(triggerPackData.ActionsPackList);
            }

            Reset();
        }

        private void Reset()
        {
            modifierIdsBank = null!;
            skillHeatLevelManager = null!;
            skillVisualManager = null!;
            skillComponentsPoolHelper = null!;
            gameLoopManager = null!;
            
            skillConfig = null!;
        
            runner = null!;
            selfUnitId = default;
            selectedUnitId = default;
            selfUnit = null;
            selectedUnit = null;
            targetUnitIdsList.Clear();
            targetEffectValues.Clear();
            
            
            lifeTimer = default;
            durationTicks = default;
            continueRunningWhileHolding = default;
            triggerTimer = default;
            activatedTriggersCountMap.Clear();
            collisionDetectedTargets.Clear();
            affectedCollisionTargets.Clear();
            shouldStop = false;
            startSkillTick = 0;
            skillPositionOnCollisionTriggered = default;
            visualToken = -1;
            
            findTargetHitObjectsSet.Clear();
            targetUnitIdsListsToRelease.Clear();
            triggerPacksDataMap.Clear();

            foreach (var data in spawnActions)
            {
                data.targetUnitIdsList.Clear();
                ListPool<NetworkId>.Release(data.targetUnitIdsList);
            }
            spawnActions.Clear();
            isCollisionTriggered = false;
            isClickTriggered = false;

            followStrategy = null!;

            listener = null;
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

            if (selfUnitId.IsValid)
            {
                selfUnit = runner.FindObject(selfUnitId);
            }

            if (selectedUnitId.IsValid)
            {
                selectedUnit = runner.FindObject(selectedUnitId);
                if (selectedUnit == null)
                {
                    shouldStop = true;
                }
            }

            if (isCollisionTriggered)
            {
                ExecuteTriggerPack(COLLISION_TRIGGER_TYPE);

                //reset trigger and targets after actions execution
                isCollisionTriggered = false;
                collisionDetectedTargets.Clear();
            }

            if (isClickTriggered)
            {
                ExecuteTriggerPack(CLICK_TRIGGER_TYPE);

                isClickTriggered = false;
            }

            if (!lifeTimer.IsRunning)
            {
                startSkillTick = runner.Tick;
                durationTicks = skillConfig.DurationTicks.Resolve(this).RoundToInt();
                lifeTimer = TickTimer.CreateFromTicks(runner, durationTicks);
                continueRunningWhileHolding = skillConfig.ContinueRunningWhileHolding;

                if (triggerPacksDataMap.TryGetValue(TIMER_TRIGGER_TYPE, out var triggerPackData)
                    && triggerPackData.ActionTrigger is TimerSkillActionTrigger timerTrigger)
                {
                    triggerTimer = TickTimer.CreateFromTicks(runner, timerTrigger.DelayTicks.Resolve(this).RoundToInt());
                }

                ExecuteTriggerPack(START_TRIGGER_TYPE);
            }

            if (!lifeTimer.Expired(runner))
            {
                if (triggerTimer.Expired(runner))
                {
                    triggerTimer = default;
                    ExecuteTriggerPack(TIMER_TRIGGER_TYPE);
                }

                CheckPeriodicTrigger();
            }

            var stopRunningByLackHolding = skillConfig.ContinueRunningWhileHolding && !continueRunningWhileHolding;
            continueRunningWhileHolding = false;

            if (lifeTimer.Expired(runner) || shouldStop || stopRunningByLackHolding)
            {
                lifeTimer = default;

                ExecuteTriggerPack(FINISH_TRIGGER_TYPE);

                shouldStop = true;

                RefreshVisual(-1);
            }
        }

        private void CheckPeriodicTrigger()
        {
            var ticksFromStart = runner.Tick - startSkillTick;

            if (triggerPacksDataMap.TryGetValue(PERIODIC_TRIGGER_TYPE, out var triggerPackData)
                && triggerPackData.ActionTrigger is PeriodicSkillActionTrigger periodicTrigger
                && ticksFromStart % periodicTrigger.FrequencyTicks.Resolve(this) == 0)
            {
                ExecuteTriggerPack(PERIODIC_TRIGGER_TYPE);
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
                OnPrepareToRelease?.Invoke(this);
                OnReadyToRelease?.Invoke(this);
            }
        }

        public void UpdateMapPoint(Vector3 mapPoint)
        {
            if (shouldStop) return;
            
            dynamicMapPoint = mapPoint;
        }

        public void ApplyHolding()
        {
            if (shouldStop || !skillConfig.ContinueRunningWhileHolding) return;
            
            continueRunningWhileHolding = true;
        }

        
        private int GetExecutionChargeLevel(int executionChargeProgress)
        {
            var level = 0;
            var executionChargeStepValues = skillConfig.ExecutionChargeStepValues;
            while (level < executionChargeStepValues.Length && executionChargeProgress >= executionChargeStepValues[level])
            {
                level++;
            }

            return level;
        }

        private int GetExecutionChargeProgress()
        {
            if (durationTicks == 0)
            {
                return 0;
            }
            return Math.Min(
                100,
                (int)(100 * (runner.Tick - startSkillTick) / (float)durationTicks)
            );
        }

        public void OnClickTrigger()
        {
            if (shouldStop) return;

            clicksCount++;
            isClickTriggered = true;
        }

        public void TryInterrupt()
        {
            if (shouldStop || !skillConfig.InterruptWithSkillController) return;

            shouldStop = true;
            RefreshVisual(-1);
        }

        public void OnLostControl()
        {
            listener = null;
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
                var moveDelta = deltaPosition.normalized * moveToTargetStrategy.Speed.Resolve(this) * PhysicsManager.DeltaTime;

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
                direction = Quaternion.AngleAxis(moveToDirectionStrategy.DirectionAngleOffset.Resolve(this), Vector3.up) * direction;
                Position += direction * moveToDirectionStrategy.Speed.Resolve(this) * PhysicsManager.DeltaTime;
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
                    case CollisionDetectedSkillFindTargetsStrategy:
                        foreach (var collisionDetectedTarget in collisionDetectedTargets)
                        {
                            if (collisionDetectedTarget.Id == collisionDetectedTarget.NetworkObject.Id)
                            {
                                findTargetHitObjectsSet.Add(collisionDetectedTarget.NetworkObject.gameObject);
                            }
                        }

                        break;
                    case TargetUnitsSkillFindTargetsStrategy:
                        foreach (var targetId in targetUnitIdsList)
                        {
                            var networkObject = runner.FindObject(targetId);
                            if (networkObject != null)
                            {
                                findTargetHitObjectsSet.Add(networkObject.gameObject);
                            }
                        }
                        break;
                }
            }

            return findTargetHitObjectsSet;
        }

        private void CheckCollisionTrigger()
        {
            if (triggerPacksDataMap.TryGetValue(COLLISION_TRIGGER_TYPE, out var triggerPackData)
                && triggerPackData.ActionTrigger is CollisionSkillActionTrigger collisionTrigger)
            {
                var hitsCount = Physics.OverlapSphereNonAlloc(
                    position: Position,
                    radius: collisionTrigger.Radius.Resolve(this),
                    results: colliders,
                    layerMask: GetLayerMaskByType(collisionTrigger.TargetType) |
                               collisionTrigger.TriggerByDecorationsLayer
                );

                var isDecorationHit = false;
                for (var i = 0; i < hitsCount; i++)
                {
                    var colliderObject = colliders[i].gameObject;
                    if (isSelfGameObject(colliderObject)) continue;
                    isDecorationHit |= (1 << colliderObject.layer & collisionTrigger.TriggerByDecorationsLayer) > 0;
                    
                    if (colliderObject.TryGetComponent<NetworkObject>(out var networkObject))
                    {
                        var targetData = new CollisionTargetData
                        {
                            Id = networkObject.Id,
                            NetworkObject = networkObject
                        };
                        if (!collisionTrigger.IsAffectTargetsOnlyOneTime)
                        {
                            collisionDetectedTargets.Add(targetData);
                        }
                        else if (!affectedCollisionTargets.Contains(targetData))
                        {
                            collisionDetectedTargets.Add(targetData);
                            affectedCollisionTargets.Add(targetData);
                        }
                    }
                }

                if (!isCollisionTriggered && (isDecorationHit || collisionDetectedTargets.Count > 0))
                {
                    skillPositionOnCollisionTriggered = Position;
                    isCollisionTriggered = true;
                }
            }
        }

        private void ExecuteTriggerPack(Type triggerType)
        {
            if (triggerPacksDataMap.TryGetValue(triggerType, out var triggerPackData))
            {
                var actionsPackList = triggerPackData.ActionsPackList;
                var activatedTriggersCount = activatedTriggersCountMap[triggerType]++;

                foreach (var actionsPack in actionsPackList)
                {
                    ExecuteActions(actionsPack, activatedTriggersCount);
                }
            }
        }

        private void ExecuteActions(SkillActionsPackData actionsPackData, int activatedTriggersCount)
        {
            var actionTargets = FindActionTargets(actionsPackData.FindTargetsStrategies);

            var targetIdsList = ListPool<NetworkId>.Get();
            foreach (var target in actionTargets)
            {
                if (target.TryGetComponent<NetworkObject>(out var networkObject))
                {
                    targetIdsList.Add(networkObject.Id);
                }
            }
            
            foreach (var action in actionsPackData.Actions)
            {
                ApplyAction(action, actionTargets, targetIdsList, activatedTriggersCount);
            }
        }

        private void ApplyAction(
            SkillActionBase action,
            IEnumerable<GameObject> actionTargets,
            List<NetworkId> targetIdsList,
            int activatedTriggersCount
        )
        {
            foreach (var actionTarget in actionTargets)
            {
                switch (action)
                {
                    case HeatChargeAction addChargeAction:
                        skillHeatLevelManager.AddCharge(addChargeAction.ChargeValue.Resolve(this, actionTarget).RoundToInt());
                        break;
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
                            damageValue = damageAction.DamageValue.Resolve(this, actionTarget)
                        };
                        damageable.AddDamage(ref damageActionData);

                        break;
                    case ForceSkillAction forceAction
                        when actionTarget.TryGetInterface(out ObjectWithGettingKnockBack knockable):

                        var skillPosition =
                            isCollisionTriggered ? skillPositionOnCollisionTriggered : Position;
                        var direction = actionTarget.transform.position - skillPosition;
                        var knockBackActionData = new KnockBackActionData { force = direction.normalized * forceAction.ForceValue.Resolve(this, actionTarget) };
                        knockable.AddKnockBack(ref knockBackActionData);

                        break;
                    case HealSkillAction healAction
                        when actionTarget.TryGetInterface<Healable>(out var healable):
                        var healActionData = new HealActionData
                        {
                            healOwner = selfUnit,
                            healValue = healAction.HealValue.Resolve(this, actionTarget)
                        };
                        healable.AddHeal(ref healActionData);

                        break;
                    case DashSkillAction dashAction
                        when actionTarget.TryGetInterface<Dashable>(out var dashable):

                        var directionByType = GetDirectionByType(dashAction.DirectionType);
                        var dashActionData = new DashActionData
                        {
                            direction = directionByType,
                            speed = dashAction.Speed.Resolve(this, actionTarget),
                            durationTicks = dashAction.DurationTicks.Resolve(this, actionTarget).RoundToInt()
                        };
                        dashable.AddDash(ref dashActionData);

                        break;
                    case StunSkillAction stunAction
                        when actionTarget.TryGetInterface(out ObjectWithGettingStun stunnable):
                        var stunActionData = new StunActionData
                        {
                            durationTicks = stunAction.DurationTicks.Resolve(this, actionTarget).RoundToInt()
                        };
                        stunnable.AddStun(ref stunActionData);

                        break;
                    case SpawnConfigWithTargetSkillAction spawnWithTargetAction:
                        if (actionTarget.TryGetComponent<NetworkObject>(out var networkObject))
                        {
                            spawnActions.Add(new SpawnSkillActionData
                            {
                                spawnAction = spawnWithTargetAction,
                                selectedUnitId = networkObject.Id,
                                targetUnitIdsList = targetIdsList
                            });
                        }

                        break;
                }
            }

            if (action is SpawnSkillActionBase spawnAction)
            {
                switch (action)
                {
                    case SpawnConfigSkillAction:
                    case SpawnPrefabSkillAction:
                    case SpawnSkillVisualAction:
                        spawnActions.Add(new SpawnSkillActionData
                        {
                            spawnAction = spawnAction,
                            selectedUnitId = selectedUnitId,
                            targetUnitIdsList = targetIdsList
                        });
                        break;
                }
            }

            if (action is StartOwnerAnimationSkillAction startAnimationSkillAction)
            {
                listener?.OnStartAnimationRequest(startAnimationSkillAction.AnimationIndex);
            }

            if (action is StopSkillAction stopAction && activatedTriggersCount >= stopAction.LiveUntilTriggersCount)
            {
                shouldStop = true;
                if (stopAction.StopControllerExecution)
                {
                    listener?.OnStopSkillExecutionRequest();
                }
            }
        }

        private void OnSpawnPhase()
        {
            var currentExecutionChargeProgress = skillConfig.StartNewExecutionCharging
                ? GetExecutionChargeProgress()
                : executionChargeProgress;
            
            var currentExecutionChargeLevel = skillConfig.StartNewExecutionCharging
                ? GetExecutionChargeLevel(currentExecutionChargeProgress)
                : executionChargeLevel;
            

            foreach (var spawnActionData in spawnActions)
            {
                var spawnAction = spawnActionData.spawnAction;
                var spawnPosition = GetPointByType(spawnAction.SpawnPointType);
                var spawnRotation = Quaternion.LookRotation(GetDirectionByType(spawnAction.SpawnDirectionType));

                switch (spawnAction)
                {
                    case SpawnPrefabSkillAction spawnPrefabSkillAction:
                        var spawnedObject = runner.Spawn(
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
                        SpawnSkillConfig(
                            skillConfig: spawnConfigSkillAction.SkillConfig,
                            spawnPosition: spawnPosition,
                            spawnRotation: spawnRotation,
                            executionChargeLevel: currentExecutionChargeLevel,
                            executionChargeProgress: currentExecutionChargeProgress,
                            clicksCount: clicksCount,
                            selectedUnitId: spawnActionData.selectedUnitId,
                            selectedTargetEffectValue: selectedTargetEffectValue,
                            targetUnitIdsList: spawnActionData.targetUnitIdsList,
                            targetEffectValues: targetEffectValues
                        );
                        break;
                    case SpawnConfigWithTargetSkillAction spawnConfigWithTargetSkillAction:
                        if (!targetEffectValues.TryGetValue(spawnActionData.selectedUnitId, out var effectValue))
                        {
                            effectValue = selectedTargetEffectValue;
                        }
                        SpawnSkillConfig(
                            skillConfig: spawnConfigWithTargetSkillAction.SkillConfig,
                            spawnPosition: spawnPosition,
                            spawnRotation: spawnRotation,
                            executionChargeLevel: currentExecutionChargeLevel,
                            executionChargeProgress: currentExecutionChargeProgress,
                            clicksCount: clicksCount,
                            selectedUnitId: spawnActionData.selectedUnitId,
                            selectedTargetEffectValue: effectValue,
                            targetUnitIdsList: spawnActionData.targetUnitIdsList,
                            targetEffectValues: null
                        );
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

                targetUnitIdsListsToRelease.Add(spawnActionData.targetUnitIdsList);
            }

            foreach (var targetsList in targetUnitIdsListsToRelease)
            {
                targetsList.Clear();
                ListPool<NetworkId>.Release(targetsList);
            }

            targetUnitIdsListsToRelease.Clear();
            spawnActions.Clear();
        }

        private void SpawnSkillConfig(
            SkillConfig skillConfig,
            Vector3 spawnPosition,
            Quaternion spawnRotation,
            int executionChargeLevel,
            int executionChargeProgress,
            int clicksCount,
            NetworkId selectedUnitId,
            int selectedTargetEffectValue,
            List<NetworkId> targetUnitIdsList,
            Dictionary<NetworkId, int>? targetEffectValues
        )
        {
            var skillComponent = skillComponentsPoolHelper.Get();

            skillComponent.Init(
                skillConfig: skillConfig,
                runner: runner,
                position: spawnPosition,
                rotation: spawnRotation,
                ownerId: ownerId,
                heatLevel: heatLevel,
                stackCount: stackCount,
                initialMapPoint: initialMapPoint,
                dynamicMapPoint: dynamicMapPoint,
                powerChargeLevel: powerChargeLevel,
                executionChargeLevel: executionChargeLevel,
                executionChargeProgress: executionChargeProgress,
                clicksCount: clicksCount,
                selfUnitId: selfUnitId,
                selectedUnitId: selectedUnitId,
                selectedTargetEffectValue: selectedTargetEffectValue,
                targetUnitIdsList: targetUnitIdsList,
                targetEffectValues: targetEffectValues,
                alliesLayerMask: alliesLayerMask,
                opponentsLayerMask: opponentsLayerMask,
                listener: listener
            );
        }

        private void RefreshVisual(int token)
        {
            if (visualToken >= 0)
            {
                skillVisualManager.FinishVisual(visualToken);
            }

            visualToken = token;
        }

        public Vector3 GetPointByType(SkillPointType pointType, GameObject? foundTarget = null)
        {
            switch (pointType)
            {
                case SkillPointType.SkillPosition:
                    return Position;

                case SkillPointType.SelfUnitTarget:
                    return selfUnit != null ? selfUnit.transform.position : Position;

                case SkillPointType.SelectedUnitTarget:
                    return selectedUnit != null ? selectedUnit.transform.position : Position;
                case SkillPointType.FoundUnitTarget:
                    return foundTarget != null ? foundTarget.transform.position : Position;
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
                radius: strategyConfig.Radius.Resolve(this),
                results: colliders,
                layerMask: GetLayerMaskByType(strategyConfig.TargetType),
                queryTriggerInteraction: QueryTriggerInteraction.Ignore
            );

            for (var i = 0; i < hitsCount; i++)
            {
                var hit = colliders[i];
                if (isSelfGameObject(hit.gameObject)) continue;

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
            direction = Quaternion.AngleAxis(strategyConfig.DirectionAngleOffset.Resolve(this), Vector3.up) * direction;

            var raycastsCount = Mathf.CeilToInt(strategyConfig.Angle.Resolve(this) / ANGLE_STEP) *
                                Mathf.CeilToInt(strategyConfig.Radius.Resolve(this) / DISTANCE_STEP_MULTIPLIER);

            for (var i = 0; i <= raycastsCount; i++)
            {
                var rotateAngle = -strategyConfig.Angle.Resolve(this) / 2f
                    + i * strategyConfig.Angle.Resolve(this) / raycastsCount;
                var raycastDirection = Quaternion.AngleAxis(rotateAngle, Vector3.up) * direction;

                var hitsCount = Physics.RaycastNonAlloc(
                    origin: origin,
                    direction: raycastDirection,
                    results: raycasts,
                    maxDistance: strategyConfig.Radius.Resolve(this),
                    layerMask: GetLayerMaskByType(strategyConfig.TargetType),
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore
                );

                for (var j = 0; j < hitsCount; j++)
                {
                    var hit = raycasts[j].collider;
                    if (isSelfGameObject(hit.gameObject)) continue;

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
            direction = Quaternion.AngleAxis(strategyConfig.DirectionAngleOffset.Resolve(this), Vector3.up) * direction;
            
            var originForwardOffset = strategyConfig.OriginForwardOffset.Resolve(this);

            var center = origin + direction * (originForwardOffset + strategyConfig.Length.Resolve(this) / 2f);

            var extents = new Vector3(strategyConfig.Width.Resolve(this) / 2f, 1f, strategyConfig.Length.Resolve(this) / 2f);

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
                if (isSelfGameObject(hit.gameObject)) continue;

                targetsSet.Add(hit.gameObject);
            }
        }
        
        private bool isSelfGameObject(GameObject hitGameObject)
        {
            return selfUnit != null && hitGameObject == selfUnit.gameObject;
        }

        private Vector3 GetForward()
        {
            return Rotation * Vector3.forward;
        }

        public int GetCommonValue(SkillCommonVariableType variableType, GameObject? target)
        {
            switch (variableType)
            {
                case SkillCommonVariableType.TakenEffectValue:
                    if (targetEffectValues.Count > 0
                        && target != null
                        && target.TryGetComponent<NetworkObject>(out var networkObject)
                        && targetEffectValues.TryGetValue(networkObject.Id, out var value)
                    )
                    {
                        return value;
                    }

                    return selectedTargetEffectValue;
                case SkillCommonVariableType.HeatLevel:
                    return heatLevel;
                case SkillCommonVariableType.PowerChargeLevel:
                    return powerChargeLevel;
                case SkillCommonVariableType.ExecutionChargeLevel:
                    return executionChargeLevel;
                case SkillCommonVariableType.ExecutionChargeProgress:
                    return executionChargeProgress;
                case SkillCommonVariableType.ClicksCount:
                    return clicksCount;
                case SkillCommonVariableType.StackCount:
                    return stackCount;
                default:
                    throw new ArgumentOutOfRangeException(nameof(variableType), variableType, null);
            }
        }

        public int GetTargetValue(SkillTargetVariableType variableType, SkillTargetType targetType, GameObject? foundTarget)
        {
            var target = targetType switch
            {
                SkillTargetType.SelfTarget => selfUnit != null ? selfUnit.gameObject : null,
                SkillTargetType.SelectedTarget => selectedUnit != null ? selectedUnit.gameObject : null,
                SkillTargetType.FoundTarget => foundTarget,
                _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
            };
            
            HealthProvider healthProvider;

            switch (variableType)
            {
                case SkillTargetVariableType.MaxHP:
                    if (target != null && target.TryGetInterface(out healthProvider))
                    {
                        return (int)healthProvider.GetMaxHealth();
                    }

                    return 0;
                case SkillTargetVariableType.CurrentHP:
                    if (target != null && target.TryGetInterface(out healthProvider))
                    {
                        return (int)healthProvider.GetCurrentHealth();
                    }

                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(variableType), variableType, null);
            }
        }

        private struct CollisionTargetData
        {
            public NetworkId Id;
            public NetworkObject NetworkObject;

            public override bool Equals(object obj)
            {
                if (obj is CollisionTargetData objData)
                {
                    return Id.Equals(objData.Id);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        public interface Listener
        {
            public void OnSpawnNewSkillComponent(SkillComponent skillComponent);

            public void OnStartAnimationRequest(int animationIndex);

            public void OnStopSkillExecutionRequest();
        }
    }
}