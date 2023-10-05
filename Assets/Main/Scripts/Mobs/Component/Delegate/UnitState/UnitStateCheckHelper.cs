using System;
using Fusion;
using Main.Scripts.Actions.Health;
using Main.Scripts.Mobs.Config.UnitState.Check;
using Main.Scripts.Utils;

namespace Main.Scripts.Mobs.Component.Delegate.UnitState
{
    public static class UnitStateCheckHelper
    {
        public static bool CheckState(NetworkObject unit, UnitStateCheckBase stateCheckConfig)
        {
            switch (stateCheckConfig)
            {
                case CurrentHealthStateCheck currentHealthStateCheck:
                    if (unit.TryGetInterface<HealthProvider>(out var healthProvider))
                    {
                        return (healthProvider.GetCurrentHealth() / healthProvider.GetMaxHealth()) * 100f <
                               currentHealthStateCheck.Percent;
                    }

                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stateCheckConfig), stateCheckConfig, null);
            }
        }
    }
}