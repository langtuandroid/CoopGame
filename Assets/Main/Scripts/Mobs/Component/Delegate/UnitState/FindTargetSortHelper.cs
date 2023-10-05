using System;
using Fusion;
using Main.Scripts.Actions.Health;
using Main.Scripts.Mobs.Config.Block.FindTarget;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Mobs.Component.Delegate.UnitState
{
    public static class FindTargetSortHelper
    {
        public static int Compare(
            NetworkObject firstTarget,
            NetworkObject secondTarget,
            NetworkObject selfUnit,
            FindTargetSortType sortType
        )
        {
            switch (sortType)
            {
                case FindTargetSortType.Distance:
                    var firstDistance = Vector3.Distance(selfUnit.transform.position, firstTarget.transform.position);
                    var secondDistance = Vector3.Distance(selfUnit.transform.position, secondTarget.transform.position);
                    return firstDistance.CompareTo(secondDistance);
                case FindTargetSortType.CurrentHP:
                    var firstCurrentHP = firstTarget.GetInterface<HealthProvider>()?.GetCurrentHealth() ??
                                         throw new ArgumentNullException();
                    var secondCurrentHP = secondTarget.GetInterface<HealthProvider>()?.GetCurrentHealth() ??
                                          throw new ArgumentNullException();
                    return firstCurrentHP.CompareTo(secondCurrentHP);
                case FindTargetSortType.MaxHP:
                    var firstMaxHP = firstTarget.GetInterface<HealthProvider>()?.GetMaxHealth() ??
                                     throw new ArgumentNullException();
                    var secondMaxHP = secondTarget.GetInterface<HealthProvider>()?.GetMaxHealth() ??
                                      throw new ArgumentNullException();
                    return firstMaxHP.CompareTo(secondMaxHP);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null);
            }
        }
    }
}