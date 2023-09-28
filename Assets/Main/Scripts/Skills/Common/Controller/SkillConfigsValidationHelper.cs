using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.Skills.Common.Component.Config;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using Main.Scripts.Skills.Common.Component.Config.Follow;

namespace Main.Scripts.Skills.Common.Controller
{
    public static class SkillConfigsValidationHelper
    {
        public static void Validate(SkillControllerConfig skillControllerConfig)
        {
            if (skillControllerConfig.CastDurationSec > skillControllerConfig.ExecutionDurationSec)
            {
                throw new Exception($"{skillControllerConfig.name}: ExecutionDurationSec must be bigger or equal CastDurationSec");
            }
            
            foreach (var skillConfig in skillControllerConfig.RunOnStartSkillConfigs)
            {
                CheckSkillConfig(skillConfig, skillControllerConfig.name);
            }
            
            foreach (var skillConfig in skillControllerConfig.RunAfterCastSkillConfigs)
            {
                CheckSkillConfig(skillConfig, skillControllerConfig.name);
            }
            
            if (skillControllerConfig.ActivationType != SkillActivationType.WithUnitTarget)
            {
                var configName = GetConfigNameThatUsedIllegalSelectedUnit(skillControllerConfig);
                if (configName != null)
                {
                    throw new Exception(
                        $"{skillControllerConfig.name}: using SelectedUnit in {configName} when ActivationType is not UnitTarget");

                }
            }
            
            if (skillControllerConfig.ActivationType != SkillActivationType.WithMapPointTarget)
            {
                var configName = GetConfigNameThatUsedIllegalInitialMapPoint(skillControllerConfig);
                if (configName != null)
                {
                    throw new Exception(
                        $"{skillControllerConfig.name}: using InitialMapPoint in {configName} when ActivationType is not MapPointTarget");

                }
            }
        }

        private static void CheckSkillConfig(SkillConfig? skillConfig, string skillControllerConfigName)
        {
            if (skillConfig == null)
            {
                throw new Exception($"{skillControllerConfigName}: SkillConfigs has empty value");
            }

            if (skillConfig.FollowStrategy == null)
            {
                throw new Exception($"{skillControllerConfigName}::{skillConfig.name}: FollowStrategy has empty value");
            }

            var triggersSet = new HashSet<Type>();
            foreach (var triggerPack in skillConfig.TriggerPacks)
            {
                if (triggerPack.ActionTrigger == null)
                {
                    throw new Exception($"{skillControllerConfigName}::{skillConfig.name}: ActionTrigger has empty value");
                }

                var triggerType = triggerPack.ActionTrigger.GetType();
                if (triggersSet.Contains(triggerType))
                {
                    throw new Exception($"{skillControllerConfigName}::{skillConfig.name}::{triggerPack.ActionTrigger}: ActionTrigger type is already registered");
                }

                foreach (var actionsPack in triggerPack.ActionsPackList)
                {
                    if (actionsPack.FindTargetsStrategies.Any(findTargetsStrategy => findTargetsStrategy == null))
                    {
                        throw new Exception($"{skillControllerConfigName}::{skillConfig.name}::{triggerPack.ActionTrigger}::{actionsPack.name}: FindTargetsStrategies has empty value");
                    }

                    if (actionsPack.Actions.Any(action => action == null))
                    {
                        throw new Exception($"{skillControllerConfigName}::{skillConfig.name}::{triggerPack.ActionTrigger}::{actionsPack.name}: Actions has empty value");
                    }
                }
            }
        }

        private static string? GetConfigNameThatUsedIllegalSelectedUnit(SkillControllerConfig skillControllerConfig)
        {
            foreach (var skillConfig in skillControllerConfig.RunAfterCastSkillConfigs)
            {
                if (skillConfig.SpawnPointType == SkillSpawnPointType.SelectedUnitTarget)
                {
                    return skillConfig.name;
                }

                if (skillConfig.SpawnDirectionType is SkillSpawnDirectionType.ToSelectedUnit
                    or SkillSpawnDirectionType.SelectedUnitLookDirection
                    or SkillSpawnDirectionType.SelectedUnitMoveDirection)
                {
                    return skillConfig.name;
                }

                if (IsPointOrDirectionUseSelectedUnit(null, skillConfig.FollowDirectionType))
                {
                    return skillConfig.name;
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
                                        return $"{skillConfig.name}::{actionsPack.name}::{aroundPoint.name}";
                                    }

                                    break;
                                case CircleSectorSkillFindTargetsStrategy circleSector:
                                    if (IsPointOrDirectionUseSelectedUnit(circleSector.OriginPoint,
                                            circleSector.DirectionType))
                                    {
                                        return $"{skillConfig.name}::{actionsPack.name}::{circleSector.name}";
                                    }

                                    break;
                                case RectangleSkillFindTargetsStrategy rectangle:
                                    if (IsPointOrDirectionUseSelectedUnit(rectangle.OriginPoint,
                                            rectangle.DirectionType))
                                    {
                                        return $"{skillConfig.name}::{actionsPack.name}::{rectangle.name}";
                                    }

                                    break;
                                case SelectedUnitSkillFindTargetsStrategy selectedUnitStrategy:
                                    return $"{skillConfig.name}::{actionsPack.name}::{selectedUnitStrategy.name}";
                            }
                        }

