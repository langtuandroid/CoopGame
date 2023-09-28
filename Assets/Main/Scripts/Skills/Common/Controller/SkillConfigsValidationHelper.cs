using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Scripts.Skills.Common.Component.Config;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using Main.Scripts.Skills.Common.Component.Config.Follow;
using Main.Scripts.Skills.Common.Component.Config.Trigger;

namespace Main.Scripts.Skills.Common.Controller
{
    public static class SkillConfigsValidationHelper
    {
        public static void Validate(SkillControllerConfig skillControllerConfig)
        {
            if (skillControllerConfig.CastDurationSec > skillControllerConfig.ExecutionDurationSec)
            {
                OnError(
                    "ExecutionDurationSec must be bigger or equal CastDurationSec",
                    skillControllerConfig.name
                );
            }
            
            foreach (var skillConfig in skillControllerConfig.RunOnCastSkillConfigs)
            {
                CheckSkillConfig(skillConfig, skillControllerConfig.name);
            }
            
            foreach (var skillConfig in skillControllerConfig.RunOnExecutionSkillConfigs)
            {
                CheckSkillConfig(skillConfig, skillControllerConfig.name);
            }
            
            if (skillControllerConfig.ActivationType != SkillActivationType.WithUnitTarget)
            {
                CheckUsingIllegalSelectedUnit(skillControllerConfig);
            }
            
            if (skillControllerConfig.ActivationType != SkillActivationType.WithMapPointTarget)
            {
                CheckUsingIllegalInitialMapPoint(skillControllerConfig);
            }
        }

        private static void CheckSkillConfig(SkillConfig? skillConfig, string skillControllerConfigName)
        {
            if (skillConfig == null)
            {
                OnError(
                    "SkillConfigs list has empty value",
                    skillControllerConfigName
                );
                return;
            }

            if (skillConfig.FollowStrategy == null)
            {
                OnError(
                    "FollowStrategy has empty value",
                    skillControllerConfigName,
                    skillConfig.name
                );
            }

            var triggersSet = new HashSet<Type>();
            foreach (var triggerPack in skillConfig.TriggerPacks)
            {
                if (triggerPack.ActionTrigger == null)
                {
                    OnError(
                        "TriggerPacks list has empty ActionTrigger value",
                        skillControllerConfigName,
                        skillConfig.name
                    );
                    return;
                }

                var triggerType = triggerPack.ActionTrigger.GetType();
                if (triggersSet.Contains(triggerType))
                {
                    OnError(
                        "ActionTrigger type is already registered in TriggerPacks",
                        skillControllerConfigName,
                        skillConfig.name,
                        triggerPack.ActionTrigger.name
                    );
                }

                triggersSet.Add(triggerType);

                foreach (var actionsPack in triggerPack.ActionsPackList)
                {
                    if (actionsPack.FindTargetsStrategies.Any(findTargetsStrategy => findTargetsStrategy == null))
                    {
                        OnError(
                            "FindTargetsStrategies list has empty value",
                            skillControllerConfigName,
                            skillConfig.name,
                            actionsPack.name
                        );
                    }

                    if (actionsPack.Actions.Any(action => action == null))
                    {
                        OnError(
                            "Actions list has empty value",
                            skillControllerConfigName,
                            skillConfig.name,
                            actionsPack.name
                        );
                    }

                    if (triggerPack.ActionTrigger is not CollisionSkillActionTrigger)
                    {
                        var collisionDetectedStrategy = actionsPack.FindTargetsStrategies.Find(findTargetsStrategy =>
                            findTargetsStrategy is CollisionDetectedSkillFindTargetsStrategy);
                        if (collisionDetectedStrategy != null)
                        {
                            OnError(
                                $"FindTargetsStrategy {collisionDetectedStrategy.name} has CollisionDetectedSkillFindTargetsStrategy type when ActionTrigger is not CollisionSkillActionTrigger",
                                skillControllerConfigName,
                                skillConfig.name,
                                actionsPack.name
                            );
                        }
                    }
                }
            }
        }

