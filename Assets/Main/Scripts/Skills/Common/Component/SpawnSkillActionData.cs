using System.Collections.Generic;
using Fusion;
using Main.Scripts.Skills.Common.Component.Config.Action;

namespace Main.Scripts.Skills.Common.Component
{
    public struct SpawnSkillActionData
    {
        public SpawnSkillActionBase spawnAction;
        public NetworkId selectedUnitId;
        public List<NetworkId> targetUnitIdsList;
    }
}