                        foreach (var skillAction in actionsPack.Actions)
                        {
                            switch (skillAction)
                            {
                                case DashSkillAction dashAction:
                                    if (IsPointOrDirectionUseSelectedUnit(null, dashAction.DirectionType))
                                    {
                                        return $"{skillConfig.name}::{actionsPack.name}::{dashAction.name}";
                                    }

                                    break;
                                case SpawnConfigSkillAction spawnAction:
                                    if (IsPointOrDirectionUseSelectedUnit(spawnAction.SpawnPointType,
                                            spawnAction.SpawnDirectionType))
                                    {
                                        return $"{skillConfig.name}::{actionsPack.name}::{spawnAction.name}";
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
                            return skillConfig.name + "::" + attachToFollow.name;
                        }

                        break;
                    case MoveToDirectionSkillFollowStrategy moveToDirection:
                        if (IsPointOrDirectionUseSelectedUnit(null, moveToDirection.MoveDirectionType))
                        {
                            return skillConfig.name + "::" + moveToDirection.name;
                        }

                        break;
                    case MoveToTargetSkillFollowStrategy moveToTarget:
                        if (IsPointOrDirectionUseSelectedUnit(moveToTarget.MoveTo, null))
                        {
                            return skillConfig.name + "::" + moveToTarget.name;
                        }

                        break;
                }
            }

            return null;
        }

        private static bool IsPointOrDirectionUseSelectedUnit(SkillPointType? skillPointType,
            SkillDirectionType? skillDirectionType)
        {
            return skillPointType == SkillPointType.SelectedUnitTarget
                   || skillDirectionType is SkillDirectionType.ToSelectedUnit
                       or SkillDirectionType.SelectedUnitLookDirection
                       or SkillDirectionType.SelectedUnitMoveDirection;
        }

        private static string? GetConfigNameThatUsedIllegalInitialMapPoint(SkillControllerConfig skillControllerConfig)
        {
            foreach (var skillConfig in skillControllerConfig.RunAfterCastSkillConfigs)
            {
                if (skillConfig.SpawnPointType == SkillSpawnPointType.InitialMapPointTarget)
                {
                    return skillConfig.name;
                }

                if (skillConfig.SpawnDirectionType == SkillSpawnDirectionType.ToInitialMapPoint)
                {
                    return skillConfig.name;
                }

                if (IsPointOrDirectionUseInitialMapPoint(null, skillConfig.FollowDirectionType))
                {
                    return skillConfig.name;
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
                                        return $"{skillConfig.name}::{actionsPack.name}::{aroundPoint.name}";
                                    }

                                    break;
                                case CircleSectorSkillFindTargetsStrategy circleSector:
                                    if (IsPointOrDirectionUseInitialMapPoint(circleSector.OriginPoint,
                                            circleSector.DirectionType))
                                    {
                                        return $"{skillConfig.name}::{actionsPack.name}::{circleSector.name}";
                                    }

                                    break;
                                case RectangleSkillFindTargetsStrategy rectangle:
                                    if (IsPointOrDirectionUseInitialMapPoint(rectangle.OriginPoint,
                                            rectangle.DirectionType))
                                    {
                                        return $"{skillConfig.name}::{actionsPack.name}::{rectangle.name}";
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
                                        return $"{skillConfig.name}::{actionsPack.name}::{dashAction.name}";
                                    }

                                    break;
                                case SpawnConfigSkillAction spawnAction:
                                    if (IsPointOrDirectionUseInitialMapPoint(spawnAction.SpawnPointType,
                                            spawnAction.SpawnDirectionType))
                                    {
                                        return $"{skillConfig.name}::{actionsPack.name}::{spawnAction.name}";
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
                            return $"{skillConfig.name}::{attachToFollow.name}";
                        }

                        break;
                    case MoveToDirectionSkillFollowStrategy moveToDirection:
                        if (IsPointOrDirectionUseInitialMapPoint(null, moveToDirection.MoveDirectionType))
                        {
                            return $"{skillConfig.name}::{moveToDirection.name}";
                        }

                        break;
                    case MoveToTargetSkillFollowStrategy moveToTarget:
                        if (IsPointOrDirectionUseInitialMapPoint(moveToTarget.MoveTo, null))
                        {
                            return $"{skillConfig.name}::{moveToTarget.name}";
                        }

                        break;
                }
            }

            return null;
        }

        private static bool IsPointOrDirectionUseInitialMapPoint(
            SkillPointType? skillPointType,
            SkillDirectionType? skillDirectionType)
        {
            return skillPointType == SkillPointType.InitialMapPointTarget
                   || skillDirectionType == SkillDirectionType.ToInitialMapPoint;
        }
    }
}