        private static void CheckUsingIllegalSelectedUnit(SkillControllerConfig skillControllerConfig)
        {
            var errorMessage = "Using SelectedUnit when ActivationType is not UnitTarget";
            
            foreach (var skillConfig in skillControllerConfig.RunOnExecutionSkillConfigs)
            {
                if (skillConfig.SpawnPointType == SkillSpawnPointType.SelectedUnitTarget
                    || IsPointOrDirectionUseSelectedUnit(null, skillConfig.FollowDirectionType))
                {
                    OnError(
                        errorMessage,
                        skillControllerConfig.name, 
                        skillConfig.name
                    );
                }

                if (skillConfig.SpawnDirectionType is SkillSpawnDirectionType.ToSelectedUnit
                    or SkillSpawnDirectionType.SelectedUnitLookDirection
                    or SkillSpawnDirectionType.SelectedUnitMoveDirection)
                {
                    OnError(
                        errorMessage,
                        skillControllerConfig.name, 
                        skillConfig.name
                    );
                }

                foreach (var triggerPack in skillConfig.TriggerPacks)
                {
                    foreach (var actionsPack in triggerPack.ActionsPackList)
                    {
                        foreach (var findTargetsStrategy in actionsPack.FindTargetsStrategies)
                        {
                            switch (findTargetsStrategy)
                            {
                                case AroundPointSkillFindTargetsStrategy aroundPoint:
                                    if (IsPointOrDirectionUseSelectedUnit(aroundPoint.OriginPoint, null))
                                    {
                                        OnError(
                                            errorMessage,
                                            skillControllerConfig.name, 
                                            skillConfig.name,
                                            actionsPack.name,
                                            aroundPoint.name
                                        );
                                    }

                                    break;
                                case CircleSectorSkillFindTargetsStrategy circleSector:
                                    if (IsPointOrDirectionUseSelectedUnit(circleSector.OriginPoint,
                                            circleSector.DirectionType))
                                    {
                                        OnError(
                                            errorMessage,
                                            skillControllerConfig.name, 
                                            skillConfig.name,
                                            actionsPack.name,
                                            circleSector.name
                                        );
                                    }

                                    break;
                                case RectangleSkillFindTargetsStrategy rectangle:
                                    if (IsPointOrDirectionUseSelectedUnit(rectangle.OriginPoint,
                                            rectangle.DirectionType))
                                    {
                                        OnError(
                                            errorMessage,
                                            skillControllerConfig.name, 
                                            skillConfig.name,
                                            actionsPack.name,
                                            rectangle.name
                                        );
                                    }

                                    break;
                                case SelectedUnitSkillFindTargetsStrategy selectedUnitStrategy:
                                    OnError(
                                        errorMessage,
                                        skillControllerConfig.name, 
                                        skillConfig.name,
                                        actionsPack.name,
                                        selectedUnitStrategy.name
                                    );
                                    break;
                            }
                        }

                        foreach (var skillAction in actionsPack.Actions)
                        {
                            switch (skillAction)
                            {
                                case DashSkillAction dashAction:
                                    if (IsPointOrDirectionUseSelectedUnit(null, dashAction.DirectionType))
                                    {
                                        OnError(
                                            errorMessage,
                                            skillControllerConfig.name, 
                                            skillConfig.name,
                                            actionsPack.name,
                                            dashAction.name
                                        );
                                    }

                                    break;
                                case SpawnConfigSkillAction spawnAction:
                                    if (IsPointOrDirectionUseSelectedUnit(spawnAction.SpawnPointType,
                                            spawnAction.SpawnDirectionType))
                                    {
                                        OnError(
                                            errorMessage,
                                            skillControllerConfig.name, 
                                            skillConfig.name,
                                            actionsPack.name,
                                            spawnAction.name
                                        );
                                    }

                                    break;
                            }
                        }
                    }
                }

                switch (skillConfig.FollowStrategy)
                {
                    case AttachToTargetSkillFollowStrategy attachToFollow:
                        if (IsPointOrDirectionUseSelectedUnit(attachToFollow.AttachTo, null))
                        {
                            OnError(
                                errorMessage,
                                skillControllerConfig.name, 
                                skillConfig.name,
                                attachToFollow.name
                            );
                        }

                        break;
                    case MoveToDirectionSkillFollowStrategy moveToDirection:
                        if (IsPointOrDirectionUseSelectedUnit(null, moveToDirection.MoveDirectionType))
                        {
                            OnError(
                                errorMessage,
                                skillControllerConfig.name, 
                                skillConfig.name,
                                moveToDirection.name
                            );
                        }

                        break;
                    case MoveToTargetSkillFollowStrategy moveToTarget:
                        if (IsPointOrDirectionUseSelectedUnit(moveToTarget.MoveTo, null))
                        {
                            OnError(
                                errorMessage,
                                skillControllerConfig.name, 
                                skillConfig.name,
                                moveToTarget.name
                            );
                        }

                        break;
                }
            }
        }

        private static bool IsPointOrDirectionUseSelectedUnit(SkillPointType? skillPointType,
            SkillDirectionType? skillDirectionType)
        {
            return skillPointType == SkillPointType.SelectedUnitTarget
                   || skillDirectionType is SkillDirectionType.ToSelectedUnit
                       or SkillDirectionType.SelectedUnitLookDirection
                       or SkillDirectionType.SelectedUnitMoveDirection;
        }

