using System;
using Fusion;
using Main.Scripts.Mobs.Component.Delegate.FindTarget.Holder;
using Main.Scripts.Mobs.Component.Delegate.UnitState;
using Main.Scripts.Mobs.Config.Block.FindTarget;
using Main.Scripts.Player.InputSystem.Target;
using UnityEngine;

namespace Main.Scripts.Mobs.Component.Delegate.FindTarget
{
    public class FindTargetBlockDelegate : MobBlockDelegate
    {
        private FindTargetMobBlockConfig findTargetConfig = null!;
        private FindTargetHolderDelegate? holderDelegate;
        private MobBlockDelegate continueWithTargetContextBlockDelegate = null!;

        private Collider[] colliders = new Collider[100];

        public void Init(FindTargetMobBlockConfig findTargetConfig)
        {
            this.findTargetConfig = findTargetConfig;
            if (findTargetConfig.FindTargetHolderConfig != null)
            {
                holderDelegate = FindTargetHolderDelegateHelper.Create(findTargetConfig.FindTargetHolderConfig);
            }
            continueWithTargetContextBlockDelegate =
                MobBlockDelegateHelper.Create(findTargetConfig.ContinueWithTargetContextBlock);
        }

        public void Do(ref MobBlockContext context, out MobBlockResult blockResult)
        {
            var target = holderDelegate?.GetHoldTarget(ref context);
            if (target == null)
            {
                var hitsCount = Physics.OverlapSphereNonAlloc(
                    position: context.SelfUnit.transform.position,
                    radius: findTargetConfig.SearchRadius,
                    results: colliders,
                    layerMask: GetLayerMaskByType(ref context, findTargetConfig.TargetType)
                );

                for (var i = 0; i < hitsCount; i++)
                {
                    var colliderObject = colliders[i].gameObject;
                    if (!colliderObject.TryGetComponent<NetworkObject>(out var networkObject))
                    {
                        continue;
                    }

                    if (findTargetConfig.StateCheckFilter != null
                        && !UnitStateCheckHelper.CheckState(networkObject, findTargetConfig.StateCheckFilter))
                    {
                        continue;
                    }

                    var signOfDesc = findTargetConfig.OrderByDesc ? -1 : 1;
                    if (target == null
                        || FindTargetSortHelper.Compare(
                            networkObject,
                            target,
                            context.SelfUnit,
                            findTargetConfig.SortType) * signOfDesc < 0
                       )
                    {
                        target = networkObject;
                    }
                }
                
                if (target != null)
                {
                    holderDelegate?.SetTarget(target);
                }
            }

            var newContext = context;
            newContext.TargetUnit = target;
            continueWithTargetContextBlockDelegate.Do(ref newContext, out blockResult);
        }

        public void Reset()
        {
            if (holderDelegate != null)
            {
                FindTargetHolderDelegateHelper.Release(holderDelegate);
            }
            MobBlockDelegateHelper.Release(continueWithTargetContextBlockDelegate);

            findTargetConfig = null!;
            holderDelegate = null;
            continueWithTargetContextBlockDelegate = null!;
        }

        private LayerMask GetLayerMaskByType(ref MobBlockContext context, UnitTargetType targetType)
        {
            return targetType switch
            {
                UnitTargetType.Allies => context.AlliesLayerMask,
                UnitTargetType.Opponents => context.OpponentsLayerMask,
                UnitTargetType.All => context.AlliesLayerMask | context.OpponentsLayerMask,
                _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
            };
        }
    }
}