        private static void CheckUsingIllegalInitialMapPoint(SkillControllerConfig skillControllerConfig)
        {
            var errorMessage = "Using InitialMapPoint when ActivationType is not MapPointTarget";
            foreach (var skillConfig in skillControllerConfig.RunOnExecutionSkillConfigs)
            {
                if (skillConfig.SpawnPointType == SkillSpawnPointType.InitialMapPointTarget
                    || skillConfig.SpawnDirectionType == SkillSpawnDirectionType.ToInitialMapPoint
                    || IsPointOrDirectionUseInitialMapPoint(null, skillConfig.FollowDirectionType))
                {
                    OnError(
                        errorMessage,
                        skillControllerConfig.name, 
                        skillConfig.name
                    );
                }

                foreach (var triggerPack in skillConfig.TriggerPacks)
                {
                    foreach (var actionsPack in triggerPack.ActionsPackList)
                    {
                        foreach (var findTargetsStrategy in actionsPack.FindTargetsStrategies)
                        {
                            switch (findTargetsStrategy)
                            {
                                case AroundPointSkillFindTargetsStrategy aroundPoint:
                                    if (IsPointOrDirectionUseInitialMapPoint(aroundPoint.OriginPoint, null))
                                    {
                                        OnError(
                                            errorMessage,
                                            skillControllerConfig.name,
                                            skillConfig.name,
                                            actionsPack.name,
                                            aroundPoint.name
                                        );
                                    }

                                    break;
                                case CircleSectorSkillFindTargetsStrategy circleSector:
                                    if (IsPointOrDirectionUseInitialMapPoint(circleSector.OriginPoint,
                                            circleSector.DirectionType))
                                    {
                                        OnError(
                                            errorMessage,
                                            skillControllerConfig.name,
                                            skillConfig.name,
                                            actionsPack.name,
                                            circleSector.name
                                        );
                                    }

                                    break;
                                case RectangleSkillFindTargetsStrategy rectangle:
                                    if (IsPointOrDirectionUseInitialMapPoint(rectangle.OriginPoint,
                                            rectangle.DirectionType))
                                    {
                                        OnError(
                                            errorMessage,
                                            skillControllerConfig.name,
                                            skillConfig.name,
                                            actionsPack.name,
                                            rectangle.name
                                        );
                                    }

                                    break;
                            }
                        }

                        foreach (var skillAction in actionsPack.Actions)
                        {
                            switch (skillAction)
                            {
                                case DashSkillAction dashAction:
                                    if (IsPointOrDirectionUseInitialMapPoint(null, dashAction.DirectionType))
                                    {
                                        OnError(
                                            errorMessage,
                                            skillControllerConfig.name,
                                            skillConfig.name,
                                            actionsPack.name,
                                            dashAction.name
                                        );
                                    }

                                    break;
                                case SpawnConfigSkillAction spawnAction:
                                    if (IsPointOrDirectionUseInitialMapPoint(spawnAction.SpawnPointType,
                                            spawnAction.SpawnDirectionType))
                                    {
                                        OnError(
                                            errorMessage,
                                            skillControllerConfig.name,
                                            skillConfig.name,
                                            actionsPack.name,
                                            spawnAction.name
                                        );
                                    }

                                    break;
                            }
                        }
                    }
                }

                switch (skillConfig.FollowStrategy)
                {
                    case AttachToTargetSkillFollowStrategy attachToFollow:
                        if (IsPointOrDirectionUseInitialMapPoint(attachToFollow.AttachTo, null))
                        {
                            OnError(
                                errorMessage,
                                skillControllerConfig.name,
                                skillConfig.name,
                                attachToFollow.name
                            );
                        }

                        break;
                    case MoveToDirectionSkillFollowStrategy moveToDirection:
                        if (IsPointOrDirectionUseInitialMapPoint(null, moveToDirection.MoveDirectionType))
                        {
                            OnError(
                                errorMessage,
                                skillControllerConfig.name,
                                skillConfig.name,
                                moveToDirection.name
                            );
                        }

                        break;
                    case MoveToTargetSkillFollowStrategy moveToTarget:
                        if (IsPointOrDirectionUseInitialMapPoint(moveToTarget.MoveTo, null))
                        {
                            OnError(
                                errorMessage,
                            skillControllerConfig.name,
                                skillConfig.name,
                                moveToTarget.name
                            );
                        }

                        break;
                }
            }
        }

        private static bool IsPointOrDirectionUseInitialMapPoint(
            SkillPointType? skillPointType,
            SkillDirectionType? skillDirectionType)
        {
            return skillPointType == SkillPointType.InitialMapPointTarget
                   || skillDirectionType == SkillDirectionType.ToInitialMapPoint;
        }

        private static void OnError(string message, params string[] path)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < path.Length; i++)
            {
                builder.Append(path[i]);
                builder.Append(i == path.Length - 1 ? "::" : " - ");
            }

            builder.Append(message);

            throw new Exception(builder.ToString());
        }
    }
}